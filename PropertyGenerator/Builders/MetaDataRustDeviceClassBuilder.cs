using FiftyOne.MetaData.Entities;

namespace PropertyGenerator.Builders
{
    /// <summary>
    /// Rust class builder for Device Detection.
    /// The Device Detection Rust data base reads every value out of the dynamic
    /// property bag, so the type mapping and the by-name getters differ from the
    /// IP Intelligence builder: integers are `i64`, there are no weighted
    /// properties, and the getters are the `*_property` accessors on
    /// `DeviceDataBase`.
    /// </summary>
    internal class MetaDataRustDeviceClassBuilder : RustClassBuilderBase<IPropertyMetaData>
    {
        protected override string DataBaseType => "DeviceDataBase";

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
                // [JavaClassBuilder] !! Device Detection integers are i64.
                ValueTypeEnum.Int => property.IsList ? "Vec<i64>" : "i64",
                ValueTypeEnum.Bool => property.IsList ? "Vec<bool>" : "bool",
                ValueTypeEnum.Double => property.IsList ? "Vec<f64>" : "f64",
                ValueTypeEnum.String => property.IsList ? "Vec<String>" : "String",
                ValueTypeEnum.Single => property.IsList ? "Vec<f64>" : "f64",
                ValueTypeEnum.JavaScript => property.IsList ? "Vec<String>" : "String",
                ValueTypeEnum.WKBR => property.IsList ? "Vec<String>" : "String",
                _ => "String",
            };

            return $"AspectPropertyValue<{type}>";
        }

        protected override string GetStoreGetter(IPropertyMetaData property)
        {
            // Kept consistent with GetPropertyType: only the string-like types
            // render as a list (Vec<String>), and every Device Detection list is
            // a string list. Any other type, and any unhandled type (which falls
            // through to a scalar String like the C# and Java generators do),
            // reads through its scalar getter.
            return property.ValueType switch
            {
                ValueTypeEnum.Int => "integer_property",
                ValueTypeEnum.Bool => "bool_property",
                ValueTypeEnum.Double => "double_property",
                ValueTypeEnum.Single => "double_property",
                ValueTypeEnum.String => property.IsList ? "string_list_property" : "string_property",
                ValueTypeEnum.JavaScript => "string_property",
                ValueTypeEnum.WKBR => property.IsList ? "string_list_property" : "string_property",
                _ => "string_property",
            };
        }

        protected override string GetCoreValueType(IPropertyMetaData property)
        {
            // Mirrors GetPropertyType / GetStoreGetter so the published metadata
            // type matches how the value is stored and read back.
            return property.ValueType switch
            {
                ValueTypeEnum.Int => "Integer",
                ValueTypeEnum.Bool => "Bool",
                ValueTypeEnum.Double => "Double",
                ValueTypeEnum.Single => "Double",
                ValueTypeEnum.JavaScript => "JavaScript",
                ValueTypeEnum.String => property.IsList ? "StringList" : "String",
                ValueTypeEnum.WKBR => property.IsList ? "StringList" : "String",
                _ => "String",
            };
        }
    }
}
