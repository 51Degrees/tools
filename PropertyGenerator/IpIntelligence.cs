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
                properties: GetProperties(),
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
                properties: GetProperties(),
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
                properties: GetProperties(),
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
                properties: GetProperties(),
                outputPath: basePath + "/IPIntelligenceDataBase.java");
        }

        public override void BuildRust(string basePath)
        {
            Console.WriteLine(String.Format(
                "Building ip_intelligence_data.rs in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            Directory.CreateDirectory(basePath);
            var builder = new MetaDataRustClassBuilder();

            builder.Build(
                copyright: GetCopyright(),
                traitName: "IpIntelligenceData",
                moduleDescription:
                    "Generated strongly-typed read accessors for IP Intelligence.\n" +
                    "\n" +
                    "This file is produced by the 51Degrees PropertyGenerator tool from the\n" +
                    "common metadata. It defines the [`IpIntelligenceData`] trait, one accessor\n" +
                    "per documented property, and implements it for the hand-written\n" +
                    "[`IpIntelligenceDataBase`] by delegating to its by-name stores. Plain\n" +
                    "properties resolve to a single typed value; the weighted properties\n" +
                    "(the country-code distributions and `Mcc`) resolve to an ordered list of\n" +
                    "weighted candidates.",
                traitDescription:
                    "Strongly-typed read accessors for IP Intelligence properties.\n" +
                    "\n" +
                    "Covers the network and location properties of an IP. Each accessor\n" +
                    "returns an [`AspectPropertyValue`] wrapping the property's value type, so\n" +
                    "an absent value carries the engine's no-value reason.",
                uses:
                [
                    "fiftyone_pipeline_core::{PropertyValueType, WeightedValue}",
                    "fiftyone_pipeline_engines::{AspectData, AspectPropertyValue}",
                    "crate::data::IpIntelligenceDataBase",
                ],
                properties: GetProperties(),
                outputPath: basePath + "/ip_intelligence_data.rs");
        }

        private static readonly ProductEnum[] IpiProducts = [
            ProductEnum.IPIV4Enterprise,
            ProductEnum.IPIV4Lite,
        ];

        private static readonly IPropertyMetaData[] IpEchoProperties = [
            new PropertyMetaData
            {
                Name = "Ip",
                Description = "The IPv4 address of the request.",
                ValueType = ValueTypeEnum.IP,
            },
            new PropertyMetaData
            {
                Name = "IpV6",
                Description = "The IPv6 address of the request.",
                ValueType = ValueTypeEnum.IP,
            },
        ];

        private IPropertyMetaData[] GetProperties()
        {
            return _metaData.AllProducts
                .Where(p => IpiProducts.Contains(p.Product))
                .SelectMany(p => p.Properties)
                .DistinctBy(i => i.Name)
                .Concat(IpEchoProperties)
                .ToArray();
        }
    }
}
