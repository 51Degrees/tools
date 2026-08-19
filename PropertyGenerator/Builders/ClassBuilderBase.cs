using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyGenerator.Builders
{
    /// <summary>
    /// Abstract base class for buiding class files.
    /// </summary>
    /// <typeparam name="T">
    /// The type of property which is read. This can be from an engine,
    /// or metadata.
    /// </typeparam>
    abstract class ClassBuilderBase<T>
    {
        /// <summary>
        /// Get the name of the property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected abstract string GetPropertyName(T property);

        /// <summary>
        /// Get the type of the property as a string for the language being
        /// generated.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected abstract string GetPropertyType(T property);

        /// <summary>
        /// Get the description from the property.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected abstract string GetPropertyDescription(T property);

        /// <summary>
        /// Get the page with more information about the property. Returns null
        /// or an empty string when there is not one, so callers test with
        /// <see cref="string.IsNullOrWhiteSpace(string)"/>. The default has no
        /// URL so that a builder whose property type carries none need not
        /// implement it.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual string GetPropertyUrl(T property)
        {
            return string.Empty;
        }

        /// <summary>
        /// Get the values which the property can return, with the meaning of
        /// each one, for the properties whose values are worth showing to a
        /// developer. The default documents no values so that a builder whose
        /// property type carries none need not implement it.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual IReadOnlyList<DocumentedValue> GetPropertyValues(
            T property)
        {
            return Array.Empty<DocumentedValue>();
        }
    }
}
