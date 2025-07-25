using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;

namespace Mediator.CodeGen.Extensions
{
    internal static class NamedTypeSymbolExtensions
    {
        public static bool ImplementsInterface(this INamedTypeSymbol symbol, INamedTypeSymbol interfaceSymbol)
        {
            return symbol
                .AllInterfaces
                .Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, interfaceSymbol));
        }

        public static bool IsConcrete(this INamedTypeSymbol symbol)
        {
            return
                !symbol.IsAbstract &&
                !symbol.IsGenericType &&
                symbol.TypeKind != TypeKind.Interface;
        }
    }
}
