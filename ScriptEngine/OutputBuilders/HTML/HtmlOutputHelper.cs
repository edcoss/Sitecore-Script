using ScriptSharp.ScriptEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.OutputBuilders
{
    public class HTMLOutputHelper
    {
        public static int CalculateIndentSpacing(int indentSpacing)
        {
            return indentSpacing * 10;
        }

        public static string GetPropertyAccesorIcon(AccessModifier accessModifier)
        {
            var iconUrl = "img/tools-solid.svg";
            switch (accessModifier)
            {
                case AccessModifier.Public: iconUrl = "img/tools-solid-blue4.svg"; break;
                case AccessModifier.Internal: iconUrl = "img/tools-solid-blue5.svg"; break;
                case AccessModifier.ProtectedInternal: iconUrl = "img/tools-solid-blue3.svg"; break;
                case AccessModifier.Protected: iconUrl = "img/tools-solid-blue2.svg"; break;
                case AccessModifier.Private: iconUrl = "img/tools-solid-blue6.svg"; break;
                case AccessModifier.Undefined: iconUrl = "img/tools-solid-blue1.svg"; break;
            }
            return iconUrl;
        }

        public static string GetFieldAccesorIcon(AccessModifier accessModifier)
        {
            var iconUrl = "img/tools-solid.svg";
            switch (accessModifier)
            {
                case AccessModifier.Public: iconUrl = "img/tools-solid-pink5.svg"; break;
                case AccessModifier.Internal: iconUrl = "img/tools-solid-pink3.svg"; break;
                case AccessModifier.ProtectedInternal: iconUrl = "img/tools-solid-pink2.svg"; break;
                case AccessModifier.Protected: iconUrl = "img/tools-solid-pink6.svg"; break;
                case AccessModifier.Private: iconUrl = "img/tools-solid-pink4.svg"; break;
                case AccessModifier.Undefined: iconUrl = "img/tools-solid-pink1.svg"; break;
            }
            return iconUrl;
        }

        public static string GetExpandCodeIcon()
        {
            return "img/arrow-circle-right-yellow.svg";
        }

        public static string GetCollapseIcon()
        {
            return "img/layer-group-solid-yellow.svg";
        }

        public static string GetPropertyEvalCode(AccessModifier accessModifier, EvalCode evalCode)
        {
            string evalCodeText = string.Empty;
            if (accessModifier == AccessModifier.Public)
            {
                if (evalCode.HasIndex && !string.IsNullOrWhiteSpace(evalCode.MemberName) && evalCode.HasIndexParameter)
                {
                    evalCodeText = string.Format("using System.Reflection; using System.Linq; ReturnValue.GetType()" +
                    ".GetProperty(\"{0}\", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)" +
                    ".GetValue(ReturnValue, new object[] {{ {1} }})", evalCode.MemberName, evalCode.IndexPosition);
                }
                else if (evalCode.HasIndex && !string.IsNullOrWhiteSpace(evalCode.MemberName) && !evalCode.HasIndexParameter)
                {
                    evalCodeText = string.Format("using System.Reflection; using System.Linq; ReturnValue.GetType()" +
                    ".GetProperty(\"{0}\", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)" +
                    ".GetAccessors(true)" +
                    ".FirstOrDefault(m => m.ReturnType != typeof(void))?" +
                    ".Invoke(ReturnValue, new object[] {{ {1} }})", evalCode.MemberName, evalCode.IndexPosition);
                }
                else if (!string.IsNullOrWhiteSpace(evalCode.MemberName) && !evalCode.HasIndexParameter)
                {
                    evalCodeText = string.Format("using System.Reflection; using System.Linq; ReturnValue.GetType()" +
                    ".GetProperty(\"{0}\", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)" +
                    ".GetAccessors(true)" +
                    ".FirstOrDefault(m => m.ReturnType != typeof(void))?" +
                    ".Invoke(ReturnValue, null)", evalCode.MemberName);
                }
            }
            else
            {
                if (evalCode.HasIndex && !string.IsNullOrWhiteSpace(evalCode.MemberName) && evalCode.HasIndexParameter)
                {
                    evalCodeText = string.Format("using System.Reflection; using System.Linq; ReturnValue.GetType()" +
                    ".GetProperty(\"{0}\", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)" +
                    ".GetValue(ReturnValue, new object[] {{ {1} }})", evalCode.MemberName, evalCode.IndexPosition);
                }
                else if (evalCode.HasIndex && !string.IsNullOrWhiteSpace(evalCode.MemberName) && !evalCode.HasIndexParameter)
                {
                    evalCodeText = string.Format("using System.Reflection; using System.Linq; ReturnValue.GetType()" +
                    ".GetProperty(\"{0}\", BindingFlags.Instance | BindingFlags.NonPublic |BindingFlags.Static)" +
                    ".GetAccessors(true)" +
                    ".FirstOrDefault(m => m.ReturnType != typeof(void))?" +
                    ".Invoke(ReturnValue, new object[] {{ {1} }})", evalCode.MemberName, evalCode.IndexPosition);
                }
                else if (!string.IsNullOrWhiteSpace(evalCode.MemberName) && !evalCode.HasIndexParameter)
                {
                    evalCodeText = string.Format("using System.Reflection; using System.Linq; ReturnValue.GetType()" +
                    ".GetProperty(\"{0}\", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)" +
                    ".GetAccessors(true)" +
                    ".FirstOrDefault(m => m.ReturnType != typeof(void))?" +
                    ".Invoke(ReturnValue, null)", evalCode.MemberName);
                }
            }
            return evalCodeText;
        }

        public static string GetFieldEvalCode(AccessModifier accessModifier, EvalCode evalCode)
        {
            string evalCodeText = string.Empty;
            if (accessModifier == AccessModifier.Public)
            {
                evalCodeText = string.Format("using System.Reflection; ReturnValue.GetType()" +
                    ".GetField(\"{0}\", BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static)" +
                    ".GetValue(ReturnValue)", evalCode.MemberName);
            }
            else
            {
                evalCodeText = string.Format("using System.Reflection; ReturnValue.GetType()" +
                    ".GetField(\"{0}\", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)" +
                    ".GetValue(ReturnValue)",  evalCode.MemberName);
            }
            return evalCodeText;
        }

        public static string GetEnumerableObjectEvalCode(EvalCode evalCode)
        {
            string evalCodeText = string.Format(
            "using System.Collections; " +
            "long i = 0; " +
            "object value = null; " +
            "foreach (var item in (IEnumerable)ReturnValue) " +
            "{{ if (i == {0}) value = item; i++; }} " +
            "value", evalCode.IndexPosition);
            
            return evalCodeText;
        }
    }

    
}
