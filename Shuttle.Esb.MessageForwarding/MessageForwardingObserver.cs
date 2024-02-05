using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.MessageForwarding
{
    public class MessageForwardingObserver : IPipelineObserver<OnAfterHandleMessage>
    {
        private readonly MessageRouteCollection _messageRoutes = new MessageRouteCollection();

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
                    messageRoute = new MessageRoute(new Uri(messageRouteOptions.Uri));

                    _messageRoutes.Add(messageRoute);
                }

                foreach (var specification in messageRouteOptions.Specifications)
                {
                    messageRoute.AddSpecification(specificationFactory.Create(specification.Name, specification.Value));
                }
            }
        }

        public void Execute(OnAfterHandleMessage pipelineEvent)
        {
            ExecuteAsync(pipelineEvent, false).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAfterHandleMessage pipelineEvent)
        {
            await ExecuteAsync(pipelineEvent, false).ConfigureAwait(false);
        }

        private async Task ExecuteAsync(OnAfterHandleMessage pipelineEvent, bool sync)
        {
            var state = Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)).Pipeline.State;
            var message = Guard.AgainstNull(state.GetMessage(), StateKeys.Message);
            var handlerContext = Guard.AgainstNull(state.GetHandlerContext() as IHandlerContext, StateKeys.HandlerContext);

            foreach (
                var uri in
                _messageRoutes.FindAll(message.GetType().FullName)
                    .Select(messageRoute => messageRoute.Uri.ToString())
                    .ToList())
            {
                var recipientUri = uri;

                if (sync)
                {
                    handlerContext.Send(message, builder => builder.WithRecipient(recipientUri));
                }
                else
                {
                    await handlerContext.SendAsync(message, builder => builder.WithRecipient(recipientUri)).ConfigureAwait(false);
                }
            }
        }
    }
}