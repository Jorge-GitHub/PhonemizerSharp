using PhonemizerSharp.Application.Providers.EspeakNgService;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Settings.Providers;

namespace PhonemizerSharp.Ut.Application.Providers.EspeakNgService;

[TestClass]
public sealed class EspeakNgProcessPhonemizerTests
{
    [TestMethod]
    public async Task PhonemizeAsync_EmptyText_ThrowsArgumentException()
    {
        EspeakNgProcessPhonemizer phonemizer = new();
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
    public async Task PhonemizeAsync_MissingExecutable_ThrowsHelpfulException()
    {
        EspeakNgProcessPhonemizer phonemizer = new(new EspeakNgProviderSettings
        {
            ExecutablePath = "phonemizer-sharp-missing-espeak-ng"
        });

        InvalidOperationException exception =
            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
                phonemizer.PhonemizeAsync(new PhonemizerRequest
                {
                    Text = "Hello",
                    Language = "en-gb"
                }));

        StringAssert.Contains(exception.Message, "eSpeak NG executable");
    }
}
