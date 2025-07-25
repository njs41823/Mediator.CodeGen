using System;
using Mediator.CodeGen.Extensions;

namespace Mediator.CodeGen.Contracts
{
    public sealed class MultipleRequestHandlersFoundException(Type requestType)
        : Exception($"Multiple {typeof(IRequestHandler<,>).GetFriendlyName()}s were found for {nameof(requestType)} '{requestType.Name}'.")
    {
        public Type RequestType { get; } = requestType;
    }
}
