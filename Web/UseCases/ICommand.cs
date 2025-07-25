using Mediator.CodeGen.Contracts;
using Web.Shared;

namespace Web.UseCases
{
    internal interface ICommand : IRequest<Result>;

    internal interface ICommand<TResult> : IRequest<Result<TResult>>
        where TResult : IEquatable<TResult>;
}
