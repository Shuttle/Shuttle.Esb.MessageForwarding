using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.MessageForwarding;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageForwarding(this IServiceCollection services, Action<MessageForwardingBuilder>? builder = null)
    {
        var messageForwardingBuilder = new MessageForwardingBuilder(Guard.AgainstNull(services));

        builder?.Invoke(messageForwardingBuilder);

        services.TryAddSingleton<MessageForwardingHostedService, MessageForwardingHostedService>();
        services.TryAddSingleton<MessageForwardingObserver, MessageForwardingObserver>();

        services.AddSingleton(Options.Create(messageForwardingBuilder.Options));

        services.AddHostedService<MessageForwardingHostedService>();

        return services;
    }
}