using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Models
{
    public class EvalCode
    {
        public string MemberName { get; set; }
        public bool HasIndex { get; set; }
        public int IndexPosition { get; set; }
        public string ObjectType { get; set; }

        public bool HasIndexParameter { get; set; }
    }
}
