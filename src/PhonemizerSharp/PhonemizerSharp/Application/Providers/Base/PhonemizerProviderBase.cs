using PhonemizerSharp.Application.Contracts;
using PhonemizerSharp.Domain.Phonemes.Enums;
using PhonemizerSharp.Domain.Requests;

namespace PhonemizerSharp.Application.Providers.Base;

public abstract class PhonemizerProviderBase : IPhonemizer
{
    public abstract Task<Domain.Results.PhonemizerResult> PhonemizeAsync(
        PhonemizerRequest request,
        CancellationToken cancellationToken = default);

    protected void ValidateRequest(PhonemizerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Text))
        {
            throw new ArgumentException(
                "Text is required for phonemization.",
                nameof(request));
        }

        if (request.Alphabet != PhonemeAlphabet.Ipa)
        {
            throw new NotSupportedException(
                $"Phoneme alphabet '{request.Alphabet}' is not supported.");
        }
    }

    protected string NormalizeText(string text)
    {
        return text.Trim();
    }

    protected string ResolveLanguage(string language, string defaultLanguage)
    {
        string value = !string.IsNullOrWhiteSpace(language)
            ? language.Trim() : (defaultLanguage ?? "").Trim();

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                "A phonemizer language or provider default language is required.");
        }

        return value.ToLowerInvariant();
    }
}
