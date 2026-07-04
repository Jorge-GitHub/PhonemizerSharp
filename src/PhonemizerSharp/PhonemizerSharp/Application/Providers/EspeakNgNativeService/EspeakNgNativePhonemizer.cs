using PhonemizerSharp.Application.Providers.Base;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Results;
using PhonemizerSharp.Domain.Settings.Enums;
using PhonemizerSharp.Domain.Settings.Providers;

namespace PhonemizerSharp.Application.Providers.EspeakNgNativeService;

public sealed class EspeakNgNativePhonemizer : PhonemizerProviderBase, IDisposable
{
    private static readonly object SyncRoot = new();

    private readonly EspeakNgProviderSettings settings;
    private EspeakNgNativeLibrary? library;
    private bool disposed;

    public EspeakNgNativePhonemizer() : this(new EspeakNgProviderSettings()) { }

    public EspeakNgNativePhonemizer(EspeakNgProviderSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        this.settings = settings;
    }

    public override Task<PhonemizerResult> PhonemizeAsync(
        PhonemizerRequest request,
        CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(this.disposed, this);
        base.ValidateRequest(request);
        cancellationToken.ThrowIfCancellationRequested();

        string text = base.NormalizeText(request.Text);
        string language = base.ResolveLanguage(
            request.Language, this.settings.DefaultLanguage);

        string phonemes;
        lock (SyncRoot)
        {
            EspeakNgNativeLibrary nativeLibrary = this.GetLibrary();
            nativeLibrary.SetVoice(language);
            phonemes = nativeLibrary.TextToPhonemes(
                text,
                this.ResolveIpaMode());
        }

        if (string.IsNullOrEmpty(phonemes))
        {
            throw new InvalidOperationException(
                "Native eSpeak NG returned no phonemes for the supplied text.");
        }

        return Task.FromResult(new PhonemizerResult
        {
            Text = text,
            Phonemes = phonemes,
            Language = language,
            Alphabet = request.Alphabet,
            Provider = PhonemizerProvider.EspeakNg,
            Runtime = PhonemizerRuntime.NativeLibrary,
            ProviderName = "eSpeak NG Native"
        });
    }

    public void Dispose()
    {
        if (!this.disposed)
        {
            this.library?.Dispose();
            this.disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    private EspeakNgNativeLibrary GetLibrary()
    {
        if (this.library is null)
        {
            try
            {
                this.library = new EspeakNgNativeLibrary(
                    this.ResolveNativeLibraryPath());
                this.library.Initialize(this.ResolveDataPath());
            }
            catch (Exception exception)
                when (exception is DllNotFoundException
                    or EntryPointNotFoundException
                    or BadImageFormatException)
            {
                throw new InvalidOperationException(
                    "The native eSpeak NG library was not found or could not be loaded. "
                    + "Install libespeak-ng, or set EspeakNgProviderSettings.NativeLibraryPath.",
                    exception);
            }
        }

        return this.library;
    }

    private string ResolveNativeLibraryPath()
    {
        if (!string.IsNullOrWhiteSpace(this.settings.NativeLibraryPath))
        {
            return this.settings.NativeLibraryPath.Trim();
        }

        string libraryPath = Path.Combine(
            this.ResolveBundledNativeFolder(),
            "libespeak-ng.dll");

        return File.Exists(libraryPath)
            ? libraryPath
            : "";
    }

    private string ResolveDataPath()
    {
        if (!string.IsNullOrWhiteSpace(this.settings.DataPath))
        {
            return this.ResolveEspeakDataHomePath(
                this.settings.DataPath.Trim());
        }

        string nativeFolder = this.ResolveBundledNativeFolder();
        string dataPath = Path.Combine(nativeFolder, "espeak-ng-data");

        return Directory.Exists(dataPath)
            ? nativeFolder
            : "";
    }

    private string ResolveEspeakDataHomePath(string dataPath)
    {
        string normalizedPath = dataPath.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        if (Path.GetFileName(normalizedPath).Equals(
            "espeak-ng-data",
            StringComparison.OrdinalIgnoreCase))
        {
            return Directory.GetParent(normalizedPath)?.FullName ?? normalizedPath;
        }

        return normalizedPath;
    }

    private string ResolveBundledNativeFolder()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "Application",
            "Providers",
            "EspeakNgNativeService",
            "Libraries",
            "Native",
            "Win-x64");
    }

    private int ResolveIpaMode()
    {
        return this.settings.IpaMode > 0
            ? this.settings.IpaMode
            : 3;
    }
}
