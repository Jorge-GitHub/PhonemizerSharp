using PhonemizerSharp.Application.Providers.EspeakNgNativeService;
using PhonemizerSharp.Domain.Phonemes.Enums;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Results;
using PhonemizerSharp.Domain.Settings.Enums;

namespace PhonemizerSharp.Ut.Application.Providers.EspeakNgNativeService;

[TestClass]
[TestCategory("LiveEspeakNgNative")]
public sealed class EspeakNgNativePhonemizerLiveTests
{
    [TestMethod]
    public async Task PhonemizeAsync_WithInstalledNativeEspeakNg_ReturnsIpaPhonemes()
    {
        using EspeakNgNativePhonemizer phonemizer = new();

        try
        {
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
            Assert.AreEqual(PhonemizerRuntime.NativeLibrary, result.Runtime);
        }
        catch (InvalidOperationException exception)
            when (exception.Message.Contains(
                "native eSpeak NG library",
                StringComparison.OrdinalIgnoreCase))
        {
            Assert.Inconclusive(
                "Native eSpeak NG was not found. Install libespeak-ng to run live native phonemizer tests.");
        }
    }
}
