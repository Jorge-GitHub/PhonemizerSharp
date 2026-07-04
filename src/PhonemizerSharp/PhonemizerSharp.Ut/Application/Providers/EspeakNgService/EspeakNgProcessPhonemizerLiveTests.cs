using PhonemizerSharp.Application.Providers.EspeakNgService;
using PhonemizerSharp.Domain.Phonemes.Enums;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Results;
using PhonemizerSharp.Domain.Settings.Enums;
using PhonemizerSharp.Ut.Helpers;

namespace PhonemizerSharp.Ut.Application.Providers.EspeakNgService;

[TestClass]
[TestCategory("LiveEspeakNg")]
public sealed class EspeakNgProcessPhonemizerLiveTests
{
    [TestMethod]
    public async Task PhonemizeAsync_WithInstalledEspeakNg_ReturnsIpaPhonemes()
    {
        new EspeakNgTestHelper().RequireEspeakNg();

        EspeakNgProcessPhonemizer phonemizer = new();

        PhonemizerResult result =
            await phonemizer.PhonemizeAsync(new PhonemizerRequest
            {
                Text = "Hello George",
                Language = "en-gb"
            });

        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Phonemes));
        Assert.AreEqual("en-gb", result.Language);
        Assert.AreEqual(PhonemeAlphabet.Ipa, result.Alphabet);
        Assert.AreEqual(PhonemizerProvider.EspeakNg, result.Provider);
        Assert.AreEqual(PhonemizerRuntime.Process, result.Runtime);
    }
}
