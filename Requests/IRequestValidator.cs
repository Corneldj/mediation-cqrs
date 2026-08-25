//----------------------------------------------------------------------------------
//
// IRequestValidator.cs -- The IRequestValidator class.
//
//----------------------------------------------------------------------------------

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// IRequest Validator
/// </summary>
/// <typeparam name="TRequest">TRequest</typeparam>
public interface IRequestValidator<in TRequest>
{
    //------------------------------------------------------------------------------
    /// <summary>
    /// Validate Async
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ValidateAsync(TRequest request, CancellationToken cancellationToken);
}
