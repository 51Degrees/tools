using FiftyOne.MetaData.Entities;

namespace PropertyGenerator.Builders
{
    internal class MetaDataRustClassBuilder : RustClassBuilderBase<IPropertyMetaData>
    {
        protected override string DataBaseType => "IpIntelligenceDataBase";

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
                // Keep the branches in sync with [CSClassBuilder] and
                // [JavaClassBuilder] !!
                ValueTypeEnum.Int => property.IsList ? "Vec<i32>" : "i32",
                ValueTypeEnum.Bool => property.IsList ? "Vec<bool>" : "bool",
                ValueTypeEnum.Double => property.IsList ? "Vec<f64>" : "f64",
                ValueTypeEnum.String => property.IsList ? "Vec<String>" : "String",
                ValueTypeEnum.Single => property.IsList ? "Vec<f32>" : "f32",
                // IP addresses are surfaced as their textual form in Rust.
                ValueTypeEnum.IP => property.IsList ? "Vec<String>" : "String",
                ValueTypeEnum.JavaScript => property.IsList ? "Vec<String>" : "String",
                ValueTypeEnum.WKBR => property.IsList ? "Vec<String>" : "String",
                ValueTypeEnum.WeightedString => "Vec<WeightedValue<String>>",
                ValueTypeEnum.WeightedInt => "Vec<WeightedValue<i32>>",
                ValueTypeEnum.WeightedDouble => "Vec<WeightedValue<f64>>",
                ValueTypeEnum.WeightedSingle => "Vec<WeightedValue<f32>>",
                ValueTypeEnum.WeightedBool => "Vec<WeightedValue<bool>>",
                ValueTypeEnum.WeightedByte => "Vec<WeightedValue<Vec<u8>>>",
                ValueTypeEnum.WeightedIp => "Vec<WeightedValue<String>>",
                ValueTypeEnum.WeightedWKBR => "Vec<WeightedValue<String>>",
                _ => "String",
            };

            return $"AspectPropertyValue<{type}>";
        }

        protected override string GetStoreGetter(IPropertyMetaData property)
        {
            if (IsWeighted(property.ValueType))
            {
                // The Rust data base stores every weighted property as a weighted
                // string list (its candidate values are strings).
                return "weighted_string";
            }

            return property.ValueType switch
            {
                ValueTypeEnum.Int => "integer",
                ValueTypeEnum.Bool => "boolean",
                ValueTypeEnum.Double => "double",
                ValueTypeEnum.Single => "float",
                _ => "string",
            };
        }

        protected override string GetCoreValueType(IPropertyMetaData property)
        {
            if (IsWeighted(property.ValueType))
            {
                // Weighted lists surface through the dynamic bag as a flattened
                // key-value list of value/weight records.
                return "KeyValueList";
            }

            return property.ValueType switch
            {
                ValueTypeEnum.Int => "Integer",
                ValueTypeEnum.Bool => "Bool",
                // The core metadata enum has no single-precision variant, so a
                // single float is published as a double, as the C# generator does.
                ValueTypeEnum.Double => "Double",
                ValueTypeEnum.Single => "Double",
                ValueTypeEnum.JavaScript => "JavaScript",
                ValueTypeEnum.String => property.IsList ? "StringList" : "String",
                _ => "String",
            };
        }

        private static bool IsWeighted(ValueTypeEnum valueType)
        {
            return valueType >= ValueTypeEnum.WeightedString
                && valueType <= ValueTypeEnum.WeightedWKBR;
        }
    }
}
