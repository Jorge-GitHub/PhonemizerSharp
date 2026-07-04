using PhonemizerSharp.Application.Contracts;
using PhonemizerSharp.Application.Factories;
using PhonemizerSharp.Application.Providers.EspeakNgNativeService;
using PhonemizerSharp.Application.Providers.EspeakNgService;
using PhonemizerSharp.Domain.Settings;
using PhonemizerSharp.Domain.Settings.Enums;

namespace PhonemizerSharp.Ut.Application.Factories;

[TestClass]
public sealed class PhonemizerProviderFactoryTests
{
    [TestMethod]
    public void Create_DefaultSettings_ReturnsEspeakNgNativeProvider()
    {
        IPhonemizer provider =
            new PhonemizerProviderFactory().Create(new PhonemizerSettings());

        Assert.IsInstanceOfType<EspeakNgNativePhonemizer>(provider);
    }

    [TestMethod]
    public void Create_EspeakProcessRuntime_ReturnsProcessProvider()
    {
        PhonemizerSettings settings = new()
        {
            Provider = PhonemizerProvider.EspeakNg,
            Runtime = PhonemizerRuntime.Process
        };

        IPhonemizer provider =
            new PhonemizerProviderFactory().Create(settings);

        Assert.IsInstanceOfType<EspeakNgProcessPhonemizer>(provider);
    }

    [TestMethod]
    public void Create_EspeakNativeRuntime_ReturnsNativeProvider()
    {
        PhonemizerSettings settings = new()
        {
            Provider = PhonemizerProvider.EspeakNg,
            Runtime = PhonemizerRuntime.NativeLibrary
        };

        IPhonemizer provider =
            new PhonemizerProviderFactory().Create(settings);

        Assert.IsInstanceOfType<EspeakNgNativePhonemizer>(provider);
    }

    [TestMethod]
    public void Create_UnsupportedProvider_ThrowsNotSupported()
    {
        PhonemizerSettings settings = new()
        {
            Provider = PhonemizerProvider.Gruut
        };

        NotSupportedException exception =
            Assert.ThrowsExactly<NotSupportedException>(() =>
                new PhonemizerProviderFactory().Create(settings));

        StringAssert.Contains(exception.Message, "Gruut");
    }

    [TestMethod]
    public void Create_UnsupportedRuntime_ThrowsNotSupported()
    {
        PhonemizerSettings settings = new()
        {
            Provider = PhonemizerProvider.EspeakNg,
            Runtime = PhonemizerRuntime.Python
        };

        NotSupportedException exception =
            Assert.ThrowsExactly<NotSupportedException>(() =>
                new PhonemizerProviderFactory().Create(settings));

        StringAssert.Contains(exception.Message, "Python");
    }
}
