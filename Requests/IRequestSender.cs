//----------------------------------------------------------------------------------
//
// IRequestSender.cs -- The IRequestSender class.
//
//----------------------------------------------------------------------------------

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// Request Sender
/// </summary>
public interface IRequestSender
{
    //------------------------------------------------------------------------------
    /// <summary>
    /// Send
    /// </summary>
    /// <typeparam name="TResponse">TResponse</typeparam>
    /// <param name="request">Request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>TResponse</returns>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}
