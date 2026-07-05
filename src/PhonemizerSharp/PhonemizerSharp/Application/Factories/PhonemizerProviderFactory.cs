using PhonemizerSharp.Application.Contracts;
using PhonemizerSharp.Application.Providers.EspeakNgNativeService;
using PhonemizerSharp.Application.Providers.EspeakNgService;
using PhonemizerSharp.Domain.Settings;
using PhonemizerSharp.Domain.Settings.Enums;

namespace PhonemizerSharp.Application.Factories;

public sealed class PhonemizerProviderFactory
{
    public IPhonemizer Create(PhonemizerSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        return settings.Provider switch
        {
            PhonemizerProvider.EspeakNg =>
                this.CreateEspeakNg(settings),
            _ => throw new NotSupportedException(
                $"Phonemizer provider '{settings.Provider}' is not supported.")
        };
    }

    private IPhonemizer CreateEspeakNg(PhonemizerSettings settings)
    {
        return settings.Runtime switch
        {
            PhonemizerRuntime.Auto =>
                new EspeakNgNativePhonemizer(settings.EspeakNg),
            PhonemizerRuntime.NativeLibrary =>
                new EspeakNgNativePhonemizer(settings.EspeakNg),
            PhonemizerRuntime.Process =>
                new EspeakNgProcessPhonemizer(settings.EspeakNg),
            _ => throw new NotSupportedException(
                $"Runtime '{settings.Runtime}' is not supported for eSpeak NG.")
        };
    }
}
