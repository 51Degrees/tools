using FiftyOne.MetaData.Entities;
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
    /// Engine based implementation of CSClassBuilder.
    /// This uses property metdata from an engine to get the properties.
    /// </summary>
    internal class EngineCSClassBuilder : CSClassBuilderBase<IFiftyOneAspectPropertyMetaData>
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
                // Keep the branches in sync with [JavaClassBuilder] !!
                Type intType when intType == typeof(Int32) => "int",
                Type boolType when boolType == typeof(Boolean) => "bool",
                Type doubleType when doubleType == typeof(Double) => "double",
                Type listType when listType == typeof(IReadOnlyList<string>) => "IReadOnlyList<string>",
                Type floatType when floatType == typeof(float) => "float",
                Type ipType when ipType == typeof(IPAddress) => "IPAddress",
                Type javaScriptType when javaScriptType == typeof(JavaScript) => "JavaScript",
                _ => "string"
            };

            return $"IAspectPropertyValue<{coreType}>";
        }
    }

}
