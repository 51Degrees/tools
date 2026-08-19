using FiftyOne.DeviceDetection.Hash.Engine.OnPremise.FlowElements;
using FiftyOne.MetaData.Entities;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using PropertyGenerationTool;
using PropertyGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PropertyGenerator.Builders
{
    /// <summary>
    /// Class builder for Java.
    /// Methods for getting info from a property are extracted so that the
    /// class is not tied to the type of property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    abstract class JavaClassBuilderBase<T> : ClassBuilderBase<T>
    {
        internal string GetGetterName(T property)
        {
            return "get" + GetPropertyName(property)
                .Replace("/", "")
                .Replace("-", "");
        }

        internal string GetAsString(T property)
        {
            return $"((String)this.getWithCheck(\"{GetPropertyName(property).ToLower()}\"))";
        }

        internal string GetGetter(T property)
        {
            var type = GetPropertyType(property);
            var parts = type.Split(new[] { '<', '>', ',' }, StringSplitOptions.RemoveEmptyEntries);
            return $"public {type} {GetGetterName(property)}() {{ return getAs(\"{GetPropertyName(property).ToLower()}\", {string.Join(", ", parts.Select(p => p + ".class"))}); }}";
        }

        /// <summary>
        /// Build the body of the Javadoc comment for a generated accessor:
        /// the description, the table of values the property can return, and
        /// a link to the page with more information. The link is written last
        /// because Javadoc requires block tags to follow the description.
        /// </summary>
        /// <param name="property"></param>
        /// <param name="indent">
        /// Indent to write each line at, matching the member being documented.
        /// </param>
        /// <returns>
        /// The lines of the comment body, ready to write.
        /// </returns>
        internal IEnumerable<string> GetJavaDocBody(T property, string indent)
        {
            yield return indent + " * " + GetPropertyDescription(property);

            var url = GetPropertyUrl(property);
            var valueTable = PropertyDocumentation.BuildValueTable(
                GetPropertyValues(property),
                url);
            if (valueTable.Count > 0)
            {
                yield return indent + " * <p>";
                yield return indent + " * Possible values:";
                yield return indent + " * <pre>";
                foreach (var line in valueTable)
                {
                    yield return indent + " * " +
                        PropertyDocumentation.EscapeMarkup(line);
                }
                yield return indent + " * </pre>";
            }
            if (string.IsNullOrWhiteSpace(url) == false)
            {
                var escapedUrl = PropertyDocumentation.EscapeMarkup(url.Trim());
                yield return indent + " *";
                yield return indent +
                    $" * @see <a href=\"{escapedUrl}\">More information</a>";
            }
        }

        internal void BuildInterface(
            string name,
            string copyright,
            string description,
            string package,
            IEnumerable<string> imports,
            T[] properties,
            string outputPath)
        {
            using (var outputStream = new FileStream(outputPath, FileMode.Create))
            using (var writer = new StreamWriter(outputStream))
            {
                writer.WriteLine(copyright);
                writer.WriteLine($"package {package};");

                foreach (var import in imports)
                {
                    writer.WriteLine($"import {import};");
                }
                writer.WriteLine("import fiftyone.pipeline.engines.data.AspectData;");
                writer.WriteLine("import fiftyone.pipeline.engines.data.AspectPropertyValue;");
                writer.WriteLine("import java.util.List;");

                writer.WriteLine("// This interface sits at the top of the name space in order to make");
                writer.WriteLine("// life easier for consumers.");
                writer.WriteLine("/**");
                writer.WriteLine(description);
                writer.WriteLine(" */");
                writer.WriteLine($"public interface {name} extends AspectData");
                writer.WriteLine("{");
                foreach (var property in properties
                    .Where(p => Constants.excludedProperties.Contains(GetPropertyName(p)) == false)
                    .OrderBy(GetPropertyName))
                {
                    writer.WriteLine("\t/**");
                    foreach (var line in GetJavaDocBody(property, "\t"))
                    {
                        writer.WriteLine(line);
                    }
                    writer.WriteLine("\t */");
                    writer.WriteLine("\t{0} {1}();",
                        GetPropertyType(property),
                        GetGetterName(property));
                }

                writer.WriteLine("}");
            }
        }

        internal void BuildClass(
            string name,
            string interfaceName,
            string copyright,
            string package,
            IEnumerable<string> imports,
            T[] properties,
            string outputPath)
        {
            using (var outputStream = new FileStream(outputPath, FileMode.Create))
            using (var writer = new StreamWriter(outputStream))
            {
                writer.WriteLine(copyright);
                writer.WriteLine($"package {package};");

                foreach (var import in imports)
                {
                    writer.WriteLine($"import {import};");
                }
                writer.WriteLine("import fiftyone.pipeline.core.data.FlowData;");
                writer.WriteLine("import fiftyone.pipeline.engines.data.AspectData;");
                writer.WriteLine("import fiftyone.pipeline.engines.data.AspectDataBase;");
                writer.WriteLine("import fiftyone.pipeline.engines.data.AspectPropertyMetaData;");
                writer.WriteLine("import fiftyone.pipeline.engines.flowelements.AspectEngine;");
                writer.WriteLine("import fiftyone.pipeline.engines.data.AspectPropertyValue;");
                writer.WriteLine("import fiftyone.pipeline.engines.services.MissingPropertyService;");
                writer.WriteLine("import org.slf4j.Logger;");
                writer.WriteLine("import java.util.List;");

                writer.WriteLine($"public abstract class {name} extends AspectDataBase implements {interfaceName}");
                writer.WriteLine("{");

                writer.WriteLine("/**");
                writer.WriteLine(" * Constructor.");
                writer.WriteLine(" * @param logger used for logging");
                writer.WriteLine(" * @param flowData the {@link FlowData} instance this element data will be");
                writer.WriteLine(" *                 associated with");
                writer.WriteLine(" * @param engine the engine which created the instance");
                writer.WriteLine(" * @param missingPropertyService service used to determine the reason for");
                writer.WriteLine(" *                               a property value being missing");
                writer.WriteLine(" */");
                writer.WriteLine($"\tprotected {name}(");
                writer.WriteLine("\t\tLogger logger,");
                writer.WriteLine("\t\tFlowData flowData,");
                writer.WriteLine("\t\tAspectEngine<? extends AspectData, ? extends AspectPropertyMetaData> engine,");
                writer.WriteLine("\t\tMissingPropertyService missingPropertyService) {");
                writer.WriteLine("\t\tsuper(logger, flowData, engine, missingPropertyService);");
                writer.WriteLine("\t}");

                foreach (var property in properties
                    .Where(p => Constants.excludedProperties.Contains(GetPropertyName(p)) == false)
                    .OrderBy(GetPropertyName))
                {
                    writer.WriteLine("\t/**");
                    foreach (var line in GetJavaDocBody(property, "\t"))
                    {
                        writer.WriteLine(line);
                    }
                    writer.WriteLine("\t */");
                    writer.WriteLine("\t@SuppressWarnings(\"unchecked\")");
                    writer.WriteLine("\t@Override");
                    writer.WriteLine("\t" + GetGetter(property));
                }
                writer.WriteLine("}");
            }
        }
    }
}
