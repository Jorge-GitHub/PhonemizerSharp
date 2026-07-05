using PhonemizerSharp.Domain.Settings.Enums;
using PhonemizerSharp.Domain.Settings.Providers;

namespace PhonemizerSharp.Domain.Settings;

public sealed class PhonemizerSettings
{
    public PhonemizerProvider Provider { get; set; } =
        PhonemizerProvider.EspeakNg;
    public PhonemizerRuntime Runtime { get; set; } =
        PhonemizerRuntime.Auto;
    public EspeakNgProviderSettings EspeakNg { get; set; } = new();
}
