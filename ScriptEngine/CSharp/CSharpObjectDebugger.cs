using ScriptSharp.ScriptEngine.Abstractions;
using ScriptSharp.ScriptEngine.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine.CSharp
{
    public class CSharpObjectDebugger : IObjectDebugger
    {
        public int MaxIndent { get; set; }
        public IObjectOutputBuilder OutputBuilder { get; private set; }
        public ObjectDebuggerSettings Settings { get; set; }

        public CSharpObjectDebugger(IObjectOutputBuilder outputBuilder)
        {
            this.Settings = new ObjectDebuggerSettings()
            {
                DisplayTypesForObjectDetails = new List<Type>(),
                SkipTypesForPropertyDetails = new List<Type>(),
                SkipTypesForValueReprint = new List<Type>(),
                TypesAsPrimitivesForValuePrint = new List<Type>(),
                EvalCode = new string[] { },
            };
            this.MaxIndent = int.MaxValue;
            this.OutputBuilder = outputBuilder;
        }

        public void GenerateObjectDebugDetails(object obj, int indent, System.IO.TextWriter writer)
        {
            if (indent > MaxIndent) return;
            if (obj == null) return;

            if (Settings.Writer == null) Settings.Writer = writer;

            Type objType = obj.GetType();
            var properties = objType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).OrderBy(p => p.Name).ToArray();
            var fields = objType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).OrderBy(f => f.Name).ToArray();

            // don't display values on complex objects, only primitives and displayable types
            if (objType.IsPrimitive ||
                Settings.DisplayTypesForObjectDetails.Contains(objType) ||
                properties.Length == 0 ||
                fields.Length == 0)
            {
                writer.WriteLine(OutputBuilder.GetOutputObjectValue(indent, objType.ToString(), obj));
            }

            // skip deeper analysis if primitive or skippable type
            if (objType.IsPrimitive || Settings.SkipTypesForPropertyDetails.Contains(objType))
            {
                return;
            }

            // Process object if it has properties and fields
            // Or if it is a complex object like an array or enumerable
            ProcessObjectProperties(properties, obj, indent);
            ProcessEnumerableObject(objType, obj, indent);
            ProcessObjectFields(fields, obj, indent);
        }

        private void ProcessObjectProperties(PropertyInfo[] properties, object obj, int indent)
        {
            foreach (PropertyInfo property in properties)
            {
                try
                {
                    property.GetMethod.GetAccessModifier();
                    object propValue = null;
                    var indexParams = property.GetIndexParameters();
                    if (indexParams.Length == 0)
                    {
                        propValue = property.GetValue(obj, null);
                        GeneratePropertyDebugDetails(propValue, property, indent + 2);
                    }
                    else
                    {
                        // Indexer property
                        var length = obj.GetPropertyValue<int>("Length");
                        if (length == 0) length = obj.GetPropertyValue<int>("Count");
                        for (int i = 0; i < length; i++)
                        {
                            var item = property.GetValue(obj, new object[] { i });
                            GeneratePropertyDebugDetails(item, property, indent + 2, hasIndex: true, indexPos: i);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Settings.Writer.WriteLine(OutputBuilder.GetOutputPropertyException(indent, property.PropertyType.ToString(), property.Name, ex.Message));
                }
            }
        }

        private void ProcessObjectFields(FieldInfo[] fields, object obj, int indent)
        {
            foreach (FieldInfo field in fields)
            {
                try
                {
                    object fieldValue = field.GetValue(obj);
                    Type fieldType = field.FieldType;
                    AccessModifier accessModifier = field.GetAccessModifier();
                    Settings.Writer.WriteLine(OutputBuilder.GetOutputFieldObject(indent + 2, accessModifier, fieldType.ToString(), field.Name, fieldValue, new Models.EvalCode() { MemberName = field.Name }));
                }
                catch (Exception ex)
                {
                    Settings.Writer.WriteLine(OutputBuilder.GetOutputPropertyException(indent, field.FieldType.ToString(), field.Name, ex.Message));
                }
            }
        }

        private void ProcessEnumerableObject(Type objType, object obj, int indent)
        {
            if (objType.IsArray || obj is IEnumerable)
            {
                int j = 0;
                foreach (var element in (IEnumerable)obj)
                {
                    var elementType = element.GetType().ToString();
                    if (!Settings.SkipTypesForValueReprint.Contains(elementType.GetType()))
                        Settings.Writer.WriteLine(OutputBuilder.GetOutputEnumerableObjectWithObjectValue(indent + 2, elementType, j, element, new Models.EvalCode() { HasIndex = true, IndexPosition = j }));
                    else
                        Settings.Writer.WriteLine(OutputBuilder.GetOutputEnumerableObject(indent + 2, elementType, j, element, new Models.EvalCode() { HasIndex = true, IndexPosition = j }));
                    j++;
                }
            }
        }

        public void GeneratePropertyDebugDetails(object propertyValue, PropertyInfo property, int indent, bool hasIndex = false, int indexPos = 0)
        {
            if (!property.PropertyType.IsPrimitive &&
                !Settings.TypesAsPrimitivesForValuePrint.Contains(property.PropertyType))
            {
                // this applies for non-primitive types
                if (propertyValue is IEnumerable)
                {
                    var elems = propertyValue as IEnumerable;
                    if (elems != null)
                    {
                        if (hasIndex)
                            Settings.Writer.WriteLine(OutputBuilder.GetOutputIndexerProperty(
                                indent,
                                property.AccessModifierForGet(),
                                property.AccessModifierForSet(),
                                property.PropertyType.ToString(),
                                property.Name,
                                indexPos,
                                new Models.EvalCode() { HasIndex = true, IndexPosition = indexPos, MemberName = property.Name }));
                        else
                            Settings.Writer.WriteLine(OutputBuilder.GetOutputEnumerableProperty(
                                indent,
                                property.AccessModifierForGet(),
                                property.AccessModifierForSet(),
                                property.PropertyType.ToString(),
                                property.Name,
                                new Models.EvalCode() { MemberName = property.Name }));
                    }
                }
                else
                {
                    // propertyValue is not an IEnumerable object

                    if (hasIndex)
                        Settings.Writer.WriteLine(OutputBuilder.GetOutputIndexerObjectProperty(
                            indent,
                            property.AccessModifierForGet(),
                            property.AccessModifierForSet(),
                            property.PropertyType.ToString(),
                            property.Name,
                            indexPos,
                            propertyValue ?? "<null>",
                            new Models.EvalCode() { HasIndex = true, IndexPosition = indexPos, MemberName = property.Name }));
                    else
                        Settings.Writer.WriteLine(OutputBuilder.GetOutputObjectProperty(
                            indent,
                            property.AccessModifierForGet(),
                            property.AccessModifierForSet(),
                            property.PropertyType.ToString(),
                            property.Name,
                            propertyValue ?? "<null>",
                            new Models.EvalCode() { MemberName = property.Name }));
                }
            }
            else
            {
                // This logic is for handling properties with primitive types

                if (hasIndex)
                    Settings.Writer.WriteLine(OutputBuilder.GetOutputPrimitivePropertyWithIndex(
                        indent,
                        property.AccessModifierForGet(),
                        property.AccessModifierForSet(),
                        property.PropertyType.ToString(),
                        property.Name,
                        indexPos,
                        propertyValue ?? "<null>",
                        new Models.EvalCode() { HasIndex = true, IndexPosition = indexPos, MemberName = property.Name }));
                else
                    Settings.Writer.WriteLine(OutputBuilder.GetOutputPrimitiveProperty(
                        indent,
                        property.AccessModifierForGet(),
                        property.AccessModifierForSet(),
                        property.PropertyType.ToString(),
                        property.Name,
                        propertyValue ?? "<null>",
                        new Models.EvalCode() { MemberName = property.Name }));
            }
        }

        public string ProcessOutput(TextWriter output, TextWriter error, TextWriter returnValueOutput, Exception ex = null, int indent = 0)
        {
            return OutputBuilder.BuildOutput(output, error, returnValueOutput, ex, indent, Settings.Eval);
        }
    }
}
