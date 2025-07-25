using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Mediator.CodeGen.Generators.SourceTextGenerators
{
    internal sealed class SenderSourceTextGenerator : IMediatorSourceTextGenerator
    {
        public string HintName => "Sender.g.cs";

        public SourceText Generate(MediatorSourceTextGeneratorContext context)
        {
            var sourceText = """    
using Mediator.CodeGen.Contracts;

namespace Mediator.CodeGen.Implementation
{
    internal sealed class Sender(
        IServiceProvider serviceProvider,
        RequestHandlerDelegatesMap requestHandlerDelegatesMap,
        PipelineBehaviorDelegatesMap pipelineBehaviorDelegatesMap) : ISender
    {
        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
        {
            if (!requestHandlerDelegatesMap.TryGetRequestHandlerDelegate(request.GetType(), out var requestHandlerDelegates)
                || requestHandlerDelegates.Length == 0)
            {
                throw new RequestHandlerNotFoundException(request.GetType());
            }

            if (requestHandlerDelegates.Length > 1)
            {
                throw new MultipleRequestHandlersFoundException(request.GetType());
            }

            var requestHandlerDelegate = requestHandlerDelegates[0];

            if (!pipelineBehaviorDelegatesMap.TryGetPipelineBehaviorDelegates(request.GetType(), out var pipelineBehaviorDelegates))
            {
                pipelineBehaviorDelegates = [];
            }

            PipelineBehaviorNextDelegate<TResponse> nextPipelineBehaviorDelegate = () =>
            {
                return requestHandlerDelegate(serviceProvider, request, cancellationToken) is Task<TResponse> responseTask
                    ? responseTask
                    : throw new InvalidCastException();
            };

            for (var i = pipelineBehaviorDelegates.Length - 1; i >= 0; i--)
            {
                var pipelineBehaviorDelegate = pipelineBehaviorDelegates[i];

                var next = nextPipelineBehaviorDelegate;

                nextPipelineBehaviorDelegate = () =>
                {
                    return pipelineBehaviorDelegate(serviceProvider, request, next, cancellationToken) is Task<TResponse> responseTask
                        ? responseTask
                        : throw new InvalidCastException();
                };
            }

            return nextPipelineBehaviorDelegate();
        }
    }
}
""";

            return SourceText.From(sourceText, Encoding.UTF8);
        }
    }
}