using ScriptSharp.ScriptEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.Abstractions
{
    public interface IObjectOutputBuilder
    {
        bool IsHTML { get; }
        string GetOutputVariable(int indent, string objectValue);
        string GetOutputObjectValue(int indent, string objectType, object objectValue);
        string GetOutputPropertyException(int indent, string objectType, string propertyName, string exceptionMessage);
        string GetOutputEnumerableObjectWithObjectValue(int indent, string objectType, int indexPosition, object objectValue, EvalCode code);
        string GetOutputEnumerableObject(int indent, string objectType, int indexPosition, object objectValue, EvalCode code);
        string GetOutputIndexerProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, EvalCode code);
        string GetOutputEnumerableProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, EvalCode code);
        string GetOutputEnumerablePropertyWithoutValue(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, EvalCode code);
        string GetOutputEnumerablePropertyValue(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, object objectValue, EvalCode code);
        string GetOutputIndexerObjectProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, object objectValue, EvalCode code);
        string GetOutputObjectProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, object objectValue, EvalCode code);
        string GetOutputPrimitivePropertyWithIndex(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, object objectValue, EvalCode code);
        string GetOutputPrimitiveProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, object objectValue, EvalCode code);
        string GetOutputFieldObject(int indent, AccessModifier accessModifier, string objectType, string fieldName, object objectValue, EvalCode code);
        string GetOutputScriptEngineError(int indent, Exception ex);

        string BuildOutputREPL(TextWriter output, TextWriter error, TextWriter returnValueOutput, Exception ex);

        string BuildOutput(TextWriter output, TextWriter error, TextWriter returnValueOutput, Exception ex, int indent, bool eval);

    }
}
