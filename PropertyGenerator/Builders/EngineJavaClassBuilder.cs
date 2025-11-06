using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PropertyGenerator.Builders
{
    /// <summary>
    /// Engine based implementation of JavaClassBuilder.
    /// This uses property metdata from an engine to get the properties.
    /// </summary>
    internal class EngineJavaClassBuilder : JavaClassBuilderBase<IFiftyOneAspectPropertyMetaData>
    {
        protected override string GetPropertyDescription(IFiftyOneAspectPropertyMetaData property)
        {
            return property.Description;
        }

        protected override string GetPropertyName(IFiftyOneAspectPropertyMetaData property)
        {
            return property.Name;
        }

        protected override string GetPropertyType(IFiftyOneAspectPropertyMetaData property)
        {

            var coreType = property.Type switch
            {
                Type intType when intType == typeof(Int32) => "Integer",
                Type boolType when boolType == typeof(Boolean) => "Boolean",
                Type doubleType when doubleType == typeof(Double) => "Double",
                Type listType when listType == typeof(IReadOnlyList<string>) => "List<String>",
                Type floatType when floatType == typeof(float) => "Float",
                Type ipType when ipType == typeof(IPAddress) => "InetAddress",
                Type javaScriptType when javaScriptType == typeof(JavaScript) => "JavaScript",
                _ => "String"
            };

            return $"IAspectPropertyValue<{coreType}>";
        }
    }

}
