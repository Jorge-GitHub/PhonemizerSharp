using PhonemizerSharp.Application.Contracts;
using PhonemizerSharp.Application.Factories;
using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Results;
using PhonemizerSharp.Domain.Settings;

namespace PhonemizerSharp.Application;

public sealed class PhonemizerService : IPhonemizer, IDisposable
{
    private readonly IPhonemizer phonemizer;
    private readonly bool ownsPhonemizer;

    public PhonemizerService() : this(new PhonemizerSettings()) { }

    public PhonemizerService(PhonemizerSettings settings)
        : this(new PhonemizerProviderFactory().Create(settings),
            ownsPhonemizer: true) { }

    public PhonemizerService(IPhonemizer phonemizer)
        : this(phonemizer, ownsPhonemizer: false) { }

    public Task<PhonemizerResult> PhonemizeAsync(
        PhonemizerRequest request,
        CancellationToken cancellationToken = default)
    {
        return this.phonemizer.PhonemizeAsync(request, cancellationToken);
    }

    public void Dispose()
    {
        if (this.ownsPhonemizer
            && this.phonemizer is IDisposable disposable)
        {
            disposable.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private PhonemizerService(IPhonemizer phonemizer, bool ownsPhonemizer)
    {
        ArgumentNullException.ThrowIfNull(phonemizer);

        this.phonemizer = phonemizer;
        this.ownsPhonemizer = ownsPhonemizer;
    }
}
