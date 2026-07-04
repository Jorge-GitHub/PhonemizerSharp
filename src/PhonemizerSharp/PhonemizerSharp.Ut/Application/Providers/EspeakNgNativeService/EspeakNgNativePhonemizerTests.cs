using PhonemizerSharp.Application.Providers.EspeakNgNativeService;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Settings.Providers;

namespace PhonemizerSharp.Ut.Application.Providers.EspeakNgNativeService;

[TestClass]
public sealed class EspeakNgNativePhonemizerTests
{
    [TestMethod]
    public async Task PhonemizeAsync_EmptyText_ThrowsArgumentException()
    {
        EspeakNgNativePhonemizer phonemizer = new();
        PhonemizerRequest request = new()
        {
            Text = " "
        };

        ArgumentException exception =
            await Assert.ThrowsExactlyAsync<ArgumentException>(() =>
                phonemizer.PhonemizeAsync(request));

        StringAssert.Contains(exception.Message, "Text is required");
    }

    [TestMethod]
    public async Task PhonemizeAsync_MissingNativeLibrary_ThrowsHelpfulException()
    {
        EspeakNgNativePhonemizer phonemizer = new(new EspeakNgProviderSettings
        {
            NativeLibraryPath = "phonemizer-sharp-missing-libespeak-ng"
        });

        InvalidOperationException exception =
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
                phonemizer.PhonemizeAsync(new PhonemizerRequest
                {
                    Text = "Hello",
                    Language = "en-gb"
                }));

        StringAssert.Contains(exception.Message, "native eSpeak NG library");
    }
}
