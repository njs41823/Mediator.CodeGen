using Mediator.CodeGen.Contracts;
using Web.Shared;

namespace Web.UseCases
{
    internal interface ICommandHandler<TRequest> : IRequestHandler<TRequest, Result>
        where TRequest : ICommand
    { }

    internal interface ICommandHandler<TRequest, TResult> : IRequestHandler<TRequest, Result<TResult>>
        where TRequest : ICommand<TResult>
        where TResult : IEquatable<TResult>
    { }
}
