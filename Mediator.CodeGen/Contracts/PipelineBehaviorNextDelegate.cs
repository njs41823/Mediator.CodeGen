using System.Threading.Tasks;

namespace Mediator.CodeGen.Contracts
{
    public delegate Task<TResponse> PipelineBehaviorNextDelegate<TResponse>();
}
