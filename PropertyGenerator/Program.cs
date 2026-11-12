/* *********************************************************************
 * This Original Work is copyright of 51 Degrees Mobile Experts Limited.
 * Copyright 2026 51 Degrees Mobile Experts Limited, Davidson House,
 * Forbury Square, Reading, Berkshire, United Kingdom RG1 3EU.
 *
 * This Original Work is licensed under the European Union Public Licence
 * (EUPL) v.1.2 and is subject to its terms as set out below.
 *
 * If a copy of the EUPL was not distributed with this file, You can obtain
 * one at https://opensource.org/licenses/EUPL-1.2.
 *
 * The 'Compatible Licences' set out in the Appendix to the EUPL (as may be
 * amended by the European Commission) shall be deemed incompatible for
 * the purposes of the Work and the provisions of the compatibility
 * clause in Article 5 of the EUPL shall not apply.
 *
 * If using the Work as, or as part of, a network application, by
 * including the attribution notice(s) required under Article 5 of the EUPL
 * in the end user terms of the application under an appropriate heading,
 * such notice(s) shall fulfill the requirements of that article.
 * ********************************************************************* */

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
                // Rust
                deviceDetection.BuildRust(Path.Combine(args[1], "Rust"));
                Console.WriteLine("Done Device Detection.");
            }
            else if (args[0].Equals(IPI, StringComparison.InvariantCultureIgnoreCase))
            {
                var ipIntelligence = new IpIntelligence(metaData);
                // C#
                ipIntelligence.BuildCSharp(Path.Combine(args[1], "CSharp"));
                // Java
                ipIntelligence.BuildJava(Path.Combine(args[1], "Java"));
                // Rust
                ipIntelligence.BuildRust(Path.Combine(args[1], "Rust"));
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
