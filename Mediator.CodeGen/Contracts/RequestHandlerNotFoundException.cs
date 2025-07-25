using System;
using Mediator.CodeGen.Extensions;

namespace Mediator.CodeGen.Contracts
{
    public sealed class RequestHandlerNotFoundException(Type requestType)
        : Exception($"No {typeof(IRequestHandler<,>).GetFriendlyName()} was found for {nameof(requestType)} '{requestType.Name}'.")
    {
        public Type RequestType { get; } = requestType;
    }
}
