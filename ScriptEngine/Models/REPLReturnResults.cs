using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Models
{
    public class REPLReturnResults
    {
        public string Results { get; set; }
        public bool IsBalanced { get; set; }
        public long ElapsedMilliseconds { get; set; }
        public bool IsError { get; set; }
    }
}
