using ScriptSharp.ScriptEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.OutputBuilders
{
    public class HTMLOutputParameters
    {
        private AccessModifier getAccessModifier;
        private AccessModifier setAccessModifier;
        private AccessModifier fieldAccessModifier;
        private EvalCode evalCode;
        private int indent;

        public HTMLOutputParameters(int indent, EvalCode evalCode, AccessModifier getAccessModifier, AccessModifier setAccessModifier)
        {
            this.getAccessModifier = getAccessModifier;
            this.setAccessModifier = setAccessModifier;
            this.evalCode = evalCode;
            this.indent = indent;
        }

        public HTMLOutputParameters(int indent, EvalCode evalCode, AccessModifier fieldAccessModifier)
        {
            this.fieldAccessModifier = fieldAccessModifier;
            this.evalCode = evalCode;
            this.indent = indent;
        }

        public int Indent => HTMLOutputHelper.CalculateIndentSpacing(indent);
        public string ExpandCodeIcon => HTMLOutputHelper.GetExpandCodeIcon();
        public string CollapseCodeIcon => HTMLOutputHelper.GetCollapseIcon();
        public string PropertyGetAccessModifierIcon => HTMLOutputHelper.GetPropertyAccesorIcon(getAccessModifier);
        public string PropertySetAccessModifierIcon => HTMLOutputHelper.GetPropertyAccesorIcon(setAccessModifier);
        public string PropertyGetAccessModifierDescription => string.Format("{0} property get;", getAccessModifier.ToString());
        public string PropertySetAccessModifierDescription => string.Format("{0} property set;", setAccessModifier.ToString());
        public string PropertyEvalCodeText => HTMLOutputHelper.GetPropertyEvalCode(getAccessModifier, evalCode);
        public string FieldAccessModifierDescription => string.Format("{0} field", fieldAccessModifier.ToString());
        public string FieldAccessModifierIcon => HTMLOutputHelper.GetFieldAccesorIcon(fieldAccessModifier);
        public string FieldEvalCodeText => HTMLOutputHelper.GetFieldEvalCode(fieldAccessModifier, evalCode);
        public string EnumerableEvalCodeText => HTMLOutputHelper.GetEnumerableObjectEvalCode(evalCode);

    }
}
