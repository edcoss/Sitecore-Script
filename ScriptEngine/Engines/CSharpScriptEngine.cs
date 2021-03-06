﻿using System;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Collections;
using System.Linq;
using Microsoft.CodeAnalysis;
using ScriptSharp.ScriptEngine.Models;
using ScriptSharp.ScriptEngine.Abstractions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ScriptSharp.ScriptEngine.Engines
{
    public class CSharpScriptEngine : IScriptEngine
    {
        private static ScriptState<object> staticScriptState = null;

        /// <summary>
        /// ScriptState stores Roslyn's compiled script code and variables. It's stored and retrieved from Session.
        /// It uses InProc session state mode. It does not work when using SQL, Mongo or Redis session providers.
        /// As it is not decorated with the Serializable attribute.
        /// </summary>
        private ScriptState<object> ScriptState
        {
            get
            {
                if (System.Web.HttpContext.Current != null &&
                    System.Web.HttpContext.Current.Session != null &&
                    System.Web.HttpContext.Current.Session.Mode == System.Web.SessionState.SessionStateMode.InProc)
                {
                    // Roslyn's ScriptState object can only work in InProc session state, as it's not marked as serializable to be handled on SQL, Mongo or Redis
                    return System.Web.HttpContext.Current.Session["scriptState"] as ScriptState<object>;
                }
                else
                {
                    // Otherwise, use static...
                    return staticScriptState;
                }
            }
            set
            {
                if (System.Web.HttpContext.Current != null &&
                    System.Web.HttpContext.Current.Session != null &&
                    System.Web.HttpContext.Current.Session.Mode == System.Web.SessionState.SessionStateMode.InProc)
                {
                    System.Web.HttpContext.Current.Session["scriptState"] = value;
                }
                else
                { 
                    staticScriptState = value;
                }
            }
        }

        private static ScriptOptions staticScriptOptions = ScriptOptions.Default;

        /// <summary>
        /// ScriptOptions stores scripting settings. It's stored and retrieved from Session.
        /// It uses InProc session state mode. It does not work when using SQL, Mongo or Redis session providers.
        /// As it is not decorated with the Serializable attribute.
        /// </summary>
        private static ScriptOptions ScriptOptions
        {
            get
            {
                if (System.Web.HttpContext.Current != null &&
                    System.Web.HttpContext.Current.Session != null &&
                    System.Web.HttpContext.Current.Session.Mode == System.Web.SessionState.SessionStateMode.InProc)
                {
                    // Only InProc session state works
                    var sessionScriptOptions = System.Web.HttpContext.Current.Session["scriptOptions"] as ScriptOptions;
                    sessionScriptOptions = sessionScriptOptions == null ? ScriptOptions.Default : sessionScriptOptions;
                    return sessionScriptOptions;
                }
                else
                {
                    // otherwise, use static ...
                    return staticScriptOptions;
                }
            }
            set
            {
                if (System.Web.HttpContext.Current != null &&
                    System.Web.HttpContext.Current.Session != null &&
                    System.Web.HttpContext.Current.Session.Mode == System.Web.SessionState.SessionStateMode.InProc)
                {
                    // Only InProc session state works
                    System.Web.HttpContext.Current.Session["scriptOptions"] = value;
                }
                else
                {
                    // otherwise, use static ...
                    staticScriptOptions = value;
                }
            }
        }

        private static Globals globals = new Globals();

        public static Globals Globals
        {
            get
            {
                if (System.Web.HttpContext.Current != null &&
                    System.Web.HttpContext.Current.Session != null &&
                    System.Web.HttpContext.Current.Session.Mode == System.Web.SessionState.SessionStateMode.InProc)
                {
                    var sessionGlobals = System.Web.HttpContext.Current.Session["Globals"] as Globals;
                    sessionGlobals = sessionGlobals == null ? new Globals() : sessionGlobals;
                    return sessionGlobals;
                }
                else
                {
                    return globals;
                }
            }

            set
            {
                if (System.Web.HttpContext.Current != null &&
                    System.Web.HttpContext.Current.Session != null &&
                    System.Web.HttpContext.Current.Session.Mode == System.Web.SessionState.SessionStateMode.InProc)
                {
                    System.Web.HttpContext.Current.Session["Globals"] = value;
                }
                else
                {
                    globals = value;
                }
            }
        }

        public void SetReferences(IEnumerable<string> dllReferences)
        {
            ScriptOptions.AddReferences(dllReferences);
        }

        public void SetNamespaces(IEnumerable<string> namespaces)
        {
            ScriptOptions.AddImports(namespaces);
        }

        public void Configure(string path, IEnumerable<string> dllReferences, MetadataReferenceResolver metadataReferenceResolver, SourceReferenceResolver sourceReferenceResolver)
        {
            ScriptOptions = ScriptOptions
                .WithFilePath(path)
                .WithReferences(dllReferences)
                .WithFileEncoding(Encoding.UTF8)
                .WithEmitDebugInformation(true);

            ScriptOptions = metadataReferenceResolver != null ? ScriptOptions.WithMetadataResolver(metadataReferenceResolver) : ScriptOptions;
            ScriptOptions = sourceReferenceResolver != null ? ScriptOptions.WithSourceResolver(sourceReferenceResolver) : ScriptOptions;
        }

        public ScriptExecutionResponse Execute(string code)
        {
            var response = ExecuteScriptCode(code,
                (codeParam) => {
                    ScriptState = ScriptState == null ? CSharpScript.RunAsync(codeParam, ScriptOptions).Result : ScriptState.ContinueWithAsync(codeParam, ScriptOptions).Result;
                    return ScriptState.ReturnValue;
                });
            return response;
        }

        public ScriptExecutionResponse ExecuteDelegate(string code)
        {
            var response = ExecuteScriptCode(code,
                (codeParam) => { 
                    var script = CSharpScript.Create(codeParam, ScriptOptions).CreateDelegate();
                    return script.Invoke().Result;
                });
            return response;
        }

        public ScriptExecutionResponse ExecuteScriptCode(string code, Func<string, object> function)
        {
            var response = new ScriptExecutionResponse();
            var sw = new System.Diagnostics.Stopwatch();
            try
            {
                sw.Start();
                response.ReturnValue = function(code);
                sw.Stop();
            }
            catch (AggregateException ae)
            {
                var sb = new StringBuilder();
                int indent = 0;
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    var indentStr = new string(' ', indent);
                    sb.Append(indentStr).Append("Error Message: ").AppendLine(ex.Message);
                    sb.Append(indentStr).Append("StackTrace: ").AppendLine(ex.StackTrace);
                    sb.AppendLine();
                    indent += 4;
                }
                Console.Error.WriteLine(sb.ToString());
                return null;
            }
            finally
            {
                if (sw.IsRunning) sw.Stop();
                response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                GC.Collect();
            }
            return response;
        }

        public void ResetScriptState()
        {
            ScriptState = null;
            ScriptOptions = ScriptOptions.Default;
            Globals = null;
        }

        public ScriptState<object> GetScriptState()
        {
            return ScriptState;
        }

        public object GetVariable(string code)
        {
            var variable = ScriptState.GetVariable(code);
            if (variable == null) return null;
            return variable.Value;
        }

        public ScriptExecutionResponse Evaluate(string code)
        {
            var response = new ScriptExecutionResponse();
            var sw = new System.Diagnostics.Stopwatch();
            try
            {
                sw.Start();
                CSharpScript.EvaluateAsync(code, ScriptOptions, Globals, typeof(Globals))
                .ContinueWith(e =>
                {
                    if (e.IsFaulted)
                    {
                        var sb = new StringBuilder();
                        Exception ex = e.Exception;
                        while (ex != null)
                        {
                            sb.Append("Error Message: ").AppendLine(ex.Message);
                            sb.Append("StackTrace: ").AppendLine(ex.StackTrace);
                            sb.AppendLine();
                            Console.Error.WriteLine(sb.ToString());
                            ex = ex.InnerException;
                        }
                    }
                    response.ReturnValue = e.Result;
                }).Wait();
                sw.Stop();
            }
            catch (AggregateException ae)
            {
                var sb = new StringBuilder();
                foreach (var ex in ae.Flatten().InnerExceptions)
                {
                    sb.Append("Error Message: ").AppendLine(ex.Message);
                    sb.Append("StackTrace: ").AppendLine(ex.StackTrace);
                    sb.AppendLine();
                }
                Console.Error.WriteLine(sb.ToString());
                return null;
            }
            finally
            {
                if (sw.IsRunning) sw.Stop();
                response.ElapsedMilliseconds = sw.ElapsedMilliseconds;
                GC.Collect();
            }
            return response;
        }

        public bool IsCompleteStatement(string code)
        {
            var parseOptions = new CSharpParseOptions(LanguageVersion.Default, kind: SourceCodeKind.Script);
            var syntaxTree = SyntaxFactory.ParseSyntaxTree(code.ToString(), parseOptions);
            if (SyntaxFactory.IsCompleteSubmission(syntaxTree))
            {
                return true;
            }

            return TryWithSemicolon(syntaxTree, parseOptions);
        }

        private bool TryWithSemicolon(SyntaxTree syntaxTree, CSharpParseOptions parseOptions)
        {
            var root = syntaxTree.GetRootAsync().Result;
            var nodes = root.ChildNodes().ToList();
            if (nodes.Any()
                && nodes.First() is FieldDeclarationSyntax declaration
                && declaration.SemicolonToken.IsMissing)
            {
                var withSemicolon = declaration.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));
                var newTree = syntaxTree.WithRootAndOptions(
                    root.ReplaceNode(declaration, withSemicolon),
                    parseOptions
                );
                if (SyntaxFactory.IsCompleteSubmission(newTree))
                {
                    return true;
                }
            }
            return false;
        }

    }
}
