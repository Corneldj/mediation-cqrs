# Mediation

A small, dependency-light CQRS mediator for .NET 10.

Requests, notifications and cross-cutting behaviors, built on `Microsoft.Extensions.DependencyInjection`
and nothing else. Cross-cutting concerns are layered around handlers with the decorator pattern, so
handlers stay free of logging, validation, transaction and retry plumbing.

## Concepts

| Abstraction | Purpose |
| --- | --- |
| `IRequest<TResponse>` | A command or query. Exactly one handler. |
| `IRequestHandler<TRequest, TResponse>` | Handles one request type and returns a response. |
| `INotification` | An event. Zero or more handlers. |
| `INotificationHandler<TNotification>` | Handles a published notification. |
| `IPipelineBehavior<TMessage>` | A cross-cutting decorator wrapping a handler. |
| `IRequestSender` | Sends requests. |
| `IPublisher` | Publishes notifications. |
| `IMediator` | Both of the above. |

`IRequestSender` and `IPublisher` are separate so a consumer can depend on only the half it uses.
Both are backed by the same `RequestSender` instance.

## Registration

```csharp
services.AddMediator();

// Scan assemblies for every IRequestHandler<,> and INotificationHandler<>
services.AddRequestHandlers(typeof(Program).Assembly);

// Or register handlers one at a time
services.AddDecoratedRequestHandler<GetOrder, Order, GetOrderHandler>();
services.AddNotificationHandler<OrderShipped, NotifyCustomer>();

// Behaviors are open generics, registered once, applied to everything
services.AddPipelineBehavior(typeof(LoggingBehavior<>));
services.AddPipelineBehavior(typeof(ValidationBehavior<>));
```

Behaviors run in registration order — the first registered is the outermost. The same behavior
chain wraps both the send pipeline and the publish pipeline.

## Sending a request

```csharp
public sealed record GetOrder(int Id) : IRequest<Order>;

public sealed class GetOrderHandler(IOrderRepository repository)
    : IRequestHandler<GetOrder, Order>
{
    public Task<Order> HandleAsync(GetOrder request, CancellationToken cancellationToken) =>
        repository.FindAsync(request.Id, cancellationToken);
}

Order order = await sender.Send(new GetOrder(42), cancellationToken);
```

## Publishing a notification

```csharp
public sealed record OrderShipped(int Id) : INotification;

public sealed class NotifyCustomer : INotificationHandler<OrderShipped>
{
    public Task HandleAsync(OrderShipped notification, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

await publisher.Publish(new OrderShipped(42), cancellationToken);
```

Handlers are invoked sequentially, in registration order. If one throws, the remaining handlers
do not run.

## Writing a behavior

```csharp
public sealed class LoggingBehavior<TMessage>(ILogger<LoggingBehavior<TMessage>> logger)
    : IPipelineBehavior<TMessage>
{
    public async Task HandleAsync(
        TMessage message,
        PipelineDelegate next,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling {Message}", typeof(TMessage).Name);
        await next();
        logger.LogInformation("Handled {Message}", typeof(TMessage).Name);
    }
}
```

`PipelineDelegate` returns a non-generic `Task`; the response is captured by the terminal
delegate inside `RequestSender`, so a behavior can observe and short-circuit the call without
being generic over `TResponse`.

### Validation

`IRequestValidator<TRequest>` is a convention, not something the mediator calls on its own.
Resolve validators inside a behavior:

```csharp
public sealed class ValidationBehavior<TMessage>(IEnumerable<IRequestValidator<TMessage>> validators)
    : IPipelineBehavior<TMessage>
{
    public async Task HandleAsync(
        TMessage message,
        PipelineDelegate next,
        CancellationToken cancellationToken)
    {
        foreach (IRequestValidator<TMessage> validator in validators)
        {
            await validator.ValidateAsync(message, cancellationToken);
        }

        await next();
    }
}
```

## Notes

- Behaviors are registered as open generics, so a single registration closes over every request
  and notification type the mediator sees. `IPipelineBehavior<TMessage>` is contravariant, so a
  behavior written against a base type also satisfies injection of a derived one.
- `Send` resolves both the handler and the behavior chain from the request's *runtime* type, so
  sending through an `IRequest<TResponse>` reference still dispatches to the concrete handler.
  `Publish` resolves from the *compile-time* `TNotification`, which is the same type in the usual
  case but differs if you publish through a base-typed reference.
- Everything is registered as transient.

## Building

```bash
dotnet build
```

Targets `net10.0`, nullable enabled, warnings treated as errors.

## License

[MIT](LICENSE)
