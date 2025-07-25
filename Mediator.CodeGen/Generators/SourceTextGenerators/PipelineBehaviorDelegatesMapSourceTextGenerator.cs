using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace Mediator.CodeGen.Generators.SourceTextGenerators
{
    internal sealed class PipelineBehaviorDelegatesMapSourceTextGenerator : IMediatorSourceTextGenerator
    {
        public string HintName => "PipelineBehaviorDelegatesMap.g.cs";

        public SourceText Generate(MediatorSourceTextGeneratorContext context)
        {
            var sourceText = GetSourceText(
                context.RequestToResponseSymbolMap,
                context.RequestToPipelineBehaviorSymbolsMap);

            return SourceText.From(sourceText, Encoding.UTF8);
        }

        private string GetSourceText(
            Dictionary<INamedTypeSymbol, INamedTypeSymbol> requestToResponseSymbolMap,
            Dictionary<INamedTypeSymbol, Queue<INamedTypeSymbol>> requestToPipelineBehaviorSymbolsMap)
        {
            var requestTypeToPipelineBehaviorDelegatesMapEntries = new StringBuilder();

            foreach (var kvp in requestToPipelineBehaviorSymbolsMap)
            {
                if (kvp.Value.Count == 0)
                {
                    continue;
                }

                var requestTypeName = kvp.Key.ToDisplayString();

                if (!requestToResponseSymbolMap.TryGetValue(kvp.Key, out var responseType))
                {
                    continue;
                }

                var responseTypeName = responseType.ToDisplayString();

                requestTypeToPipelineBehaviorDelegatesMapEntries.Append($$"""
            {
                typeof({{requestTypeName}}),
                [
""");

                foreach (var pipelineBehaviorType in kvp.Value)
                {
                    requestTypeToPipelineBehaviorDelegatesMapEntries.Append($$"""

                    HandlePipelineBehavior<{{requestTypeName}}, {{responseTypeName}}, {{pipelineBehaviorType}}>,
""");
                }

                requestTypeToPipelineBehaviorDelegatesMapEntries.Append($$"""

                ]
            },

""");
            }

            return $$"""
#nullable enable

using System;
using System.Threading;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Mediator.CodeGen.Contracts;

namespace Mediator.CodeGen.Implementation
{
    internal delegate object PipelineBehaviorDelegate(IServiceProvider serviceProvider, object request, Delegate next, CancellationToken cancellationToken);

    internal sealed class PipelineBehaviorDelegatesMap
    {
        public bool TryGetPipelineBehaviorDelegates(
            Type requestType,
            [NotNullWhen(true)] out PipelineBehaviorDelegate[]? pipelineBehaviorDelegates)
        {
            return map.TryGetValue(requestType, out pipelineBehaviorDelegates);
        }

        private static Task<TResponse> HandlePipelineBehavior<TRequest, TResponse, TPipelineBehavior>(IServiceProvider serviceProvider, object request, Delegate next, CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
            where TPipelineBehavior : IPipelineBehavior<TRequest, TResponse>
        {
            if (request is not TRequest typedRequest)
            {
                throw new InvalidCastException();
            }

            if (next is not PipelineBehaviorNextDelegate<TResponse> pipelineBehaviorNextDelegate)
            {
                throw new InvalidCastException();
            }

            var pipelineBehavior = serviceProvider.GetRequiredService<TPipelineBehavior>();

            return pipelineBehavior.Handle(typedRequest, pipelineBehaviorNextDelegate, cancellationToken);
        }

        private readonly FrozenDictionary<Type, PipelineBehaviorDelegate[]> map = new Dictionary<Type, PipelineBehaviorDelegate[]>
        {
{{requestTypeToPipelineBehaviorDelegatesMapEntries}}
        }.ToFrozenDictionary();
    }
}

""";
        }
    }
}