using System;

namespace Mediator.CodeGen.Contracts
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class PipelineBehaviorPriorityAttribute(uint priority) : Attribute
    {
        public uint Priority { get; } = priority;
    }
}
