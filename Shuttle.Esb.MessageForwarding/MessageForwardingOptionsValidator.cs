using System;
using System.Linq;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Esb.MessageForwarding;

public class MessageForwardingOptionsValidator : IValidateOptions<MessageForwardingOptions>
{
    public ValidateOptionsResult Validate(string? name, MessageForwardingOptions options)
    {
        Guard.AgainstNull(options, nameof(options));

        foreach (var messageRoute in options.ForwardingRoutes)
        {
            if (!Uri.TryCreate(messageRoute.Uri, UriKind.RelativeOrAbsolute, out _))
            {
                return ValidateOptionsResult.Fail(string.Format(Esb.Resources.InvalidUriException, messageRoute.Uri, "MessageRoute.Uri"));
            }

            if (!(messageRoute.Specifications ?? Enumerable.Empty<MessageRouteOptions.SpecificationOptions>())
                .Any())
            {
                return ValidateOptionsResult.Fail(Esb.Resources.MessageRoutesRequireSpecificationException);
            }
        }

        return ValidateOptionsResult.Success;
    }
}