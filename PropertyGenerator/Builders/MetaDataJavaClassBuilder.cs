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
                ValueTypeEnum.WeightedString => property.IsList ? "List<IWeightedValue<List<String>>>" : "List<IWeightedValue<String>>",
                ValueTypeEnum.WeightedInt => property.IsList ? "List<IWeightedValue<List<Integer>>>" : "List<IWeightedValue<Integer>>",
                ValueTypeEnum.WeightedDouble => property.IsList ? "List<IWeightedValue<List<Double>>>" : "List<IWeightedValue<Double>>",
                ValueTypeEnum.WeightedSingle => property.IsList ? "List<IWeightedValue<List<Float>>>" : "List<IWeightedValue<Float>>",
                ValueTypeEnum.WeightedBool => property.IsList ? "List<IWeightedValue<List<Boolean>>>" : "List<IWeightedValue<Boolean>>",
                ValueTypeEnum.WeightedByte => property.IsList ? "List<IWeightedValue<List<Byte[]>>>" : "List<IWeightedValue<Byte[]>>",
                ValueTypeEnum.WeightedIp => property.IsList ? "List<IWeightedValue<List<InetAddress>>>" : "List<IWeightedValue<InetAddress>>",
                ValueTypeEnum.WeightedWKBR => property.IsList ? "List<IWeightedValue<List<WktString>>>" : "List<IWeightedValue<WktString>>",
                _ => "String",
            };

            return $"AspectPropertyValue<{type}>";
        }
    }
}
