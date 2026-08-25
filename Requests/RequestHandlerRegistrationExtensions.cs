//----------------------------------------------------------------------------------
//
// RequestHandlerRegistrationExtensions.cs -- The RequestHandlerRegistrationExtensions class.
//
//----------------------------------------------------------------------------------

using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace Mediation.Requests;

//----------------------------------------------------------------------------------
/// <summary>
/// Request handler registration extensions
/// </summary>
public static class RequestHandlerRegistrationExtensions
{
    //------------------------------------------------------------------------------
    /// <summary>
    /// Register the mediator and expose it through the
    /// <see cref="IRequestSender"/>, <see cref="IPublisher"/> and
    /// <see cref="IMediator"/> abstractions, all backed by a single instance.
    /// </summary>
    /// <param name="services">Services</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddTransient<RequestSender>();
        services.AddTransient<IMediator>(provider => provider.GetRequiredService<RequestSender>());
        services.AddTransient<IRequestSender>(provider => provider.GetRequiredService<IMediator>());
        services.AddTransient<IPublisher>(provider => provider.GetRequiredService<IMediator>());

        return services;
    }

    //------------------------------------------------------------------------------
    /// <summary>
    /// Register a cross-cutting pipeline behavior. The behavior is applied to both
    /// the send pipeline (requests) and the publish pipeline (notifications).
    /// Pass an open generic type, for example <c>typeof(LoggingBehavior&lt;&gt;)</c>.
    /// Behaviors run in registration order, the first registered being the outermost.
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="behaviorType">An open generic type implementing <see cref="IPipelineBehavior{TMessage}"/>.</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddPipelineBehavior(this IServiceCollection services, Type behaviorType)
    {
        ArgumentNullException.ThrowIfNull(behaviorType);

        if (!behaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException(
                $"'{behaviorType}' must be an open generic type, for example typeof(MyBehavior<>).",
                nameof(behaviorType));
        }

        services.AddTransient(typeof(IPipelineBehavior<>), behaviorType);

        return services;
    }

    //------------------------------------------------------------------------------
    /// <summary>
    /// Scan the given assemblies and register every concrete
    /// <see cref="IRequestHandler{TRequest, TResponse}"/> and
    /// <see cref="INotificationHandler{TNotification}"/> implementation against the
    /// interface(s) it implements. Cross-cutting concerns are applied centrally by
    /// the registered <see cref="IPipelineBehavior{TMessage}"/> behaviors.
    /// </summary>
    /// <param name="services">Services</param>
    /// <param name="assemblies">The assemblies to scan for handlers.</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddRequestHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        Type[] handlerDefinitions = [typeof(IRequestHandler<,>), typeof(INotificationHandler<>)];

        IEnumerable<Type> implementations = assemblies
            .SelectMany(GetLoadableTypes)
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false });

        foreach (Type implementation in implementations)
        {
            IEnumerable<Type> handlerServices = implementation
                .GetInterfaces()
                .Where(@interface => @interface.IsGenericType
                    && handlerDefinitions.Contains(@interface.GetGenericTypeDefinition()));

            foreach (Type handlerService in handlerServices)
            {
                services.AddTransient(handlerService, implementation);
            }
        }

        return services;
    }

    //------------------------------------------------------------------------------
    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            return exception.Types.Where(type => type is not null)!;
        }
    }

    //------------------------------------------------------------------------------
    /// <summary>
    /// Register a request handler. Cross-cutting concerns are applied centrally by
    /// the registered <see cref="IPipelineBehavior{TMessage}"/> behaviors.
    /// </summary>
    /// <typeparam name="TRequest">TRequest</typeparam>
    /// <typeparam name="TResponse">TResponse</typeparam>
    /// <typeparam name="THandler">THandler</typeparam>
    /// <param name="services">Services</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddDecoratedRequestHandler<TRequest, TResponse, THandler>(
        this IServiceCollection services)
        where TRequest : class, IRequest<TResponse>
        where THandler : class, IRequestHandler<TRequest, TResponse>
    {
        services.AddTransient<IRequestHandler<TRequest, TResponse>, THandler>();

        return services;
    }

    //------------------------------------------------------------------------------
    /// <summary>
    /// Register a notification handler. Multiple handlers may be registered for the
    /// same notification; every handler is invoked when the notification is published.
    /// Cross-cutting concerns are applied centrally by the registered
    /// <see cref="IPipelineBehavior{TMessage}"/> behaviors.
    /// </summary>
    /// <typeparam name="TNotification">TNotification</typeparam>
    /// <typeparam name="THandler">THandler</typeparam>
    /// <param name="services">Services</param>
    /// <returns>IServiceCollection</returns>
    public static IServiceCollection AddNotificationHandler<TNotification, THandler>(
        this IServiceCollection services)
        where TNotification : INotification
        where THandler : class, INotificationHandler<TNotification>
    {
        services.AddTransient<INotificationHandler<TNotification>, THandler>();

        return services;
    }
}
