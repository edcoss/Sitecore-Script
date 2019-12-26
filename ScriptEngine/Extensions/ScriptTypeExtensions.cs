using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptSharp.ScriptEngine
{
    public static class ScriptTypeExtensions
    {
        public static T GetPropertyValue<T>(this object source, string property)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            var sourceType = source.GetType();
            var sourceProperties = sourceType.GetProperties();
            var properties = sourceProperties
                .Where(s => s.Name.Equals(property));
            if (properties.Count() == 0)
            {
                sourceProperties = sourceType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic);
                properties = sourceProperties.Where(s => s.Name.Equals(property));
            }

            if (properties.Count() > 0)
            {
                var propertyValue = properties
                    .Select(s => s.GetValue(source, null))
                    .FirstOrDefault();

                return propertyValue != null ? (T)propertyValue : default(T);
            }

            return default(T);
        }

        public static AccessModifier HighestAccessModifier(this PropertyInfo propertyInfo)
        {
            var max = Math.Max((int)propertyInfo.GetMethod.GetAccessModifier(),
                (int)propertyInfo.SetMethod.GetAccessModifier());
            return (AccessModifier)max;
        }

        public static AccessModifier LowestAccessModifier(this PropertyInfo propertyInfo)
        {
            var max = Math.Min((int)propertyInfo.GetMethod.GetAccessModifier(),
                (int)propertyInfo.SetMethod.GetAccessModifier());
            return (AccessModifier)max;
        }

        public static AccessModifier AccessModifierForGet(this PropertyInfo propertyInfo)
        {
            return propertyInfo.GetMethod.GetAccessModifier();
        }

        public static AccessModifier AccessModifierForSet(this PropertyInfo propertyInfo)
        {
            return propertyInfo.SetMethod.GetAccessModifier();
        }

        public static AccessModifier GetAccessModifier(this MethodInfo methodInfo)
        {
            if (methodInfo != null)
            {
                if (methodInfo.IsPrivate)
                    return AccessModifier.Private;
                if (methodInfo.IsFamily)
                    return AccessModifier.Protected;
                if (methodInfo.IsFamilyOrAssembly)
                    return AccessModifier.ProtectedInternal;
                if (methodInfo.IsAssembly)
                    return AccessModifier.Internal;
                if (methodInfo.IsPublic)
                    return AccessModifier.Public;
            }
            return AccessModifier.Undefined;
        }

        public static AccessModifier GetAccessModifier(this FieldInfo fieldInfo)
        {
            if (fieldInfo != null)
            {
                if (fieldInfo.IsPrivate)
                    return AccessModifier.Private;
                if (fieldInfo.IsFamily)
                    return AccessModifier.Protected;
                if (fieldInfo.IsFamilyOrAssembly)
                    return AccessModifier.ProtectedInternal;
                if (fieldInfo.IsAssembly)
                    return AccessModifier.Internal;
                if (fieldInfo.IsPublic)
                    return AccessModifier.Public;
            }
            return AccessModifier.Undefined;
        }
    }

    public enum AccessModifier
    {
        Undefined,
        Private,
        Protected,
        ProtectedInternal,
        Internal,
        Public
    }
}
