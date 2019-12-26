using ScriptSharp.ScriptEngine.Abstractions;
using ScriptSharp.ScriptEngine.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.OutputBuilders
{
    public class HTMLObjectOutputBuilder : IObjectOutputBuilder
    {
        public bool IsHTML => true;

        private string VariableFormat => "<div class='variable-result-label'>Variable</div>" +
            "<div class='variable-result'>{0}</div>";

        public string GetOutputVariable(int indent, string objectValue)
        {
            return string.Format(VariableFormat, objectValue);
        }

        private string ScriptEngineErrorFormat => "<div class='scripting-engine-error'>ScriptEngine Error Message:</div>" +
            "<div class='scripting-engine-error-message'>{0}</div>" +
            "<div class='scripting-engine-error-stacktrace'>{1}</div>";

        public string GetOutputScriptEngineError(int indent, Exception ex)
        {
            return string.Format(ScriptEngineErrorFormat, ex.Message, ex.StackTrace);
        }

        private string ObjectValueFormat => "<div class='object-value-format'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<span class='object-type'>({1})</span> --> \n" +
                "<span class='object-value'>{2}</span>\n" +
            "</div>\n";

        public string GetOutputObjectValue(int indent, string objectType, object objectValue)
        {
            return string.Format(ObjectValueFormat, HTMLOutputHelper.CalculateIndentSpacing(indent), objectType, objectValue);
        }

        private string PropertyExceptionOutputFormat => "<div class='property-exception'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='property-name'>{2}</span>:\n" +
                "<span class='exception-tag'>Exception</span>\n" +
                "<span class='exception-message'>{3}</span>\n" +
            "</div>\n";

        public string GetOutputPropertyException(int indent, string objectType, string propertyName, string exceptionMessage)
        {
            return string.Format(PropertyExceptionOutputFormat, HTMLOutputHelper.CalculateIndentSpacing(indent), objectType, propertyName, exceptionMessage);
        }

        private string EnumerableObjectWithObjectValueOutputFormat => "<div class='enumerable-object-with-object-value'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{4}' data-indent='{5}' data-icon='{6}'>" +
                "<img src='{7}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<span class='enumerable-tag'>Enumerable</span> -\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='index-position'>[{2}]</span> --> \n" +
                "<span class='object-value'>{3}</span>\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputEnumerableObjectWithObjectValue(int indent, string objectType, int indexPosition, object objectValue, EvalCode evalCode)
        {
            var expandCodeIcon = HTMLOutputHelper.GetExpandCodeIcon();
            var collapseCodeIcon = HTMLOutputHelper.GetCollapseIcon();
            var evalCodeText = HTMLOutputHelper.GetEnumerableObjectEvalCode(evalCode);

            return string.Format(EnumerableObjectWithObjectValueOutputFormat,
                HTMLOutputHelper.CalculateIndentSpacing(indent),
                objectType,
                indexPosition,
                objectValue,
                evalCodeText,
                indent,
                collapseCodeIcon,
                expandCodeIcon);
        }

        private string EnumerableObjectOutputFormat => "<div class='enumerable-object'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{4}' data-indent='{5}' data-icon='{6}'>" +
                "<img src='{7}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<span class='enumerable-tag'>Enumerable</span> -\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='index-position'>[{2}]</span>\n" +
                "<span class='object-value'>{3}</span>\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputEnumerableObject(int indent, string objectType, int indexPosition, object objectValue, EvalCode evalCode)
        {
            var expandCodeIcon = HTMLOutputHelper.GetExpandCodeIcon();
            var collapseCodeIcon = HTMLOutputHelper.GetCollapseIcon();
            var evalCodeText = HTMLOutputHelper.GetEnumerableObjectEvalCode(evalCode);

            return string.Format(EnumerableObjectOutputFormat,
                HTMLOutputHelper.CalculateIndentSpacing(indent),
                objectType,
                indexPosition,
                objectValue,
                evalCodeText,
                indent,
                collapseCodeIcon, expandCodeIcon);
        }

        private string IndexerPropertyOutputFormat => "<div class='indexer-property'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{4}' data-indent='{5}' data-icon='{6}'>" +
                "<img src='{7}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<img src='{8}' alt='{9}' title='{9}'> - \n" +
                "<img src='{10}' alt='{11}' title='{11}'>\n" +
                "<span class='indexer-tag'>Indexer</span> -\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='property-name'>{2}</span>\n" +
                "<span class='index-position'>[{3}]</span>\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputIndexerProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, getAccessModifier, setAccessModifier);
            return string.Format(IndexerPropertyOutputFormat, data.Indent,
                objectType,
                propertyName,
                indexPosition,
                data.PropertyEvalCodeText,
                indent, 
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.PropertyGetAccessModifierIcon,
                data.PropertyGetAccessModifierDescription,
                data.PropertySetAccessModifierIcon,
                data.PropertySetAccessModifierDescription);
        }

        private string EnumerablePropertyOutputFormat => "<div class='enumerable-property'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{3}' data-indent='{4}' data-icon='{5}'>" +
                "<img src='{6}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<img src='{7}' alt='{8}' title='{8}'> - \n" +
                "<img src='{9}' alt='{10}' title='{10}'>\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='property-name'>{2}</span>: [+]\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputEnumerableProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, getAccessModifier, setAccessModifier);
            return string.Format(EnumerablePropertyOutputFormat,
                data.Indent,
                objectType,
                propertyName,
                data.PropertyEvalCodeText,
                indent,
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.PropertyGetAccessModifierIcon,
                data.PropertyGetAccessModifierDescription,
                data.PropertySetAccessModifierIcon,
                data.PropertySetAccessModifierDescription);
        }

        private string EnumerablePropertyWithoutValueOutputFormat => "<div class='enumerable-property-without-value'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{3}' data-indent='{4}' data-icon='{5}'>" +
                "<img src='{6}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<img src='{7}' alt='{8}' title='{8}'> - \n" +
                "<img src='{9}' alt='{10}' title='{10}'>\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='property-name'>{2}</span>: [+]\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputEnumerablePropertyWithoutValue(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, getAccessModifier, setAccessModifier);
            return string.Format(EnumerablePropertyWithoutValueOutputFormat,
                data.Indent,
                objectType,
                propertyName,
                data.PropertyEvalCodeText,
                indent,
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.PropertyGetAccessModifierIcon,
                data.PropertyGetAccessModifierDescription,
                data.PropertySetAccessModifierIcon,
                data.PropertySetAccessModifierDescription);
        }

        private string EnumerablePropertyValueOutputFormat => "<div class='enumerable-property-value'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{3}' data-indent='{4}' data-icon='{5}'>" +
                "<img src='{6}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<img src='{7}' alt='{8}' title='{8}'> - \n" +
                "<img src='{9}' alt='{10}' title='{10}'>\n" +
                "<span class='object-type'>({1})</span> => " +
                "<span class='object-value'>{2}</span>\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputEnumerablePropertyValue(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, object objectValue, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, getAccessModifier, setAccessModifier);
            return string.Format(EnumerablePropertyValueOutputFormat,
                data.Indent,
                objectType,
                objectValue,
                data.PropertyEvalCodeText,
                indent,
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.PropertyGetAccessModifierIcon,
                data.PropertyGetAccessModifierDescription,
                data.PropertySetAccessModifierIcon,
                data.PropertySetAccessModifierDescription);
        }

        private string IndexerObjectPropertyOutputFormat => "<div class='indexer-object-property'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{5}' data-indent='{6}' data-icon='{7}'><img src='{8}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<img src='{9}' alt='{10}' title='{10}'> - \n" +
                "<img src='{11}' alt='{12}' title='{12}'>\n" +
                "<span class='indexer-tag'>Indexer</span> -\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='property-name'>{2}</span>\n" +
                "<span class='index-position'>[{3}]</span>:\n" +
                "<span class='object-value'>{4}</span>\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputIndexerObjectProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, object objectValue, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, getAccessModifier, setAccessModifier);
            return string.Format(IndexerObjectPropertyOutputFormat,
                data.Indent,
                objectType,
                propertyName,
                indexPosition,
                objectValue,
                data.EnumerableEvalCodeText,
                indent,
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.PropertyGetAccessModifierIcon,
                data.PropertyGetAccessModifierDescription,
                data.PropertySetAccessModifierIcon,
                data.PropertySetAccessModifierDescription);
        }

        private string ObjectPropertyOutputFormat => "<div class='object-property'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{4}' data-indent='{5}' data-icon='{6}'><img src='{7}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<img src='{8}' alt='{9}' title='{9}'> - \n" +
                "<img src='{10}' alt='{11}' title='{11}'>\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='property-name'>{2}</span>:\n" +
                "<span class='object-value'>{3}</span>\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputObjectProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, object objectValue, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, getAccessModifier, setAccessModifier);
            return string.Format(ObjectPropertyOutputFormat,
                data.Indent,
                objectType,
                propertyName,
                objectValue,
                data.PropertyEvalCodeText,
                indent,
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.PropertyGetAccessModifierIcon,
                data.PropertyGetAccessModifierDescription,
                data.PropertySetAccessModifierIcon,
                data.PropertySetAccessModifierDescription);
        }

        private string PrimitivePropertyWithIndexOutputFormat => "<div class='primitive-property-with-index'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{5}' data-indent='{6}' data-icon='{7}'>" +
                "<img src='{8}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<img src='{9}' alt='{10}' title='{10}'> - \n" +
                "<img src='{11}' alt='{12}' title='{12}'>\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='property-name'>{2}</span>\n" +
                "<span class='index-position'>[{3}]</span>:\n" +
                "<span class='object-value'>{4}</span>\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputPrimitivePropertyWithIndex(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, int indexPosition, object objectValue, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, getAccessModifier, setAccessModifier);
            return string.Format(PrimitivePropertyWithIndexOutputFormat,
                data.Indent,
                objectType,
                propertyName,
                indexPosition,
                objectValue,
                data.PropertyEvalCodeText,
                indent,
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.PropertyGetAccessModifierIcon,
                data.PropertyGetAccessModifierDescription,
                data.PropertySetAccessModifierIcon,
                data.PropertySetAccessModifierDescription);
        }

        private string PrimitivePropertyOutputFormat => "<div class='primitive-property'>\n" +
                "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
                "<a class='expand-code' href='#' data-code='{4}' data-indent='{5}' data-icon='{6}'>" +
                "<img src='{7}' alt='Expand' title='Click to expand/collapse'></a>\n" +
                "<img src='{8}' alt='{9}' title='{9}'> - \n" +
                "<img src='{10}' alt='{11}' title='{11}'>\n" +
                "<span class='object-type'>({1})</span>\n" +
                "<span class='property-name'>{2}</span>:\n" +
                "<span class='object-value'>{3}</span>\n" +
                "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputPrimitiveProperty(int indent, AccessModifier getAccessModifier, AccessModifier setAccessModifier, string objectType, string propertyName, object objectValue, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, getAccessModifier, setAccessModifier);
            return string.Format(PrimitivePropertyOutputFormat,
                data.Indent,
                objectType,
                propertyName,
                objectValue,
                data.PropertyEvalCodeText,
                indent,
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.PropertyGetAccessModifierIcon,
                data.PropertyGetAccessModifierDescription,
                data.PropertySetAccessModifierIcon,
                data.PropertySetAccessModifierDescription);
        }

        private string FieldObjectOutputFormat => "<div class='field-object'>\n" +
            "<span class='indentation-padding' style='padding-left: {0}px'></span>\n" +
            "<a class='expand-code' href='#' data-code='{4}' data-indent='{5}' data-icon='{6}'>" +
            "<img src='{7}' alt='Expand' title='Click to expand/collapse'></a>\n" +
            "<img src='{8}' alt='{9}' title='{9}'>\n" +
            "<span class='field-type'>({1})</span>\n" +
            "<span class='field-name'>{2}</span>:\n" +
            "<span class='field-value'>{3}</span>\n" +
            "<div class='expanded-code'></div>\n" +
            "</div>\n";

        public string GetOutputFieldObject(int indent, AccessModifier accessModifier, string objectType, string fieldName, object objectValue, EvalCode evalCode)
        {
            var data = new HTMLOutputParameters(indent, evalCode, accessModifier);
            return string.Format(FieldObjectOutputFormat,
                data.Indent,
                objectType,
                fieldName,
                objectValue,
                data.FieldEvalCodeText,
                indent,
                data.CollapseCodeIcon,
                data.ExpandCodeIcon,
                data.FieldAccessModifierIcon,
                data.FieldAccessModifierDescription);
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

            if (outputSb.Length > 0 && !eval)
            {
                var htmlHeaderFormat = 
                    "<div class='script-output'>" +
                    "<span class='indentation-padding' style='padding-left: {0}px'></span>" +
                    "<span>Script:Output</span>" +
                    "</div>";

                var htmlHeaderOutput = string.Format(htmlHeaderFormat, HTMLOutputHelper.CalculateIndentSpacing(indent));
                sb.AppendLine(htmlHeaderOutput);
                sb.AppendLine(ReplaceNewLines(outputSb.ToString()));
            }
            if (errorSb.Length > 0)
            {
                var errorHeaderFormat =
                    "<div class='script-error'>" +
                    "<span class='indentation-padding' style='padding-left: {0}px'></span>" +
                    "<span>Script:Error</span>" +
                    "</div>";

                var errorHeaderOutput = string.Format(errorHeaderFormat, HTMLOutputHelper.CalculateIndentSpacing(indent));
                sb.AppendLine(errorHeaderOutput);
                sb.AppendLine(ReplaceNewLines(errorSb.ToString()));
            }
            if (returnValueSb.Length > 0)
            {
                var returnValueHeaderFormat = 
                    "<div class='script-return-value'>" +
                    "<span class='indentation-padding' style='padding-left: {0}px'></span>" +
                    "<span>Script:ReturnValue</span>" +
                    "</div>";

                var returnValueHeaderOutput = string.Format(returnValueHeaderFormat, HTMLOutputHelper.CalculateIndentSpacing(indent));
                sb.AppendLine(returnValueHeaderOutput);
                sb.AppendLine(returnValueSb.ToString());
            }
            else
            {
                if (outputSb.Length == 0)
                {
                    var emptyResponseHeaderFormat = 
                        "<div class='script-empty-response'>" +
                        "<span class='indentation-padding' style='padding-left: {0}px'>" +
                        "</span><span>Script:EmptyResponse</span>" +
                        "</div>";

                    var emptyResponseHeaderOutput = string.Format(emptyResponseHeaderFormat, HTMLOutputHelper.CalculateIndentSpacing(indent));
                    sb.AppendLine(emptyResponseHeaderOutput);
                }
            }
            if (ex != null)
            {
                sb.AppendLine("<div class='script-error-thrown'><span class='indentation-padding'></span><span>Script:ErrorThrown</span></div>");
                sb.AppendLine("<div class='script-error-message'><span class='indentation-padding'></span><span>" + ex.Message + "</span></div>");
                sb.AppendLine("<div class='script-error-stacktrace'><span class='indentation-padding'></span><span>" + ex.StackTrace + "</span></div>");
            }
            if (indent == 0)
            {
                sb.AppendLine("<div class='script-ready'><span class='indentation-padding'></span><span>Ready</span></div>");
            }

            return sb.ToString();
        }

        private string ReplaceNewLines(string text)
        {
            return text.Replace("\n", "<br/>");
        }
    }
}
