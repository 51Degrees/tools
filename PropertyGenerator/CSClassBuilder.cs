using FiftyOne.DeviceDetection.Hash.Engine.OnPremise.FlowElements;
using FiftyOne.MetaData.Entities;
using FiftyOne.Pipeline.Core.Data.Types;
using FiftyOne.Pipeline.Engines.FiftyOne.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace PropertyGenerationTool
{
    /// <summary>
    /// Engine based implementation of CSClassBuilder.
    /// This uses property metdata from an engine to get the properties.
    /// </summary>
    internal class EngineCSClassBuilder : CSClassBuilder<IFiftyOneAspectPropertyMetaData>
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
    /// Class builder for C#.
    /// Methods for getting info from a property are extracted so that the
    /// class is not tied to the type of property.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal abstract class CSClassBuilder<T> : ClassBuilderBase<T>
    {
        internal string GetReturnType(
            T property,
            Func<string, string> formatType)
        {
            string typeString = GetPropertyType(property) switch
            {
                // Keep the branches in sync with [JavaClassBuilder] !!
                Type intType when intType == typeof(Int32) => "int",
                Type boolType when boolType == typeof(Boolean) => "bool",
                Type doubleType when doubleType == typeof(Double) => "double",
                Type listType when listType == typeof(IReadOnlyList<string>) => "IReadOnlyList<string>",
                Type floatType when floatType == typeof(float) => "float",
                Type ipType when ipType == typeof(IPAddress) => "IPAddress",
                Type javaScriptType when javaScriptType == typeof(JavaScript) => "JavaScript",
                _ => "string",
            };
            return formatType(typeString);
        }

        internal string GetGetterName(T property)
        {
            return GetPropertyName(property)
                .Replace("/", "")
                .Replace("-", "");
        }

        internal string GetGetter(
            T property,
            Func<string, string> formatType)
        {
            return String.Format(
                "public {0} {1} {{ get {{ return GetAs<{0}>(\"{2}\"); }} }}",
                GetReturnType(property, formatType),
                GetGetterName(property),
                GetPropertyName(property));
        }

        internal string GetDescription(T property)
        {
            return Regex.Replace(GetPropertyDescription(property), @"<.*>", delegate (Match match)
            {
                return $"<![CDATA[{match.Value}]]>";
            });
        }

        internal string GetKeyValuePair(
            T property,
            Func<string, string> formatType)
        {
           return String.Format("{{ \"{0}\", typeof({1}) }}",
               GetPropertyName(property),
               GetReturnType(property, formatType));
        }


        internal void BuildInterface(
            string name,
            string copyright,
            string description,
            string nameSpace,
            string[] includes,
            T[] properties,
            Func<string, string> formatType,
            string outputPath)
        {
            using (var outputStream = new FileStream(outputPath, FileMode.Create))
            using (var writer = new StreamWriter(outputStream))
            {
                writer.WriteLine(copyright);
                foreach (var include in includes)
                {
                    writer.WriteLine($"using {include};");
                }
                writer.WriteLine("using FiftyOne.Pipeline.Core.Data;");
                writer.WriteLine("using FiftyOne.Pipeline.Engines.Data;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine("");

                writer.WriteLine("// This interface sits at the top of the name space in order to make ");
                writer.WriteLine("// life easier for consumers.");
                writer.WriteLine(String.Format("namespace {0}", nameSpace));
                writer.WriteLine("{");
                writer.WriteLine("\t/// <summary>");
                writer.WriteLine(description);
	            writer.WriteLine("\t/// </summary>");
                writer.WriteLine($"\tpublic interface {name} : IAspectData");
                writer.WriteLine("\t{");
                foreach (var property in properties
                    .Where(p => Constants.excludedProperties.Contains(GetPropertyName(p)) == false)
                    .OrderBy(GetPropertyName))
                {
                    writer.WriteLine("\t\t/// <summary>");
                    writer.WriteLine("\t\t/// " + GetDescription(property));
                    writer.WriteLine("\t\t/// </summary>");
                    writer.WriteLine("\t\t{0} {1} {{ get; }}",
                        GetReturnType(property, formatType),
                        GetGetterName(property));
                }

                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }

        internal void BuildClass(
            string name,
            string interfaceName,
            string copyright,
            string description,
            string nameSpace,
            string[] includes,
            T[] properties,
            Func<string, string> formatType,
            string outputPath)
        {
            using (var outputStream = new FileStream(outputPath, FileMode.Create))
            using (var writer = new StreamWriter(outputStream))
            {
                writer.WriteLine(copyright);
                foreach (var include in includes)
                {
                    writer.WriteLine($"using {include};");
                }
                writer.WriteLine("using FiftyOne.Pipeline.Core.Data;");
                writer.WriteLine("using FiftyOne.Pipeline.Core.FlowElements;");
                writer.WriteLine("using FiftyOne.Pipeline.Engines.Data;");
                writer.WriteLine("using FiftyOne.Pipeline.Engines.FlowElements;");
                writer.WriteLine("using FiftyOne.Pipeline.Engines.Services;");
                writer.WriteLine("using Microsoft.Extensions.Logging;");
                writer.WriteLine("using System;");
                writer.WriteLine("using System.Collections.Generic;");
                writer.WriteLine(String.Format("namespace {0}", nameSpace));
                writer.WriteLine("{");
                writer.WriteLine("\t/// <summary>");
                writer.WriteLine(description);
                writer.WriteLine("\t/// </summary>");
                writer.WriteLine($"\tpublic abstract class {name} : AspectDataBase, {interfaceName}");
                writer.WriteLine("\t{");
                writer.WriteLine("\t\t/// <summary>");
                writer.WriteLine("\t\t/// Constructor.");
                writer.WriteLine("\t\t/// </summary>");
                writer.WriteLine("\t\t/// <param name=\"logger\">");
                writer.WriteLine("\t\t/// The logger for this instance to use.");
                writer.WriteLine("\t\t/// </param>");
                writer.WriteLine("\t\t/// <param name=\"pipeline\">");
                writer.WriteLine("\t\t/// The Pipeline this data instance has been created by.");
                writer.WriteLine("\t\t/// </param>");
                writer.WriteLine("\t\t/// <param name=\"engine\">");
                writer.WriteLine("\t\t/// The engine this data instance has been created by.");
                writer.WriteLine("\t\t/// </param>");
                writer.WriteLine("\t\t/// <param name=\"missingPropertyService\">");
                writer.WriteLine("\t\t/// The missing property service to use when a requested property");
		        writer.WriteLine("\t\t/// does not exist.");
                writer.WriteLine("\t\t/// </param>");
                writer.WriteLine($"\t\tprotected {name}(");
                writer.WriteLine("\t\t\tILogger<AspectDataBase> logger,");
                writer.WriteLine("\t\t\tIPipeline pipeline,");
                writer.WriteLine("\t\t\tIAspectEngine engine,");
                writer.WriteLine("\t\t\tIMissingPropertyService missingPropertyService)");
                writer.WriteLine("\t\t\t: base(logger, pipeline, engine, missingPropertyService) { }");
                writer.WriteLine("");

                writer.WriteLine("\t\t/// <summary>");
                writer.WriteLine("\t\t/// Dictionary of property value types, keyed on the string");
                writer.WriteLine("\t\t/// name of the type.");
                writer.WriteLine("\t\t/// </summary>");
                writer.WriteLine("\t\tprotected static readonly IReadOnlyDictionary<string, Type> PropertyTypes =");
                writer.WriteLine("\t\t\tnew Dictionary<string, Type>()");
                writer.WriteLine("\t\t\t{");

                var filteredProperties = properties
                    .Where(p => Constants.excludedProperties.Contains(GetPropertyName(p)) == false)
                    .OrderBy(GetPropertyName)
                    .ToList();

                foreach (var property in filteredProperties)
                {
                    // Checks if the current element is the last
                    // and adds coma at the end if it is not.
                    if (filteredProperties.IndexOf(property) != filteredProperties.Count -1)
                    {
                        writer.WriteLine("\t\t\t\t" + GetKeyValuePair(property, formatType) + ",");
                    }
                    else
                    {
                        writer.WriteLine("\t\t\t\t" + GetKeyValuePair(property, formatType));
                    }
                }
                writer.WriteLine("\t\t\t};");
                writer.WriteLine("");

                foreach (var property in properties)
                {
                    writer.WriteLine("\t\t/// <summary>");
                    writer.WriteLine("\t\t/// " + GetDescription(property));
                    writer.WriteLine("\t\t/// </summary>");
                    writer.WriteLine("\t\t" + GetGetter(property, formatType));
                }
                writer.WriteLine("\t}");
                writer.WriteLine("}");
            }
        }
    }
}
