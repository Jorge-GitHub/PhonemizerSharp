namespace PhonemizerSharp.Domain.Settings.Providers;

public sealed class EspeakNgProviderSettings
{
    public string ExecutablePath { get; set; } = "espeak-ng";
    public string NativeLibraryPath { get; set; } = "";
    public string DataPath { get; set; } = "";
    public string DefaultLanguage { get; set; } = "en-us";
    public int IpaMode { get; set; } = 3;
    public int TimeoutInSeconds { get; set; } = 30;
    public bool Quiet { get; set; } = true;
}
