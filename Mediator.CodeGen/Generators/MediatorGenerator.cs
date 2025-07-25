using Mediator.CodeGen.Contracts;
using Mediator.CodeGen.Extensions;
using Mediator.CodeGen.Generators.SourceTextGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mediator.CodeGen.Generators
{
    [Generator(LanguageNames.CSharp)]
    internal sealed class MediatorGenerator : IIncrementalGenerator
    {
        private Compilation? Compilation;

        private INamedTypeSymbol? RequestInterfaceSymbol;

        private INamedTypeSymbol? RequestHandlerInterfaceSymbol;

        private INamedTypeSymbol? PipelineBehaviorInterfaceSymbol;

        private INamedTypeSymbol? PipelineBehaviorPriorityAttribute;

        private readonly IMediatorSourceTextGenerator[] sourceTextGenerators;

        public MediatorGenerator()
        {
            this.sourceTextGenerators = [.. AssemblyReference.Assembly
                .GetTypes()
                .Where(x =>
                    !x.IsInterface &&
                    !x.IsAbstract &&
                    typeof(IMediatorSourceTextGenerator).IsAssignableFrom(x))
                .Select(x =>
                {
                    try
                    {
                        return (IMediatorSourceTextGenerator)Activator.CreateInstance(x);
                    }
                    catch
                    {
                        return (IMediatorSourceTextGenerator?)null;
                    }

                })
                .Where(x => x is not null)
                .Cast<IMediatorSourceTextGenerator>()];
        }

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            if (this.sourceTextGenerators.Length == 0)
            {
                return;
            }

            var typeDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is TypeDeclarationSyntax,
                    transform: static (ctx, _) => (TypeDeclarationSyntax)ctx.Node)
                .Where(static t => t is not null);

            var compilationAndTypes = context.CompilationProvider.Combine(typeDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndTypes, (spc, source) => {

                var (compilation, classDeclarations) = source;

                this.Compilation = compilation;

                this.RequestInterfaceSymbol = this.Compilation.GetTypeByMetadataName(typeof(IRequest<>).FullName);

                this.RequestHandlerInterfaceSymbol = this.Compilation.GetTypeByMetadataName(typeof(IRequestHandler<,>).FullName);

                this.PipelineBehaviorInterfaceSymbol = this.Compilation.GetTypeByMetadataName(typeof(IPipelineBehavior<,>).FullName);

                this.PipelineBehaviorPriorityAttribute = this.Compilation.GetTypeByMetadataName(typeof(PipelineBehaviorPriorityAttribute).FullName);

                if (this.Compilation is null ||
                    this.RequestInterfaceSymbol is null ||
                    this.RequestHandlerInterfaceSymbol is null ||
                    this.PipelineBehaviorInterfaceSymbol is null ||
                    this.PipelineBehaviorPriorityAttribute is null)
                {
                    return;
                }

                var closedRequestSymbols = new List<INamedTypeSymbol>();

                var closedRequestHandlerSymbols = new List<INamedTypeSymbol>();

                var openPipelineBehaviorSymbols = new List<INamedTypeSymbol>();

                foreach (var classDeclaration in classDeclarations)
                {
                    var model = this.Compilation.GetSemanticModel(classDeclaration.SyntaxTree);

                    if (model.GetDeclaredSymbol(classDeclaration) is not INamedTypeSymbol symbol)
                    {
                        continue;
                    }

                    if (IsClosedRequest(symbol))
                    {
                        closedRequestSymbols.Add(symbol);
                    }
                    else if (IsClosedRequestHandler(symbol))
                    {
                        closedRequestHandlerSymbols.Add(symbol);
                    }
                    else if (IsPipelineBehavior(symbol))
                    {
                        openPipelineBehaviorSymbols.Add(symbol);
                    }
                }

                var requestToResponseSymbolMap = new Dictionary<INamedTypeSymbol, INamedTypeSymbol>(SymbolEqualityComparer.Default);

                var requestToRequestHandlerSymbolsMap = new Dictionary<INamedTypeSymbol, INamedTypeSymbol[]>(SymbolEqualityComparer.Default);

                var requestToPipelineBehaviorSymbolsMap = new Dictionary<INamedTypeSymbol, Queue<INamedTypeSymbol>>(SymbolEqualityComparer.Default);

                foreach (var closedRequestSymbol in closedRequestSymbols)
                {
                    var responseType = this.GetRequestResponseSymbol(closedRequestSymbol);

                    if (responseType is null)
                    {
                        continue;
                    }

                    requestToResponseSymbolMap[closedRequestSymbol] = responseType;

                    requestToRequestHandlerSymbolsMap[closedRequestSymbol] = [.. closedRequestHandlerSymbols
                        .Where(x => this.IsRequestHandlerFor(x, closedRequestSymbol))];

                    requestToPipelineBehaviorSymbolsMap[closedRequestSymbol] = [];

                    foreach (var openPipelineBehaviorSymbol in openPipelineBehaviorSymbols
                        .OrderBy(x =>
                        {
                            if (TryGetPipelineBehaviorPriority(x, out var pipelineBehaviorPriority))
                            {
                                return pipelineBehaviorPriority;
                            }

                            return uint.MaxValue;
                        }))
                    {
                        if (!this.TryClosePipelineBehavior(openPipelineBehaviorSymbol, closedRequestSymbol, out var closedPipelineBehavior))
                        {
                            continue;
                        }

                        requestToPipelineBehaviorSymbolsMap[closedRequestSymbol].Enqueue(closedPipelineBehavior!);
                    }
                }

                var context = new MediatorSourceTextGeneratorContext(
                    requestToResponseSymbolMap: requestToResponseSymbolMap,
                    requestToRequestHandlerSymbolsMap: requestToRequestHandlerSymbolsMap,
                    requestToPipelineBehaviorSymbolsMap: requestToPipelineBehaviorSymbolsMap);

                foreach (var sourceTextGenerator in this.sourceTextGenerators)
                {
                    spc.AddSource(sourceTextGenerator.HintName, sourceTextGenerator.Generate(context));
                }
            });
        }

        private bool TryGetPipelineBehaviorPriority(
            INamedTypeSymbol typeSymbol,
            out uint? priority)
        {
            priority = null;

            var pipelineBehaviorPriorityAttribute = typeSymbol
                .GetAttributes()
                .FirstOrDefault(x => SymbolEqualityComparer.Default.Equals(x.AttributeClass, this.PipelineBehaviorPriorityAttribute));

            if (pipelineBehaviorPriorityAttribute is null)
            {
                return false;
            }

            if (pipelineBehaviorPriorityAttribute.ConstructorArguments.Length == 1 &&
                pipelineBehaviorPriorityAttribute.ConstructorArguments[0].Kind == TypedConstantKind.Primitive &&
                pipelineBehaviorPriorityAttribute.ConstructorArguments[0].Value is uint value)
            {
                priority = value;

                return true;
            }

            return false;
        }


        private bool TryClosePipelineBehavior(
            INamedTypeSymbol pipelineBehaviorSymbol,
            INamedTypeSymbol closedRequestSymbol,
            out INamedTypeSymbol? closedPipelineBehavior)
        {
            closedPipelineBehavior = null;

            var closedRequestResponseSymbol = this.GetRequestResponseSymbol(closedRequestSymbol);

            if (closedRequestResponseSymbol is null)
            {
                return false;
            }

            var pipelineBehaviorInterfaceSymbol = pipelineBehaviorSymbol
                .AllInterfaces
                .FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, this.PipelineBehaviorInterfaceSymbol));

            if (pipelineBehaviorInterfaceSymbol is null || pipelineBehaviorInterfaceSymbol.TypeArguments.Length != 2)
            {
                return false;
            }

            var pipelineBehaviorRequestSymbol = pipelineBehaviorInterfaceSymbol.TypeArguments[0];

            if (SymbolEqualityComparer.Default.Equals(pipelineBehaviorRequestSymbol, closedRequestSymbol))
            {
                // the pipeline behavior's TRequest argument is equal to the closedRequestSymbol, so it must be a closed IPipelineBehavior
                closedPipelineBehavior = pipelineBehaviorSymbol;

                return true;
            }

            var typeArguments = pipelineBehaviorSymbol.TypeArguments;

            if (pipelineBehaviorRequestSymbol is ITypeParameterSymbol requestTypeParameterSymbol &&
                IsAssignableToTypeParameter(closedRequestSymbol, requestTypeParameterSymbol))
            {
                // the closedRequestSymbol is assignable to the pipeline behavior's TRequest argument, so do it
                if (typeArguments.Length == 1)
                {
                    // this is a response-specific pipeline behavior. ensure the responses match
                    var pipelineBehaviorResponseSymbol = pipelineBehaviorInterfaceSymbol.TypeArguments[1];

                    if (
                        IsAssignableTo(closedRequestResponseSymbol, pipelineBehaviorResponseSymbol)
                        ||
                        (
                            pipelineBehaviorRequestSymbol is ITypeParameterSymbol typeParameterSymbol &&
                            IsAssignableToTypeParameter(closedRequestResponseSymbol, typeParameterSymbol))
                        )
                    {
                        closedPipelineBehavior = pipelineBehaviorSymbol.Construct(closedRequestSymbol);

                        return true;
                    }

                    return false;
                }

                closedPipelineBehavior = pipelineBehaviorSymbol.Construct(closedRequestSymbol, this.GetRequestResponseSymbol(closedRequestSymbol)!);

                return true;
            }

            return false;
        }

        private  bool IsAssignableTo(
            INamedTypeSymbol sourceType,
            ITypeSymbol targetType)
        {
            return this.Compilation.ClassifyConversion(sourceType, targetType).IsImplicit;
        }

        private bool IsAssignableToTypeParameter(
            INamedTypeSymbol namedType,
            ITypeParameterSymbol typeParameter)
        {
            if (typeParameter.HasReferenceTypeConstraint && !namedType.IsReferenceType)
            {
                return false;
            }    

            if (typeParameter.HasValueTypeConstraint && !namedType.IsValueType)
            {
                return false;
            }

            if (typeParameter.HasConstructorConstraint &&
                !namedType.InstanceConstructors.Any(c => c.Parameters.Length == 0 && c.DeclaredAccessibility == Accessibility.Public))
            {
                return false;
            }

            foreach (var constraint in typeParameter.ConstraintTypes)
            {
                if (constraint is INamedTypeSymbol constraintNamedType && constraintNamedType.IsGenericType)
                {
                    bool implementsGeneric = namedType
                        .AllInterfaces
                        .Any(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, constraintNamedType.OriginalDefinition));

                    if (!implementsGeneric)
                    {
                        return false;
                    }
                }
                else
                {
                    var conversion = this.Compilation.ClassifyConversion(namedType, constraint);

                    if (!conversion.IsImplicit)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private bool IsClosedRequest(INamedTypeSymbol typeSymbol)
        {
            return
                RequestInterfaceSymbol is not null &&
                typeSymbol.IsConcrete() &&
                typeSymbol.ImplementsInterface(RequestInterfaceSymbol);
        }

        private bool IsClosedRequestHandler(INamedTypeSymbol typeSymbol)
        {
            return
                this.RequestHandlerInterfaceSymbol is not null &&
                typeSymbol.IsConcrete() &&
                typeSymbol.ImplementsInterface(this.RequestHandlerInterfaceSymbol);
        }

        private bool IsPipelineBehavior(INamedTypeSymbol typeSymbol)
        {
            if (PipelineBehaviorInterfaceSymbol is null)
            {
                return false;
            }

            return typeSymbol.ImplementsInterface(PipelineBehaviorInterfaceSymbol);
        }

        private bool IsRequestHandlerFor(
            INamedTypeSymbol typeSymbol,
            INamedTypeSymbol requestType)
        {
            if (this.RequestHandlerInterfaceSymbol is null)
            {
                return false;
            }

            var responseType = GetRequestResponseSymbol(requestType);

            if (responseType is null)
            {
                return false;
            }

            return typeSymbol.AllInterfaces
                .Any(i =>
                    SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, this.RequestHandlerInterfaceSymbol) &&
                    SymbolEqualityComparer.Default.Equals(i.TypeArguments[0], requestType) &&
                    SymbolEqualityComparer.Default.Equals(i.TypeArguments[1], responseType));
        }

        private INamedTypeSymbol? GetRequestResponseSymbol(INamedTypeSymbol requestType)
        {
            if (RequestInterfaceSymbol is null)
            {
                return null;
            }

            var symbol =
                requestType
                    .AllInterfaces
                    .FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, RequestInterfaceSymbol))
                        ?.TypeArguments.FirstOrDefault();

            return symbol is INamedTypeSymbol responseSymbol
                ? responseSymbol
                : null;
        }

        private ITypeSymbol? GetPipelineBehaviorResponseSymbol(INamedTypeSymbol pipelineBehaviorSymbol)
        {
            if (this.PipelineBehaviorInterfaceSymbol is null)
            {
                return null;
            }

            return pipelineBehaviorSymbol
                .AllInterfaces
                .FirstOrDefault(i => SymbolEqualityComparer.Default.Equals(i.OriginalDefinition, this.PipelineBehaviorInterfaceSymbol))?
                .TypeArguments
                .FirstOrDefault();
        }
    }
}
