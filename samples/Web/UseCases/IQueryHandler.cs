using Mediator.CodeGen.Contracts;
using Web.Shared;

namespace Web.UseCases
{
    internal interface IQueryHandler<TRequest, TResult> : IRequestHandler<TRequest, Result<TResult>>
        where TRequest : IQuery<TResult>
        where TResult : IEquatable<TResult>
    { }
}
