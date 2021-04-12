// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true, Inherited = false)]
    public sealed class InterpolatedBuilderArgumentAttribute : Attribute
    {
        public InterpolatedBuilderArgumentAttribute(string argument)
        {
            Arguments = new[] { argument };
        }

        public InterpolatedBuilderArgumentAttribute(params string[] arguments)
        {
            Arguments = arguments;
        }

        public string[] Arguments { get; }
    }
}
