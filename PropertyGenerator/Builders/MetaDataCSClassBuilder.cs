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
                ValueTypeEnum.Int => property.IsList ? "IReadOnlyList<int>" : "int",
                ValueTypeEnum.Bool => property.IsList ? "IReadOnlyList<bool>" : "bool",
                ValueTypeEnum.Double => property.IsList ? "IReadOnlyList<double>" : "double",
                ValueTypeEnum.String => property.IsList ? "IReadOnlyList<string>" : "string",
                ValueTypeEnum.Single => property.IsList ? "IReadOnlyList<float>" : "float",
                ValueTypeEnum.IP => property.IsList ? "IReadOnlyList<IPAddress>" : "IPAddress",
                ValueTypeEnum.JavaScript => property.IsList ? "IReadOnlyList<JavaScript>" : "JavaScript",
                ValueTypeEnum.WKBR => property.IsList ? "IReadOnlyList<WktString>" : "WktString",
                ValueTypeEnum.WeightedString => property.IsList ? "IReadOnlyList<IWeightedValue<IReadOnlyList<string>>>" : "IReadOnlyList<IWeightedValue<string>>",
                ValueTypeEnum.WeightedInt => property.IsList ? "IReadOnlyList<IWeightedValue<IReadOnlyList<int>>>" : "IReadOnlyList<IWeightedValue<int>>",
                ValueTypeEnum.WeightedDouble => property.IsList ? "IReadOnlyList<IWeightedValue<IReadOnlyList<double>>>" : "IReadOnlyList<IWeightedValue<double>>",
                ValueTypeEnum.WeightedSingle => property.IsList ? "IReadOnlyList<IWeightedValue<IReadOnlyList<float>>>" : "IReadOnlyList<IWeightedValue<float>>",
                ValueTypeEnum.WeightedBool => property.IsList ? "IReadOnlyList<IWeightedValue<IReadOnlyList<bool>>>" : "IReadOnlyList<IWeightedValue<bool>>",
                ValueTypeEnum.WeightedIp => property.IsList ? "IReadOnlyList<IWeightedValue<IReadOnlyList<IPAddress>>>" : "IReadOnlyList<IWeightedValue<IPAddress>>",
                ValueTypeEnum.WeightedWKBR => property.IsList ? "IReadOnlyList<IWeightedValue<IReadOnlyList<WktString>>>" : "IReadOnlyList<IWeightedValue<WktString>>",

                _ => "string",
            };

            return $"IAspectPropertyValue<{coreType}>";
        }
    }
}
