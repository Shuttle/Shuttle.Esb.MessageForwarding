using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.MessageForwarding;

public class MessageForwardingObserver : IPipelineObserver<OnAfterHandleMessage>
{
    private readonly MessageRouteCollection _messageRoutes = new();

    public MessageForwardingObserver(IOptions<MessageForwardingOptions> messageForwardingOptions)
    {
        Guard.AgainstNull(messageForwardingOptions, nameof(messageForwardingOptions));
        Guard.AgainstNull(messageForwardingOptions.Value, nameof(messageForwardingOptions.Value));

        var specificationFactory = new MessageRouteSpecificationFactory();

        foreach (var messageRouteOptions in messageForwardingOptions.Value.ForwardingRoutes)
        {
            var messageRoute = _messageRoutes.Find(messageRouteOptions.Uri);

            if (messageRoute == null)
            {
                messageRoute = new MessageRoute(new(messageRouteOptions.Uri));

                _messageRoutes.Add(messageRoute);
            }

            foreach (var specification in messageRouteOptions.Specifications)
            {
                messageRoute.AddSpecification(specificationFactory.Create(specification.Name, specification.Value));
            }
        }
    }

    public async Task ExecuteAsync(IPipelineContext<OnAfterHandleMessage> pipelineContext)
    {
        var state = Guard.AgainstNull(pipelineContext).Pipeline.State;
        var message = Guard.AgainstNull(state.GetMessage());
        var handlerContext = Guard.AgainstNull(state.GetHandlerContext() as IHandlerContext);
        var messageType = Guard.AgainstNullOrEmptyString(message.GetType().FullName);

        foreach (
            var uri in
            _messageRoutes.FindAll(messageType)
                .Select(messageRoute => messageRoute.Uri.ToString())
                .ToList())
        {
            var recipientUri = uri;

            await handlerContext.SendAsync(message, builder => builder.WithRecipient(recipientUri)).ConfigureAwait(false);
        }
    }
}