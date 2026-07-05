using PhonemizerSharp.Application.Providers.Base;
using PhonemizerSharp.Domain.Phonemes.Enums;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Results;
using PhonemizerSharp.Domain.Settings.Enums;
using PhonemizerSharp.Domain.Settings.Providers;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace PhonemizerSharp.Application.Providers.EspeakNgService;

public sealed class EspeakNgProcessPhonemizer : PhonemizerProviderBase
{
    private readonly EspeakNgProviderSettings settings;

    public EspeakNgProcessPhonemizer() : this(new EspeakNgProviderSettings()) { }

    public EspeakNgProcessPhonemizer(EspeakNgProviderSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        this.settings = settings;
    }

    public override async Task<PhonemizerResult> PhonemizeAsync(
        PhonemizerRequest request,
        CancellationToken cancellationToken = default)
    {
        base.ValidateRequest(request);

        string text = base.NormalizeText(request.Text);
        string language = base.ResolveLanguage(
            request.Language, this.settings.DefaultLanguage);
        ProcessStartInfo startInfo = this.CreateStartInfo(
            text, language, request.Alphabet);

        using Process process = new()
        {
            StartInfo = startInfo
        };

        this.StartProcess(process);

        using CancellationTokenSource timeoutSource =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        if (this.settings.TimeoutInSeconds > 0)
        {
            timeoutSource.CancelAfter(
                TimeSpan.FromSeconds(this.settings.TimeoutInSeconds));
        }

        string output;
        string error;
        try
        {
            Task<string> outputTask =
                process.StandardOutput.ReadToEndAsync(timeoutSource.Token);
            Task<string> errorTask =
                process.StandardError.ReadToEndAsync(timeoutSource.Token);

            await process.WaitForExitAsync(timeoutSource.Token)
                .ConfigureAwait(false);

            output = await outputTask.ConfigureAwait(false);
            error = await errorTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException exception)
            when (!cancellationToken.IsCancellationRequested)
        {
            this.TryKill(process);
            throw new TimeoutException(
                "eSpeak NG timed out while phonemizing text.",
                exception);
        }

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                "eSpeak NG failed while phonemizing text. "
                + $"Exit code: {process.ExitCode}. Error: {error}");
        }

        string phonemes = output.Trim();
        if (string.IsNullOrEmpty(phonemes))
        {
            throw new InvalidOperationException(
                "eSpeak NG returned no phonemes for the supplied text.");
        }

        return new PhonemizerResult
        {
            Text = text,
            Phonemes = phonemes,
            Language = language,
            Alphabet = request.Alphabet,
            Provider = PhonemizerProvider.EspeakNg,
            Runtime = PhonemizerRuntime.Process,
            ProviderName = "eSpeak NG"
        };
    }

    private ProcessStartInfo CreateStartInfo(
        string text,
        string language,
        PhonemeAlphabet alphabet)
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = this.ResolveExecutablePath(),
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            StandardErrorEncoding = Encoding.UTF8,
            StandardOutputEncoding = Encoding.UTF8,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (this.settings.Quiet)
        {
            startInfo.ArgumentList.Add("-q");
        }

        if (alphabet == PhonemeAlphabet.Ipa)
        {
            startInfo.ArgumentList.Add($"--ipa={this.ResolveIpaMode()}");
        }

        startInfo.ArgumentList.Add("-v");
        startInfo.ArgumentList.Add(language);
        startInfo.ArgumentList.Add(text);

        return startInfo;
    }

    private void StartProcess(Process process)
    {
        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException(
                    "eSpeak NG could not be started.");
            }
        }
        catch (Win32Exception exception)
        {
            throw new InvalidOperationException(
                "The eSpeak NG executable was not found. "
                + "Install eSpeak NG or set EspeakNgProviderSettings.ExecutablePath.",
                exception);
        }
    }

    private string ResolveExecutablePath()
    {
        return !string.IsNullOrWhiteSpace(this.settings.ExecutablePath)
            ? this.settings.ExecutablePath.Trim()
            : "espeak-ng";
    }

    private int ResolveIpaMode()
    {
        return this.settings.IpaMode > 0
            ? this.settings.IpaMode
            : 3;
    }

    private void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch
        {
            // Best effort cleanup after timeout.
        }
    }
}
