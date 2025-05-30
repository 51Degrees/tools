using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.MetaData.Entities;
using FiftyOne.MetaData.Services;
using PropertyGenerationTool;

namespace PropertyGenerator
{
    /// <summary>
    /// Generator for IP Intelligence engines.
    /// </summary>
    internal class IpIntelligence : GeneratorBase
    {
        private readonly string _copyright;
        private readonly IpiOnPremiseEngine _engine;
        private readonly IReadOnlyCollection<IPropertyMetaData> _properties;

        public IpIntelligence(IpiOnPremiseEngine engine)
        {
            _copyright = ReadCopyright();
            _engine = engine;
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

            var builder = new EngineCSClassBuilder();

            builder.BuildInterface(
                name: "IIpIntelligenceData",
                copyright: copyright,
                description: interfaceDescription,
                nameSpace: "FiftyOne.IpIntelligence",
                includes: [
                    "System.Net",
                ],
                properties: _engine.Properties.ToArray(),
                formatType: (s) => $"IAspectPropertyValue<IReadOnlyList<IWeightedValue<{s}>>>",
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
                properties: _engine.Properties.ToArray(),
                formatType: (s) => $"IAspectPropertyValue<IReadOnlyList<IWeightedValue<{s}>>>",
                outputPath: basePath + "/IpIntelligenceDataBase.cs");
        }

        public override void BuildJava(string basePath)
        {
            IEnumerable<string> imports = [
                "java.net.InetAddress",
                "fiftyone.pipeline.core.data.IWeightedValue",
            ];
            Console.WriteLine(String.Format(
                "Building IPIntelligenceData.java for in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            Directory.CreateDirectory(basePath);
            var builder = new EngineJavaClassBuilder();

            builder.BuildInterface(
                name: "IPIntelligenceData",
                copyright: _copyright,
                description: 
                " * Interface exposing typed accessors for properties related to an IP.\n" +
                " * This includes the network, and location.",
                package: "fiftyone.ipintelligence.shared",
                imports: imports,
                properties: _engine.Properties.ToArray(),
                formatType: (s) => $"AspectPropertyValue<List<IWeightedValue<{s}>>>",
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
                properties: _engine.Properties.ToArray(),
                formatType: (s) => $"AspectPropertyValue<List<IWeightedValue<{s}>>>",
                outputPath: basePath + "/IPIntelligenceDataBase.java");
        }
    }
}
