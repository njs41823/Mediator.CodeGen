using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Text;

namespace Mediator.CodeGen.Generators.SourceTextGenerators
{
    internal sealed class ServiceCollectionExtensionsSourceTextGenerator : IMediatorSourceTextGenerator
    {
        public string HintName => "ServiceCollectionExtensions.g.cs";

        public SourceText Generate(MediatorSourceTextGeneratorContext context)
        {
            var sourceText = $$"""
using Microsoft.Extensions.DependencyInjection;
using Mediator.CodeGen.Contracts;
using Mediator.CodeGen.Implementation;

namespace Mediator.CodeGen.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMediator(this IServiceCollection services)
        {
{{GetAddServicesText(context.RequestHandlerSymbols, context.PipelineBehaviorSymbols)}}

            services
                .AddSingleton<RequestHandlerDelegatesMap>()
                .AddSingleton<PipelineBehaviorDelegatesMap>();

            return services
                .AddScoped<ISender, Sender>();
        }
    }
}
""";

            return SourceText.From(sourceText, Encoding.UTF8);
        }

        private string GetAddServicesText(
            INamedTypeSymbol[] requestHandlerSymbols,
            INamedTypeSymbol[] pipelineBehaviorSymbols)
        {
            var addRequestHandlersText = GetAddRequestHandlersText(requestHandlerSymbols);

            var addPipelineBehaviorsText = GetAddPipelineBehaviorsText(pipelineBehaviorSymbols);

            return string.Join(
                "\r\n\r\n",
                new[] { addRequestHandlersText, addPipelineBehaviorsText }
                    .Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private string GetAddRequestHandlersText(INamedTypeSymbol[] requestHandlerSymbols)
        {
            if (requestHandlerSymbols.Length == 0)
            {
                return "";
            }

            var addRequestHandlersTextBuilder = new StringBuilder("""
            services
""");

            foreach (var requestHandlerSymbol in requestHandlerSymbols)
            {
                addRequestHandlersTextBuilder.Append($$"""

                .AddScoped<{{requestHandlerSymbol.ToDisplayString()}}>()
""");
            }

            addRequestHandlersTextBuilder.Append(";");

            return addRequestHandlersTextBuilder.ToString();
        }

        private string GetAddPipelineBehaviorsText(INamedTypeSymbol[] pipelineBehaviorSymbols)
        {
            if (pipelineBehaviorSymbols.Length == 0)
            {
                return "";
            }

            var addPipelineBehaviorsTextBuilder = new StringBuilder("""
            services
""");

            foreach (var pipelineBehaviorSymbol in pipelineBehaviorSymbols)
            {
                addPipelineBehaviorsTextBuilder.Append($$"""

                .AddScoped<{{pipelineBehaviorSymbol.ToDisplayString()}}>()
""");
            }

            addPipelineBehaviorsTextBuilder.Append(";");

            return addPipelineBehaviorsTextBuilder.ToString();
        }
    }
}
