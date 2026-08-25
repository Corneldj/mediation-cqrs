//----------------------------------------------------------------------------------
//
// INotificationHandler.cs -- The INotificationHandler interface.
//
//----------------------------------------------------------------------------------

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// Handles a published notification. Many handlers may handle the same notification.
/// </summary>
/// <typeparam name="TNotification">TNotification</typeparam>
public interface INotificationHandler<in TNotification>
    where TNotification : INotification
{
    //------------------------------------------------------------------------------
    /// <summary>
    /// Handle Async
    /// </summary>
    /// <param name="notification">Notification</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task HandleAsync(TNotification notification, CancellationToken cancellationToken);
}
