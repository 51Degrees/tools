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
