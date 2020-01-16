using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Abstractions
{
    public interface IScriptEngine
    {
        void Configure(string path, IEnumerable<string> dllReferences);
        ScriptExecutionResponse Execute(string code);
        ScriptExecutionResponse ExecuteDelegate(string code);
        ScriptExecutionResponse Evaluate(string code);
        void ResetScriptState();
        object GetVariable(string code);
        bool IsCompleteStatement(string code);

    }
}
