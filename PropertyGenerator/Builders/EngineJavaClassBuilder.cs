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

        protected override string GetPropertyUrl(IFiftyOneAspectPropertyMetaData property)
        {
            return property.Url;
        }

        protected override IReadOnlyList<DocumentedValue> GetPropertyValues(
            IFiftyOneAspectPropertyMetaData property)
        {
            return PropertyDocumentation.FromEngine(property);
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
