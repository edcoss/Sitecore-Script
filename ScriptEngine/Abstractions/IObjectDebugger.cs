using ScriptSharp.ScriptEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Abstractions
{
    public interface IObjectDebugger
    {
        IObjectOutputBuilder OutputBuilder { get; }
        ObjectDebuggerSettings Settings { get; set; }
        void GenerateObjectDebugDetails(object obj, int indent, System.IO.TextWriter writer);
        void GeneratePropertyDebugDetails(object propertyValue, System.Reflection.PropertyInfo property, int indent, bool hasIndex = false, int indexPos = 0);
        string ProcessOutput(System.IO.TextWriter output, System.IO.TextWriter error, System.IO.TextWriter returnValueOutput, Exception ex = null, int indent = 0);
    }
}
