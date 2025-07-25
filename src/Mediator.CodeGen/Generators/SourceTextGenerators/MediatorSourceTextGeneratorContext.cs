using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Mediator.CodeGen.Generators.SourceTextGenerators
{
    internal class MediatorSourceTextGeneratorContext(
        Dictionary<INamedTypeSymbol, INamedTypeSymbol> requestToResponseSymbolMap,
        Dictionary<INamedTypeSymbol, INamedTypeSymbol[]> requestToRequestHandlerSymbolsMap,
        Dictionary<INamedTypeSymbol, Queue<INamedTypeSymbol>> requestToPipelineBehaviorSymbolsMap)
    {
        public Dictionary<INamedTypeSymbol, INamedTypeSymbol> RequestToResponseSymbolMap { get; } = requestToResponseSymbolMap;

        public Dictionary<INamedTypeSymbol, INamedTypeSymbol[]> RequestToRequestHandlerSymbolsMap { get; } = requestToRequestHandlerSymbolsMap;

        public Dictionary<INamedTypeSymbol, Queue<INamedTypeSymbol>> RequestToPipelineBehaviorSymbolsMap { get; } = requestToPipelineBehaviorSymbolsMap;

        public INamedTypeSymbol[] RequestSymbols => [.. RequestToResponseSymbolMap.Keys];

        public INamedTypeSymbol[] RequestHandlerSymbols => [.. RequestToRequestHandlerSymbolsMap
            .SelectMany(x => x.Value)
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)];

        public INamedTypeSymbol[] PipelineBehaviorSymbols => [.. RequestToPipelineBehaviorSymbolsMap
            .SelectMany(x => x.Value)
            .Distinct<INamedTypeSymbol>(SymbolEqualityComparer.Default)];
    }
}
