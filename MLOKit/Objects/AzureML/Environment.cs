using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Objects.AzureML
{
    class Environment
    {


        public string name { get; set; }
        public string description { get; set; }
        public string latestVersion { get; set; }
        public string buildContextUri { get; set; }
        public string buildFilePath { get; set; }


        public Environment(string name, string description, string latestVersion, string buildContextUri, string buildFilePath)
        {
            this.name = name;
            this.description = description;
            this.latestVersion = latestVersion;
            this.buildContextUri = buildContextUri;
            this.buildFilePath = buildFilePath;



        }

    }
}
