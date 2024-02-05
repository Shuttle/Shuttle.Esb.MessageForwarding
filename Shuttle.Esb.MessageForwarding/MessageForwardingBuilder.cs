using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.MessageForwarding
{
    public class MessageForwardingBuilder
    {
        private MessageForwardingOptions _messageForwardingOptions = new MessageForwardingOptions();
        public IServiceCollection Services { get; }

        public MessageForwardingBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public MessageForwardingOptions Options
        {
            get => _messageForwardingOptions;
            set => _messageForwardingOptions = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}