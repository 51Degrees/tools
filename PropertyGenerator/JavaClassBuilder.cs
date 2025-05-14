using FiftyOne.DeviceDetection.Hash.Engine.OnPremise.FlowElements;
using FiftyOne.MetaData.Entities;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using PropertyGenerator;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PropertyGenerationTool
{
    /// <summary>
    /// Engine based implementation of JavaClassBuilder.
    /// This uses property metdata from an engine to get the properties.
    /// </summary>
    internal class EngineJavaClassBuilder : JavaClassBuilder<IFiftyOneAspectPropertyMetaData>
    {
        protected override string GetPropertyDescription(IFiftyOneAspectPropertyMetaData property)
        {
            return property.Description;
        }

        protected override string GetPropertyName(IFiftyOneAspectPropertyMetaData property)
        {
            return property.Name;
        }

        protected override Type GetPropertyType(IFiftyOneAspectPropertyMetaData property)
        {
            return property.Type;
        }
    }

    /// <summary>
    /// Class builder for Java.
    /// Methods for getting info from a property are extracted so that the
    /// class is not tied to the type of property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class JavaClassBuilder<T> : ClassBuilderBase<T>
    {
        internal string GetReturnType(
            T property,
            Func<string, string> formatType)
        {
            string typeString;
            switch (GetPropertyType(property))
            {
                case Type intType when intType == typeof(Int32):
                    typeString = "Integer";
                    break;
                case Type boolType when boolType == typeof(Boolean):
                    typeString = "Boolean";
                    break;
                case Type doubleType when doubleType == typeof(Double):
                    typeString = "Double";
                    break;
                case Type listType when listType == typeof(IReadOnlyList<string>):
                    typeString = "List<String>";
                    break;
                case Type javaScriptType when javaScriptType == typeof(JavaScript):
                    typeString = "JavaScript";
                    break;
                case Type javaScriptType when javaScriptType == typeof(IPAddress):
                    typeString = "InetAddress";
                    break;
                case Type stringType when stringType == typeof(String):
                default:
                    typeString = "String";
                    break;
            }
            return formatType(typeString);
        }

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

        internal string GetGetter(
            T property,
            Func<string, string> formatType)
        {
            var type = GetReturnType(property, formatType);
            var parts = type.Split(new[] { '<', '>', ','}, StringSplitOptions.RemoveEmptyEntries);
            return $"public {type} {GetGetterName(property)}() {{ return getAs(\"{GetPropertyName(property).ToLower()}\", {string.Join(", ", parts.Select(p => p + ".class"))}); }}";
        }

        internal void BuildInterface(
            string name,
            string copyright,
            string package,
            IEnumerable<string> imports,
            T[] properties,
            Func<string, string> formatType,
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
                // TODO
                writer.WriteLine(" * Interface exposing typed accessors for properties related to a device");
                writer.WriteLine(" * returned by a device detection engine.");
                writer.WriteLine(" */");
                writer.WriteLine($"public interface {name} extends AspectData");
                writer.WriteLine("{");
                foreach (var property in properties
                    .Where(p => Constants.excludedProperties.Contains(GetPropertyName(p)) == false)
                    .OrderBy(GetPropertyName))
                {
                    writer.WriteLine("\t/**");
                    writer.WriteLine("\t * " + GetPropertyDescription(property));
                    writer.WriteLine("\t */");
                    writer.WriteLine("\t{0} {1}();",
                        GetReturnType(property, formatType),
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
            Func<string, string> formatType,
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
                    .Where(p => Constants.excludedProperties.Contains(GetPropertyName(p)) == false))
                {
                    writer.WriteLine("\t/**");
                    writer.WriteLine("\t * " + GetPropertyDescription(property));
                    writer.WriteLine("\t */");
                    writer.WriteLine("\t@SuppressWarnings(\"unchecked\")");
                    writer.WriteLine("\t@Override");
                    writer.WriteLine("\t" + GetGetter(property, formatType));
                }
                writer.WriteLine("}");
            }
        }
    }
}
