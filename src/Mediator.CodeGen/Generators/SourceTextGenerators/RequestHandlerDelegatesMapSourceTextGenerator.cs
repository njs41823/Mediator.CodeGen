using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mediator.CodeGen.Generators.SourceTextGenerators
{
    internal sealed class RequestHandlerDelegatesMapSourceTextGenerator : IMediatorSourceTextGenerator
    {
        public string HintName => "RequestHandlerDelegatesMap.g.cs";

        public SourceText Generate(MediatorSourceTextGeneratorContext context)
        {
            var sourceText = GetSourceText(
                context.RequestToResponseSymbolMap,
                context.RequestToRequestHandlerSymbolsMap);

            return SourceText.From(sourceText, Encoding.UTF8);
        }

        private string GetSourceText(
            Dictionary<INamedTypeSymbol, INamedTypeSymbol> requestToResponseSymbolMap,
            Dictionary<INamedTypeSymbol, INamedTypeSymbol[]> requestToRequestHandlerSymbolsMap)
        {
            var requestTypeToRequestHandlerDelegeMapEntriesBuilder = new StringBuilder();

            foreach (var kvp in requestToRequestHandlerSymbolsMap)
            {
                var requestHandlerType = kvp.Value.FirstOrDefault();

                if (requestHandlerType is null)
                {
                    continue;
                }

                var requestTypeName = kvp.Key.ToDisplayString();

                var requestHandlerTypeName = requestHandlerType.ToDisplayString();

                if (!requestToResponseSymbolMap.TryGetValue(kvp.Key, out var responseType))
                {
                    continue;
                }

                var responseTypeName = responseType.ToDisplayString();

                requestTypeToRequestHandlerDelegeMapEntriesBuilder.Append($$"""
                            {
                                typeof({{requestTypeName}}),
                                [
                """);

                foreach (var yurt in kvp.Value)
                {
                    requestTypeToRequestHandlerDelegeMapEntriesBuilder.Append($$"""

                    HandleRequest<{{requestTypeName}}, {{responseTypeName}}, {{requestHandlerTypeName}}>,

""");
                }

                requestTypeToRequestHandlerDelegeMapEntriesBuilder.Append($$"""
                                ]
                            },

                """);
            }

            return $$"""    
#nullable enable

using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Mediator.CodeGen.Contracts;

namespace Mediator.CodeGen.Implementation
{
    internal delegate object RequestHandlerDelegate(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken);

    internal sealed class RequestHandlerDelegatesMap
    {
        public bool TryGetRequestHandlerDelegate(
            Type requestType,
            [NotNullWhen(true)] out RequestHandlerDelegate[]? requestHandlerDelegates)
        {
            return map.TryGetValue(requestType, out requestHandlerDelegates);
        }

        private static object HandleRequest<TRequest, TResponse, TRequestHandler>(IServiceProvider serviceProvider, object request, CancellationToken cancellationToken)
            where TRequest : IRequest<TResponse>
            where TRequestHandler : IRequestHandler<TRequest, TResponse>
        {
            if (request is not TRequest typedRequest)
            {
                throw new InvalidCastException();
            }

            var requestHandler = serviceProvider.GetRequiredService<TRequestHandler>()!;

            return requestHandler.Handle(typedRequest, cancellationToken);
        }

        private readonly FrozenDictionary<Type, RequestHandlerDelegate[]> map = new Dictionary<Type, RequestHandlerDelegate[]>
        {
{{requestTypeToRequestHandlerDelegeMapEntriesBuilder}}        }.ToFrozenDictionary();
    }   
}
""";
        }
    }
}