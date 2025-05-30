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
                "IIpIntelligenceData",
                copyright,
                interfaceDescription,
                "FiftyOne.IpIntelligence",
                new string[] { "System.Net" },
                _engine.Properties.ToArray(),
                (s) => $"IAspectPropertyValue<IReadOnlyList<IWeightedValue<{s}>>>",
                basePath + "/IIpIntelligenceData.cs");
            Console.WriteLine(String.Format(
                "Building IpIntelligenceDataBase.cs in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            builder.BuildClass(
                "IpIntelligenceData",
                "IIpIntelligenceData",
                _copyright,
                classDescription,
                "FiftyOne.IpIntelligence.Shared",
                new string[] { "System.Net" },
                _engine.Properties.ToArray(),
                (s) => $"IAspectPropertyValue<IReadOnlyList<IWeightedValue<{s}>>>",
                basePath + "/IpIntelligenceDataBase.cs");
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
                "IPIntelligenceData",
                _copyright,
                "fiftyone.ipintelligence.shared",
                " * Interface exposing typed accessors for properties related to an IP.\n" +
                " * This includes the network, and location.",
                imports,
                _engine.Properties.ToArray(),
                (s) => $"AspectPropertyValue<List<IWeightedValue<{s}>>>",
                basePath + "/IPIntelligenceData.java");
            Console.WriteLine(String.Format(
                "Building IPIntelligenceDataBase.java for in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            builder.BuildClass(
                "IPIntelligenceDataBase",
                "IPIntelligenceData",
                _copyright,
                "fiftyone.ipintelligence.shared",
                imports,
                _engine.Properties.ToArray(),
                (s) => $"AspectPropertyValue<List<IWeightedValue<{s}>>>",
                basePath + "/IPIntelligenceDataBase.java");
        }
    }
}
