using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Models
{
    public class ObjectDebuggerSettings
    {
        public int MaxIndent { get; set; }
        public IEnumerable<Type> DisplayTypesForObjectDetails { get; set; }
        public IEnumerable<Type> SkipTypesForPropertyDetails { get; set; }
        public IEnumerable<Type> SkipTypesForValueReprint { get; set; }
        public IEnumerable<Type> TypesAsPrimitivesForValuePrint { get; set; }

        public System.IO.TextWriter Writer { get; set; }
        public string[] EvalCode { get; set; }
        public bool Eval { get; set; }
    }
}
