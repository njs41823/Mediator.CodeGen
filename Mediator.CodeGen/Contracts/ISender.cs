using System.Threading;
using System.Threading.Tasks;

namespace Mediator.CodeGen.Contracts
{
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
    }
}