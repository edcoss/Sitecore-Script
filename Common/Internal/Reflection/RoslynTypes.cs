using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Completion;

namespace MirrorSharp.Internal.Reflection {
    using TypeInfo = System.Reflection.TypeInfo;
    using static RoslynAssemblies;

    internal static class RoslynTypes {
        public static readonly TypeInfo CodeAction = typeof(CodeAction).GetTypeInfo();
        public static readonly TypeInfo CompletionChange = typeof(CompletionChange).GetTypeInfo();
        public static readonly TypeInfo CompletionList = typeof(CompletionList).GetTypeInfo();
        public static readonly TypeInfo SymbolDisplayPartKindTags = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SymbolDisplayPartKindTags", true).GetTypeInfo();

        // ReSharper disable once InconsistentNaming
        public static readonly TypeInfo ISignatureHelpProvider = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.ISignatureHelpProvider", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpTriggerInfo = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpTriggerInfo", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpTriggerReason = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpTriggerReason", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpItems = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpItems", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpItem = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpItem", true).GetTypeInfo();
        public static readonly TypeInfo SignatureHelpParameter = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.SignatureHelp.SignatureHelpParameter", true).GetTypeInfo();

        public static readonly TypeInfo WorkspaceOptionSet = MicrosoftCodeAnalysisWorkspaces.GetType("Microsoft.CodeAnalysis.Options.WorkspaceOptionSet", true).GetTypeInfo();
        public static readonly TypeInfo WorkspaceAnalyzerOptions = MicrosoftCodeAnalysisFeatures.GetType("Microsoft.CodeAnalysis.Diagnostics.WorkspaceAnalyzerOptions", true).GetTypeInfo();        
    }
}