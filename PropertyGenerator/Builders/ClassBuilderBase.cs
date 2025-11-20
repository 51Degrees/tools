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
    }
}
