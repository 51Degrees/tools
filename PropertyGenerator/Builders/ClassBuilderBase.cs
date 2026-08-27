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

        /// <summary>
        /// Documentation assembled once per property. Cached because the
        /// interface and the class of a language are generated from the same
        /// properties, and reading the values from an engine walks and
        /// disposes every one of them on each call.
        /// </summary>
        private readonly Dictionary<T, PropertyDocs> _documentation = new();

        /// <summary>
        /// Get the URL and the laid out value table for a property, assembled
        /// the same way for every language so the emitters stay in lockstep.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        internal PropertyDocs GetPropertyDocumentation(T property)
        {
            if (_documentation.TryGetValue(property, out var docs) == false)
            {
                var url = GetPropertyUrl(property);
                docs = new PropertyDocs(
                    url,
                    PropertyDocumentation.BuildValueTable(
                        GetPropertyValues(property),
                        url));
                _documentation.Add(property, docs);
            }
            return docs;
        }
    }
}
