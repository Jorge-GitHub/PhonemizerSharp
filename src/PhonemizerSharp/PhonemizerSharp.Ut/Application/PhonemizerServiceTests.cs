using PhonemizerSharp.Application;
using PhonemizerSharp.Application.Contracts;
using PhonemizerSharp.Domain.Phonemes.Enums;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Results;
using PhonemizerSharp.Domain.Settings.Enums;

namespace PhonemizerSharp.Ut.Application;

[TestClass]
public sealed class PhonemizerServiceTests
{
    [TestMethod]
    public async Task PhonemizeAsync_DelegatesToProvider()
    {
        FakePhonemizer phonemizer = new();
        PhonemizerService service = new(phonemizer);
        PhonemizerRequest request = new()
        {
            Text = "Hello",
            Language = "en-gb"
        };

        PhonemizerResult result =
            await service.PhonemizeAsync(request);

        Assert.AreEqual(1, phonemizer.CallCount);
        Assert.AreSame(request, phonemizer.LastRequest);
        Assert.AreEqual("hello", result.Phonemes);
        Assert.AreEqual("en-gb", result.Language);
    }

    private sealed class FakePhonemizer : IPhonemizer
    {
        public int CallCount { get; private set; }
        public PhonemizerRequest? LastRequest { get; private set; }

        public Task<PhonemizerResult> PhonemizeAsync(
            PhonemizerRequest request,
            CancellationToken cancellationToken = default)
        {
            this.CallCount++;
            this.LastRequest = request;

            return Task.FromResult(new PhonemizerResult
            {
                Text = request.Text,
                Phonemes = "hello",
                Language = request.Language,
                Alphabet = PhonemeAlphabet.Ipa,
                Provider = PhonemizerProvider.EspeakNg,
                Runtime = PhonemizerRuntime.Managed,
                ProviderName = "Fake"
            });
        }
    }
}
