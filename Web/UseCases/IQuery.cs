using Mediator.CodeGen.Contracts;
using Web.Shared;

namespace Web.UseCases
{
    internal interface IQuery<TResult> : IRequest<Result<TResult>>
        where TResult : IEquatable<TResult>;
}
