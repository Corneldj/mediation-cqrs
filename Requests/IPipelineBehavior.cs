//----------------------------------------------------------------------------------
//
// IPipelineBehavior.cs -- The IPipelineBehavior interface.
//
//----------------------------------------------------------------------------------

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// A cross-cutting behavior that wraps the handling of a message. The same behavior
/// is applied to both the send pipeline (requests) and the publish pipeline
/// (notifications). Implement this interface as an open generic
/// (for example <c>MyBehavior&lt;TMessage&gt;</c>) and register it once with
/// <see cref="RequestHandlerRegistrationExtensions.AddPipelineBehavior"/>.
/// </summary>
/// <typeparam name="TMessage">The request or notification being handled.</typeparam>
public interface IPipelineBehavior<in TMessage>
{
    //------------------------------------------------------------------------------
    /// <summary>
    /// Handle Async
    /// </summary>
    /// <param name="message">The request or notification being handled.</param>
    /// <param name="next">The continuation that invokes the next behavior or handler.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task HandleAsync(TMessage message, PipelineDelegate next, CancellationToken cancellationToken);
}
