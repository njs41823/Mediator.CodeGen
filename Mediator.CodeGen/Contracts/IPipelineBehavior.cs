using System.Threading;
using System.Threading.Tasks;

namespace Mediator.CodeGen.Contracts
{
    public interface IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Task<TResponse> Handle(
            TRequest request,
            PipelineBehaviorNextDelegate<TResponse> next,
            CancellationToken cancellationToken);
    }
}