using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Mediation.Requests;

public sealed class RequestSender(IServiceProvider serviceProvider) : IMediator
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        Type requestType = request.GetType();
        Type handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        object handler = serviceProvider.GetRequiredService(handlerType);
        MethodInfo handleMethod = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.HandleAsync))!;

        TResponse response = default!;
        PipelineDelegate terminal = async () =>
            response = await (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;

        PipelineDelegate pipeline = BuildPipeline(request, requestType, terminal, cancellationToken);
        await pipeline();

        return response;
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken)
        where TNotification : INotification
    {
        ArgumentNullException.ThrowIfNull(notification);

        IEnumerable<INotificationHandler<TNotification>> handlers =
            serviceProvider.GetServices<INotificationHandler<TNotification>>();

        PipelineDelegate terminal = async () =>
        {
            foreach (INotificationHandler<TNotification> handler in handlers)
            {
                await handler.HandleAsync(notification, cancellationToken);
            }
        };

        PipelineDelegate pipeline = BuildPipeline(notification, typeof(TNotification), terminal, cancellationToken);
        await pipeline();
    }

    private PipelineDelegate BuildPipeline(
        object message,
        Type messageType,
        PipelineDelegate terminal,
        CancellationToken cancellationToken)
    {
        Type behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(messageType);
        MethodInfo behaviorMethod = behaviorType.GetMethod(nameof(IPipelineBehavior<object>.HandleAsync))!;

        PipelineDelegate next = terminal;

        foreach (object? behavior in serviceProvider.GetServices(behaviorType).Reverse())
        {
            if (behavior is null)
            {
                continue;
            }

            PipelineDelegate currentNext = next;
            object currentBehavior = behavior;
            next = () => (Task)behaviorMethod.Invoke(currentBehavior, [message, currentNext, cancellationToken])!;
        }

        return next;
    }
}
