using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScriptSharp.ScriptEngine.Abstractions;
using ScriptSharp.ScriptEngine.Models;

namespace ScriptSharp.ScriptEngine.OutputBuilders
{
    public class SimpleTextObjectOutputBuilder : IObjectOutputBuilder
    {
        public bool IsHTML => false;

        public string VariableFormat
        {
            get { return "<Variable>{0}{1}{2}"; }
        }

        public string GetOutputVariable(int indent, string objectValue)
        {
            return string.Format(VariableFormat, Environment.NewLine, objectValue, Environment.NewLine);
        }
        public string ObjectValueFormat
        {
            get { return  "{0} ({1}) --> {2}"; }
        }

        public string GetOutputObjectValue(int indent, string objectType, object objectValue)
        {
            return string.Format(ObjectValueFormat, new string(' ', indent), objectType, objectValue);
        }

        public string PropertyExceptionOutputFormat
        {
            get { return "{0} ({1}) {2}: Exception <{3}>"; }
        }

        public string GetOutputPropertyException(int indent, string objectType, string propertyName, string exceptionMessage)
        {
            return string.Format(PropertyExceptionOutputFormat, new string(' ', indent), objectType, propertyName, exceptionMessage);
        }

        public string EnumerableObjectWithObjectValueOutputFormat
        {
            get { return "{0}Enumerable-({1}) [{2}] --> {3}"; }
        }

        public string GetOutputEnumerableObjectWithObjectValue(int indent, string objectType, int indexPosition, object objectValue, EvalCode code)
        {
            return string.Format(EnumerableObjectWithObjectValueOutputFormat, new string(' ', indent), objectType, indexPosition, objectValue);
        }

        public string EnumerableObjectOutputFormat
        {
            get { return "{0}Enumerable-({1}) [{2}]"; }
        }

        public string GetOutputEnumerableObject(int indent, string objectType, int indexPosition, object objectValue, EvalCode code)
        {
            return string.Format(EnumerableObjectOutputFormat, new string(' ', indent), objectType, indexPosition);
        }

        public string IndexerPropertyOutputFormat
        {
            get { return "{0}Indexer-({1}) {2}[{3}]: [+]"; }
        }

        public string GetOutputIndexerProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, EvalCode code)
        {
            return string.Format(IndexerPropertyOutputFormat, new string(' ', indent), objectType, propertyName, indexPosition);
        }

        public string EnumerablePropertyOutputFormat
        {
            get { return "{0}({1}) {2}: [+]"; }
        }

        public string GetOutputEnumerableProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, EvalCode code)
        {
            return string.Format(EnumerablePropertyOutputFormat, new string(' ', indent), objectType, propertyName);
        }

        public string EnumerablePropertyValueOutputFormat
        {
            get { return "{0}({1}) => {2}"; }
        }

        public string GetOutputEnumerablePropertyValue(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, object objectValue, EvalCode code)
        {
            return string.Format(EnumerablePropertyValueOutputFormat, new string(' ', indent), objectType, objectValue);
        }

        public string EnumerablePropertyWithoutValueOutputFormat
        {
            get { return "{0}({1})"; }
        }

        public string GetOutputEnumerablePropertyWithoutValue(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, EvalCode code)
        {
            return string.Format(EnumerablePropertyWithoutValueOutputFormat, new string(' ', indent), objectType);
        }

        public string IndexerObjectPropertyOutputFormat
        {
            get { return "{0}Indexer-({1}) {2}[{3}]: {4}"; }
        }

        public string GetOutputIndexerObjectProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, object objectValue, EvalCode code)
        {
            return string.Format(IndexerObjectPropertyOutputFormat, new string(' ', indent), objectType, propertyName, indexPosition, objectValue);
        }

        public string ObjectPropertyOutputFormat
        {
            get { return "{0}({1}) {2}: {3}"; }
        }

        public string GetOutputObjectProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, object objectValue, EvalCode code)
        {
            return string.Format(ObjectPropertyOutputFormat, new string(' ', indent), objectType, propertyName, objectValue);
        }

        public string PrimitivePropertyWithIndexOutputFormat
        {
            get { return "{0}({1}) {2}[{3}]: {4}"; }
        }

        public string GetOutputPrimitivePropertyWithIndex(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, object objectValue, EvalCode code)
        {
            return string.Format(PrimitivePropertyWithIndexOutputFormat, new string(' ', indent), objectType, propertyName, indexPosition, objectValue);
        }

        public string PrimitivePropertyOutputFormat
        {
            get { return "{0}({1}) {2}: {3}"; }
        }

        public string GetOutputPrimitiveProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, object objectValue, EvalCode code)
        {
            return string.Format(PrimitivePropertyOutputFormat, new string(' ', indent), objectType, propertyName, objectValue);
        }

        public string FieldObjectOutputFormat
        {
            get { return "{0}({1}) {2}: {3}"; }
        }

        public string GetOutputFieldObject(int indent, AccessModifier accessModifier, string objectType, string fieldName, object objectValue, EvalCode code)
        {
            return string.Format(FieldObjectOutputFormat, new string(' ', indent), objectType, fieldName, objectValue);
        }

        public string GetOutputScriptEngineError(int indent, Exception ex)
        {
            return string.Format("ScriptEngine Error Message: {0}{1}{2}{3}", ex.Message, Environment.NewLine, ex.StackTrace, Environment.NewLine);
        }

        public string BuildOutput(TextWriter output, TextWriter error, TextWriter returnValueOutput, Exception ex, int indent, bool eval)
        {
            StringWriter swOutput = output as StringWriter;
            StringWriter swError = error as StringWriter;
            StringWriter swReturnValueOutput = returnValueOutput as StringWriter;

            swOutput.Flush();
            swError.Flush();
            swReturnValueOutput.Flush();

            var sb = new StringBuilder();
            var outputSb = swOutput.GetStringBuilder();
            var errorSb = swError.GetStringBuilder();
            var returnValueSb = swReturnValueOutput.GetStringBuilder();

            if (outputSb.Length > 0)
            {
                sb.AppendLine("<Script:Output>");
                sb.AppendLine(outputSb.ToString());
            }
            if (errorSb.Length > 0)
            {
                sb.AppendLine("<Script:Error>");
                sb.AppendLine(errorSb.ToString());
            }
            if (returnValueSb.Length > 0)
            {
                sb.AppendLine("<Script:ReturnValue>");
                sb.AppendLine(returnValueSb.ToString());
            }
            else
            {
                if (outputSb.Length == 0) sb.AppendLine("<Script:EmptyResponse>");
            }
            if (ex != null)
            {
                sb.AppendLine("<Script:ErrorThrown>");
                sb.AppendLine(ex.Message);
                sb.AppendLine(ex.StackTrace);
            }
            sb.AppendLine("<Ready>");

            return sb.ToString();
        }
    }
}
