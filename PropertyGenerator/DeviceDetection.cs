using FiftyOne.DeviceDetection.Hash.Engine.OnPremise.FlowElements;
using PropertyGenerator;

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
                "IDeviceData",
                _copyright,
                interfaceDescription,
                "FiftyOne.DeviceDetection",
                new string[]
                {
                    "FiftyOne.Pipeline.Core.Data.Types"
                },
                _engine.Properties.ToArray(),
                (s) => $"IAspectPropertyValue<{s}>",
                basePath + "/IDeviceData.cs");
            Console.WriteLine(String.Format(
                "Building DeviceDataBase.cs in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            builder.BuildClass(
                "DeviceDataBase",
                "IDeviceData",
                _copyright,
                classDescription,
                "FiftyOne.DeviceDetection.Shared",
                new string[]
                {
                    "FiftyOne.Pipeline.Core.Data.Types"
                },
                _engine.Properties.ToArray(),
                (s) => $"IAspectPropertyValue<{s}>",
                basePath + "/DeviceDataBase.cs");
        }

        public override void BuildJava(string basePath)
        {
            Console.WriteLine(String.Format(
                "Building DeviceData.java for in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            Directory.CreateDirectory(basePath);
            var builder = new EngineJavaClassBuilder();
            builder.BuildInterface(
                "DeviceData",
                _copyright,
                "fiftyone.devicedetection.shared",
                " * Interface exposing typed accessors for properties related to a device\n" +
                " * returned by a device detection engine.",
                [
                    "fiftyone.pipeline.core.data.types.JavaScript",
                ],
                _engine.Properties.ToArray(),
                (s) => $"AspectPropertyValue<{s}>",
                basePath + "/DeviceData.java");
            Console.WriteLine(String.Format(
                "Building DeviceDataBase.java for in '{0}'.",
                new DirectoryInfo(basePath).FullName));
            builder.BuildClass(
                "DeviceDataBase",
                "DeviceData",
                _copyright,
                "fiftyone.devicedetection.shared",
                [
                    "fiftyone.pipeline.core.data.types.JavaScript",
                ],
                _engine.Properties.ToArray(),
                (s) => $"AspectPropertyValue<{s}>",
                basePath + "/DeviceDataBase.java");
        }
    }
}
