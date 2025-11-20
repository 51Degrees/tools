using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using FiftyOne.DeviceDetection.Hash.Engine.OnPremise.FlowElements;
using Microsoft.Extensions.Logging;
using FiftyOne.Pipeline.Engines;
using PropertyGenerator;
using FiftyOne.IpIntelligence.Engine.OnPremise.FlowElements;
using FiftyOne.MetaData.Entities;
using FiftyOne.MetaData.Services;

namespace PropertyGenerationTool
{
    class Program
    {
        private const string IPI = "IpIntelligence";
        private const string DD = "HashV41";

        private static ILoggerFactory LoggerFactory = new LoggerFactory();

        static void Main(string[] args)
        {
            var metaData = args.Length > 2 ?
                new MetaDataService(new FileResourceService(args[2])).GetMetaData() :
                new MetaDataService().GetMetaData();

            if (args[0].Equals(DD, StringComparison.InvariantCultureIgnoreCase))
            {
                var deviceDetection = new DeviceDetection(metaData);
                // C#.
                deviceDetection.BuildCSharp(Path.Combine(args[1], "CSharp"));
                // Java.
                deviceDetection.BuildJava(Path.Combine(args[1], "Java"));
                Console.WriteLine("Done Device Detection.");
            }
            else if (args[0].Equals(IPI, StringComparison.InvariantCultureIgnoreCase))
            {
                var ipIntelligence = new IpIntelligence(metaData);
                // C#
                ipIntelligence.BuildCSharp(Path.Combine(args[1], "CSharp"));
                // Java
                ipIntelligence.BuildJava(Path.Combine(args[1], "Java"));
                Console.WriteLine("Done IP Intelligence");
            }
            else
            {
                throw new ArgumentException($"Data type {args[0]} not supported." +
                    $"Use either {DD} or {IPI}");
            }
        }
    }
}
