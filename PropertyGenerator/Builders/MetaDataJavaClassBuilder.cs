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
    internal class MetaDataJavaClassBuilder : JavaClassBuilderBase<IPropertyMetaData>
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
            var type = property.ValueType switch
            {
                // Keep the branches in sync with [CSClassBuilder] !!
                ValueTypeEnum.Int => "Integer",
                ValueTypeEnum.Bool => "Boolean",
                ValueTypeEnum.Double => "Double",
                ValueTypeEnum.String => "String",
                ValueTypeEnum.Single => "Float",
                ValueTypeEnum.IP => "InetAddress",
                ValueTypeEnum.JavaScript => "JavaScript",
                ValueTypeEnum.WKBR => "String",
                _ => "String",
            };

            if (property.IsList)
            {
                type = $"List<{type}>";
            }
            if (property.IsWeighted)
            {
                type = $"List<IWeightedValue<{type}>>";
            }

            return $"AspectPropertyValue<{type}>";
        }
    }
}
