using PhonemizerSharp.Domain.Phonemes.Enums;

namespace PhonemizerSharp.Domain.Requests;

public sealed class PhonemizerRequest
{
    public string Text { get; set; } = "";
    public string Language { get; set; } = "";
    public PhonemeAlphabet Alphabet { get; set; } = PhonemeAlphabet.Ipa;
}
