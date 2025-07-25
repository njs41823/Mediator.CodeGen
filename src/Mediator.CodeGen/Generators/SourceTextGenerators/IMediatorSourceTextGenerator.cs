using Microsoft.CodeAnalysis.Text;

namespace Mediator.CodeGen.Generators.SourceTextGenerators
{
    internal interface IMediatorSourceTextGenerator
    {
        string HintName { get;  }

        SourceText Generate(MediatorSourceTextGeneratorContext context);
    }
}
