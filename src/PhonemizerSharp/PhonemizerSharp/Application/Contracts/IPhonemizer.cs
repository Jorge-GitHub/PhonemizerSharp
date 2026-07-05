using PhonemizerSharp.Domain.Requests;
using PhonemizerSharp.Domain.Results;

namespace PhonemizerSharp.Application.Contracts;

public interface IPhonemizer
{
    Task<PhonemizerResult> PhonemizeAsync(
        PhonemizerRequest request,
        CancellationToken cancellationToken = default);
}
