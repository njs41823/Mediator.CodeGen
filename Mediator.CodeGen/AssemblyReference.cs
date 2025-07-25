using System.Reflection;

namespace Mediator.CodeGen
{
    internal static class AssemblyReference
    {
        public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
    }
}
