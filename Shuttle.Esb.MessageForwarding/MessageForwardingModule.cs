using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Esb.MessageForwarding
{
    public class MessageForwardingHostedService : IHostedService
    {
        private readonly string _inboxMessagePipelineName = typeof(InboxMessagePipeline).FullName;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly MessageForwardingObserver _messageForwardingObserver;

        public MessageForwardingHostedService(IPipelineFactory pipelineFactory, MessageForwardingObserver messageForwardingObserver)
        {
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _messageForwardingObserver = Guard.AgainstNull(messageForwardingObserver, nameof(messageForwardingObserver));

            pipelineFactory.PipelineCreated += OnPipelineCreated;
        }

        private void OnPipelineCreated(object sender, PipelineEventArgs e)
        {
            if (!(e.Pipeline.GetType().FullName ?? string.Empty)
                .Equals(_inboxMessagePipelineName, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            e.Pipeline.RegisterObserver(_messageForwardingObserver);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _pipelineFactory.PipelineCreated -= OnPipelineCreated;

            await Task.CompletedTask;
        }
    }
}