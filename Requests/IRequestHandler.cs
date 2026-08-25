//----------------------------------------------------------------------------------
//
// IRequestHandler.cs -- The IRequestHandler class.
//
//----------------------------------------------------------------------------------

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// IRequest Handler
/// </summary>
/// <typeparam name="TRequest">TRequest</typeparam>
/// <typeparam name="TResponse">TResponse</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    //------------------------------------------------------------------------------
    /// <summary>
    /// Handle Async
    /// </summary>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>TResponse</returns>
    Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
