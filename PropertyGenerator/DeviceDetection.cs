using FiftyOne.DeviceDetection.Hash.Engine.OnPremise.FlowElements;
using PropertyGenerator;
using System.Xml.Linq;

namespace PropertyGenerationTool
{
    /// <summary>
    /// Generator for Device Detection engines.
    /// </summary>
    public class DeviceDetection : GeneratorBase
    {
        private readonly DeviceDetectionHashEngine _engine;
        private readonly string _copyright;

        public DeviceDetection(DeviceDetectionHashEngine engine)
        {
            _engine = engine;
            _copyright = ReadCopyright();
        }

        public override void BuildCSharp(string basePath)
        {
            Console.WriteLine(String.Format(
                "Building IDeviceData in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            Directory.CreateDirectory(basePath);
            var builder = new EngineCSClassBuilder();
            var interfaceDescription =
                "\t/// Represents a data object containing values relating to a device.\n" +
                "\t/// This includes the hardware, operating system and browser as\n" + 
                "\t/// well as crawler details if the request actually came from a\n" + 
                "\t/// bot or other automated system.";
            var classDescription =
                "\t/// Abstract base class for properties relating to a device.\n" +
                "\t/// This includes the hardware, operating system and browser as\n" +
                "\t/// well as crawler details if the request actually came from a \n" +
                "\t/// bot or other automated system.";

            builder.BuildInterface(
                name: "IDeviceData",
                copyright: _copyright,
                description: interfaceDescription,
                nameSpace: "FiftyOne.DeviceDetection",
                includes: [
                    "FiftyOne.Pipeline.Core.Data.Types",
                ],
                properties: _engine.Properties.ToArray(),
                formatType: (s) => $"IAspectPropertyValue<{s}>",
                outputPath: basePath + "/IDeviceData.cs");

            Console.WriteLine(String.Format(
                "Building DeviceDataBase.cs in '{0}'.",
                new DirectoryInfo(basePath).FullName));

            builder.BuildClass(
                name: "DeviceDataBase",
                interfaceName: "IDeviceData",
                copyright: _copyright,
                description: classDescription,
                nameSpace: "FiftyOne.DeviceDetection.Shared",
                includes: [
                    "FiftyOne.Pipeline.Core.Data.Types",
                ],
                properties: _engine.Properties.ToArray(),
                formatType: (s) => $"IAspectPropertyValue<{s}>",
                outputPath: basePath + "/DeviceDataBase.cs");
        }

        public override void BuildJava(string basePath)
        {
            Console.WriteLine(String.Format(
                "Building DeviceData.java for in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            Directory.CreateDirectory(basePath);
            var builder = new EngineJavaClassBuilder();

            builder.BuildInterface(
                name: "DeviceData",
                copyright: _copyright,
                description: 
                " * Interface exposing typed accessors for properties related to a device\n" +
                " * returned by a device detection engine.", 
                package: "fiftyone.devicedetection.shared",
                imports: [
                    "fiftyone.pipeline.core.data.types.JavaScript",
                ],
                properties: _engine.Properties.ToArray(),
                formatType: (s) => $"AspectPropertyValue<{s}>",
                outputPath: basePath + "/DeviceData.java");

            Console.WriteLine(String.Format(
                "Building DeviceDataBase.java for in '{0}'.",
                new DirectoryInfo(basePath).FullName));

            builder.BuildClass(
                name: "DeviceDataBase",
                interfaceName: "DeviceData",
                copyright: _copyright,
                package: "fiftyone.devicedetection.shared",
                imports: [
                    "fiftyone.pipeline.core.data.types.JavaScript",
                ],
                properties: _engine.Properties.ToArray(),
                formatType: (s) => $"AspectPropertyValue<{s}>",
                outputPath: basePath + "/DeviceDataBase.java");
        }
    }
}
