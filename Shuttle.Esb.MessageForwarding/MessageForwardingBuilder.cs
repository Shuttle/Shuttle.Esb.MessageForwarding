using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.MessageForwarding;

public class MessageForwardingBuilder
{
    private MessageForwardingOptions _messageForwardingOptions = new();

    public MessageForwardingBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public MessageForwardingOptions Options
    {
        get => _messageForwardingOptions;
        set => _messageForwardingOptions = Guard.AgainstNull(value);
    }

    public IServiceCollection Services { get; }
}