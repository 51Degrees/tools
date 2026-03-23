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
                ValueTypeEnum.Int => property.IsList ? "List<Integer>" : "Integer",
                ValueTypeEnum.Bool => property.IsList ? "List<Boolean>" : "Boolean",
                ValueTypeEnum.Double => property.IsList ? "List<Double>" : "Double",
                ValueTypeEnum.String => property.IsList ? "List<String>" : "String",
                ValueTypeEnum.Single => property.IsList ? "List<Float>" : "Float",
                ValueTypeEnum.IP => property.IsList ? "List<InetAddress>" : "InetAddress",
                ValueTypeEnum.JavaScript => property.IsList ? "List<JavaScript>" : "JavaScript",
                ValueTypeEnum.WKBR => property.IsList ? "List<WktString>" : "WktString",
                ValueTypeEnum.WeightedString => "List<IWeightedValue<String>>",
                ValueTypeEnum.WeightedInt => "List<IWeightedValue<Integer>>",
                ValueTypeEnum.WeightedDouble => "List<IWeightedValue<Double>>",
                ValueTypeEnum.WeightedSingle => "List<IWeightedValue<Float>>",
                ValueTypeEnum.WeightedBool => "List<IWeightedValue<Boolean>>",
                ValueTypeEnum.WeightedByte => "List<IWeightedValue<Byte[]>>",
                ValueTypeEnum.WeightedIp => "List<IWeightedValue<InetAddress>>",
                ValueTypeEnum.WeightedWKBR => "List<IWeightedValue<WktString>>",
                _ => "String",
            };

            return $"AspectPropertyValue<{type}>";
        }
    }
}
