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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PropertyGenerationTool
{
    public class Constants
    {
        public static string[] excludedProperties = { "JavascriptBandwidth" };

        /// <summary>
        /// Most values to write into the doc comment of a single accessor.
        /// Some properties have thousands of values, which would swamp both
        /// the generated source and the tooltip that shows it, so the table
        /// stops here and points at the property's URL for the rest.
        /// </summary>
        public const int MaxDocumentedValues = 20;

        /// <summary>
        /// Longest value description written into the doc comment, in
        /// characters. Longer descriptions are shortened so that a single
        /// verbose value cannot make the table unreadable.
        /// </summary>
        public const int MaxValueDescriptionLength = 120;
    }
}
