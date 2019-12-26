﻿using JetBrains.Annotations;
using Microsoft.CodeAnalysis.Completion;

namespace MirrorSharp.Internal.Abstraction {
    internal class CurrentCompletion {
        [CanBeNull] public CompletionList List { get; set; }
        public bool ChangeEchoPending { get; set; }
        public char? PendingChar { get; set; }

        public void ResetPending() {
            ChangeEchoPending = false;
            PendingChar = null;
        }
    }
}

