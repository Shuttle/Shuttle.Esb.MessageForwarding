using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.MessageForwarding
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageForwarding(this IServiceCollection services,
            Action<MessageForwardingBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var messageForwardingBuilder = new MessageForwardingBuilder(services);

            builder?.Invoke(messageForwardingBuilder);

            services.TryAddSingleton<MessageForwardingHostedService, MessageForwardingHostedService>();
            services.TryAddSingleton<MessageForwardingObserver, MessageForwardingObserver>();

            services.AddOptions<MessageForwardingOptions>().Configure(options =>
            {
                options.ForwardingRoutes = messageForwardingBuilder.Options.ForwardingRoutes;
            });

            services.AddHostedService<MessageForwardingHostedService>();

            return services;
        }
    }
}