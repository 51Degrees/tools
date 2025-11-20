using FiftyOne.MetaData.Entities;
using FiftyOne.Pipeline.Core.Data;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Engines.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PropertyGenerator.Builders
{
    internal class MetaDataCSClassBuilder : CSClassBuilderBase<IPropertyMetaData>
    {
        protected override string GetPropertyDescription(IPropertyMetaData property)
        {
            return property.Description;
        }

        protected override string GetPropertyName(IPropertyMetaData property)
        {
            return property.Name;
        }

        protected override string GetPropertyType(IPropertyMetaData property)
        {
            var coreType = property.ValueType switch
            {
                // Keep the branches in sync with [JavaClassBuilder] !!
                ValueTypeEnum.Int => "int",
                ValueTypeEnum.Bool => "bool",
                ValueTypeEnum.Double => "double",
                ValueTypeEnum.String => "string",
                ValueTypeEnum.Single => "float",
                ValueTypeEnum.IP => "IPAddress",
                ValueTypeEnum.JavaScript => "JavaScript",
                ValueTypeEnum.WKBR => "string",
                 _ => "string",
            };

            if (property.IsList)
            {
                coreType = $"IReadOnlyList<{coreType}>";
            }
            if (property.IsWeighted)
            {
                coreType = $"IReadOnlyList<IWeightedValue<{coreType}>>";
            }

            return $"IAspectPropertyValue<{coreType}>";
        }
    }
}
