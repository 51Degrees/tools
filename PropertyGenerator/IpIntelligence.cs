using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.MetaData.Entities;
using FiftyOne.MetaData.Services;
using PropertyGenerationTool;
using PropertyGenerator.Builders;
using System.Collections.Immutable;

namespace PropertyGenerator
{
    /// <summary>
    /// Generator for IP Intelligence engines.
    /// </summary>
    internal class IpIntelligence : GeneratorBase
    {
        private readonly string _copyright;
        private readonly MetaData _metaData;
        private readonly IReadOnlyCollection<IPropertyMetaData> _properties;

        public IpIntelligence(MetaData metaData)
        {
            _metaData = metaData;
            _copyright = ReadCopyright();
        }

        public override void BuildCSharp(string basePath)
        {
            var copyright = ReadCopyright();
            Console.WriteLine(String.Format(
                "Building IIPIntelligenceData in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            Directory.CreateDirectory(basePath);
            var interfaceDescription =
                "\t/// Represents a data object containing values relating to an IP.\n" +
                "\t/// This includes the network, and location.";
            var classDescription =
                "\t/// Abstract base class for properties relating to an IP.\n" +
                "\t/// This includes the network, and location.";

            var builder = new MetaDataCSClassBuilder();

            builder.BuildInterface(
                name: "IIpIntelligenceData",
                copyright: copyright,
                description: interfaceDescription,
                nameSpace: "FiftyOne.IpIntelligence",
                includes: [
                    "System.Net",
                ],
                properties: _metaData.EngineProducts
                    .Single(i => i.Name == "IpIntelligence")
                    .Products.SelectMany(i => i.Properties)
                    .DistinctBy(i => i.Name)
                    .ToArray(),
                outputPath: basePath + "/IIpIntelligenceData.cs");

            Console.WriteLine(String.Format(
                "Building IpIntelligenceDataBase.cs in '{0}'.",
                new DirectoryInfo(basePath).FullName));

            builder.BuildClass(
                name: "IpIntelligenceData",
                interfaceName: "IIpIntelligenceData",
                copyright: _copyright,
                description: classDescription,
                nameSpace: "FiftyOne.IpIntelligence.Shared",
                includes: [
                    "System.Net",
                ],
                properties: _metaData.EngineProducts
                    .Single(i => i.Name == "IpIntelligence")
                    .Products.SelectMany(i => i.Properties)
                    .DistinctBy(i => i.Name)
                    .ToArray(),
                outputPath: basePath + "/IpIntelligenceDataBase.cs");
        }

        public override void BuildJava(string basePath)
        {
            IEnumerable<string> imports = [
                "java.net.InetAddress",
                "fiftyone.pipeline.core.data.IWeightedValue",
                "fiftyone.pipeline.core.data.WktString",
            ];
            Console.WriteLine(String.Format(
                "Building IPIntelligenceData.java for in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            Directory.CreateDirectory(basePath);
            var builder = new MetaDataJavaClassBuilder();

            builder.BuildInterface(
                name: "IPIntelligenceData",
                copyright: _copyright,
                description: 
                " * Interface exposing typed accessors for properties related to an IP.\n" +
                " * This includes the network, and location.",
                package: "fiftyone.ipintelligence.shared",
                imports: imports,
                properties: _metaData.EngineProducts
                    .Single(i => i.Name == "IpIntelligence")
                    .Products.SelectMany(i => i.Properties)
                    .DistinctBy(i => i.Name)
                    .ToArray(),
                outputPath: basePath + "/IPIntelligenceData.java");

            Console.WriteLine(String.Format(
                "Building IPIntelligenceDataBase.java for in '{0}'.",
                new DirectoryInfo(basePath).FullName));

            builder.BuildClass(
                name: "IPIntelligenceDataBase",
                interfaceName: "IPIntelligenceData",
                copyright: _copyright,
                package: "fiftyone.ipintelligence.shared",
                imports: imports,
                properties: _metaData.EngineProducts
                    .Single(i => i.Name == "IpIntelligence")
                    .Products.SelectMany(i => i.Properties)
                    .DistinctBy(i => i.Name)
                    .ToArray(),
                outputPath: basePath + "/IPIntelligenceDataBase.java");
        }
    }
}
