using System;
using Microsoft.CodeAnalysis;

namespace MirrorSharp.Internal.Roslyn {
    internal static class RoslynScriptHelper {
        public static void Validate(bool isScript, bool isRepl, Type hostObjectType) {
            if (!isScript && hostObjectType != null)
                throw new ArgumentException($"HostObjectType requires script mode (IsScript must be true).", nameof(hostObjectType));
            if (!isScript && isRepl)
                throw new ArgumentException($"REPL sub-mode requires script mode (IsScript must be true).", nameof(isRepl));
        }

        public static SourceCodeKind GetSourceKind(bool isScript) => isScript ? SourceCodeKind.Script : SourceCodeKind.Regular;
    }
}
