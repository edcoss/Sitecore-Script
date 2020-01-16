using ScriptSharp.ScriptEngine.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Models
{
    public class ScriptParameters
    {
        public bool Eval { get; set; }
        public string Code { get; set; }
        public string[] EvalCode { get; set; }
        public int StartIndent { get; set; }
        public int MaxIndent { get; set; }
        public IObjectOutputBuilder OutputFormatter { get; set; }
        public IEnumerable<Type> DisplayTypesForObjectDetails { get; set; }
        public IEnumerable<Type> SkipTypesForPropertyDetails { get; set; }
        public IEnumerable<Type> SkipTypesForValueReprint { get; set; }
        public IEnumerable<Type> TypesAsPrimitivesForValuePrint { get; set; }
    }
}
