/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2026 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 *
 * If using the Work as, or as part of, a network application, by
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading,
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

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

        protected override string GetPropertyUrl(IPropertyMetaData property)
        {
            return property.Url;
        }

        protected override IReadOnlyList<DocumentedValue> GetPropertyValues(
            IPropertyMetaData property)
        {
            return PropertyDocumentation.FromMetaData(property);
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
