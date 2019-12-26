using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine
{
    public class ScriptExecutionResponse
    {
        public object ReturnValue { get; set; }
        public string Output { get; set; }
        public string Error { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}
