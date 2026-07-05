using PhonemizerSharp.Domain.Phonemes.Enums;
using PhonemizerSharp.Domain.Settings.Enums;

namespace PhonemizerSharp.Domain.Results;

public sealed class PhonemizerResult
{
    public string Text { get; set; } = "";
    public string Phonemes { get; set; } = "";
    public string Language { get; set; } = "";
    public PhonemeAlphabet Alphabet { get; set; } = PhonemeAlphabet.Ipa;
    public PhonemizerProvider Provider { get; set; } =
        PhonemizerProvider.EspeakNg;
    public PhonemizerRuntime Runtime { get; set; } =
        PhonemizerRuntime.Auto;
    public string ProviderName { get; set; } = "";
}
