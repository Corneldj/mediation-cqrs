//----------------------------------------------------------------------------------
//
// IPublisher.cs -- The IPublisher interface.
//
//----------------------------------------------------------------------------------

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// Publishes a notification to all registered handlers.
/// </summary>
public interface IPublisher
{
    //------------------------------------------------------------------------------
    /// <summary>
    /// Publish a notification to every registered handler.
    /// </summary>
    /// <typeparam name="TNotification">TNotification</typeparam>
    /// <param name="notification">Notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification;
}
