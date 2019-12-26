using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Abstractions
{
    public interface IScriptManager
    {
        void InitializeEngine(string binPath, List<string> assembliesList);
        ScriptReturnResults RunScript(ScriptParameters parameters);
    }
}
