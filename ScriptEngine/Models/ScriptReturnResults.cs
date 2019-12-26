using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine
{
    public class ScriptReturnResults
    {
        public bool IsHTMLResult { get; set; }
        public string Results { get; set; }
        public long ElapsedMilliseconds { get; set; }
    }
}
