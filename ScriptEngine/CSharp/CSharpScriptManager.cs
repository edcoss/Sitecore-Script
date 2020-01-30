using Microsoft.CodeAnalysis;
using ScriptSharp.ScriptEngine.Abstractions;
using ScriptSharp.ScriptEngine.CSharp;
using ScriptSharp.ScriptEngine.Engines;
using ScriptSharp.ScriptEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.CSharp
{
    public class CSharpScriptManager : IScriptManager
    {
        private IScriptEngine scriptEngine;
        private IObjectDebugger objectDebugger;

        public CSharpScriptManager(IScriptEngine scriptEngine, IObjectDebugger objectDebugger)
        {
            this.scriptEngine = scriptEngine;
            this.objectDebugger = objectDebugger;
        }

        public void InitializeEngine(string binPath, List<string> assembliesList, MetadataReferenceResolver metadataReferenceResolver, SourceReferenceResolver sourceReferenceResolver)
        {
            scriptEngine.ResetScriptState();
            scriptEngine.Configure(binPath, assembliesList, metadataReferenceResolver, sourceReferenceResolver);
        }

        public REPLReturnResults RunREPL(REPLParameters parameters)
        {
            var returnResults = new REPLReturnResults();
            var returnValueWriter = new StringWriter();
            var outputWriter = new StringWriter();
            var errorWriter = new StringWriter();

            try
            {
                Console.SetOut(outputWriter);
                Console.SetError(errorWriter);

                var response = scriptEngine.Execute(parameters.Code);
                if (response != null && response.ReturnValue != null)
                {
                    returnResults.ElapsedMilliseconds = response.ElapsedMilliseconds;
                    returnResults.Results = response.ReturnValue.ToString();
                }

                returnResults.Results += objectDebugger.OutputBuilder.BuildOutputREPL(outputWriter, errorWriter, returnValueWriter, null);
            }
            catch (Exception ex)
            {
                returnResults.IsError = true;
                returnResults.Results += objectDebugger.OutputBuilder.BuildOutputREPL(outputWriter, errorWriter, returnValueWriter, ex);
                returnResults.ElapsedMilliseconds = 0;
            }
            finally
            {
                returnValueWriter.Dispose();
                outputWriter.Dispose();
                errorWriter.Dispose();
            }
            return returnResults;
        }

        public ScriptReturnResults RunScript(ScriptParameters parameters)
        {
            var returnResults = new ScriptReturnResults();
            var returnValueWriter = new StringWriter();
            var outputWriter = new StringWriter();
            var errorWriter = new StringWriter();
            var startIndent = parameters.StartIndent;

            returnResults.IsHTMLResult = objectDebugger.OutputBuilder.IsHTML;
            objectDebugger.Settings = new ObjectDebuggerSettings()
            {
                DisplayTypesForObjectDetails = parameters.DisplayTypesForObjectDetails,
                SkipTypesForPropertyDetails = parameters.SkipTypesForPropertyDetails,
                SkipTypesForValueReprint = parameters.SkipTypesForValueReprint,
                TypesAsPrimitivesForValuePrint = parameters.TypesAsPrimitivesForValuePrint,
                EvalCode = parameters.EvalCode,
                Eval = parameters.Eval,
                MaxIndent = parameters.MaxIndent
            };           

            try
            {
                var varResponse = GetVariableDetails(parameters.Code, startIndent);
                if (!string.IsNullOrWhiteSpace(varResponse))
                {
                    returnResults.Results = objectDebugger.OutputBuilder.GetOutputVariable(startIndent, varResponse);
                    return returnResults;
                }
            }
            catch
            {
                // Don't do anything 
            }

            try
            {
                Console.SetOut(outputWriter);
                Console.SetError(errorWriter);
                ScriptExecutionResponse response = null;
                long totalElapsedMilliseconds = 0;

                response = scriptEngine.Execute(parameters.Code);
                if (response != null)
                {
                    totalElapsedMilliseconds += response.ElapsedMilliseconds;
                    CSharpScriptEngine.Globals = new Globals() { ReturnValue = response.ReturnValue };
                }

                if (parameters.Eval)
                {
                    foreach (var evaluationCode in parameters.EvalCode)
                    {
                        response = scriptEngine.Evaluate(evaluationCode);
                        if (response != null)
                        {
                            totalElapsedMilliseconds += response.ElapsedMilliseconds;
                            CSharpScriptEngine.Globals = new Globals() { ReturnValue = response.ReturnValue };
                        }
                    }
                }

                if (response != null)
                {                    
                    returnResults.ElapsedMilliseconds = totalElapsedMilliseconds;
                    try
                    {
                        objectDebugger.GenerateObjectDebugDetails(response.ReturnValue, startIndent, returnValueWriter);
                        returnResults.Results = objectDebugger.ProcessOutput(outputWriter, errorWriter, returnValueWriter, null, startIndent);
                    }
                    catch (Exception ex)
                    {
                        returnResults.Results = objectDebugger.ProcessOutput(outputWriter, errorWriter, returnValueWriter, ex, startIndent);
                    }
                }
                else
                {
                    returnResults.Results = objectDebugger.ProcessOutput(outputWriter, errorWriter, returnValueWriter, null, startIndent);
                }
            }
            catch (Exception ex)
            {
                returnResults.Results = objectDebugger.OutputBuilder.GetOutputScriptEngineError(startIndent, ex);
                returnResults.ElapsedMilliseconds = 0;
            }
            finally
            {
                returnValueWriter.Dispose();
                outputWriter.Dispose();
                errorWriter.Dispose();
            }
            return returnResults;
        }

        public bool IsCompilableCode(string code)
        {
            return scriptEngine.IsCompleteStatement(code);
        }

        private string GetVariableDetails(string code, int startIndent)
        {
            string variableDetails = string.Empty;
            var value = scriptEngine.GetVariable(code);
            using (var writer = new System.IO.StringWriter())
            {
                objectDebugger.GenerateObjectDebugDetails(value, startIndent, writer);
                variableDetails = writer.ToString();
            }
            return variableDetails;
        }

        private string GetEvalDetails(string code, int startIndent)
        {
            string evalDetails = string.Empty;
            var value = scriptEngine.Evaluate(code);
            using (var writer = new System.IO.StringWriter())
            {
                objectDebugger.GenerateObjectDebugDetails(value, startIndent, writer);
                evalDetails = writer.ToString();
            }
            return evalDetails;
        }

    }
}
