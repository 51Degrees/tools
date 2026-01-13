using System;
using System.Collections.Generic;
using System.Text;

namespace CopyrightUpdater
{
    public class Config
    {
        public DirectoryConfig[] directoryConfigs { get; set; }

        public string[] extensionsToIgnore { get; set; }

        public string[] dirsToIgnore { get; set; }
        public string licenseText { get; set; }
        public string hashLicenseText { get; set; }
        public string patternLicenseText { get; set; }
        public string ipgraphLicenseText { get; set; }
    }

    public class DirectoryConfig
    {
        public string directoryKey { get; set; }
        public string[] extensionsToUpdate { get; set; }
    }
}
