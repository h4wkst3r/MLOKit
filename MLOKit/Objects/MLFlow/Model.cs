using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Objects.MLFlow
{
    class Model
    {

        public string modelName { get; set; }
        public string modelVersion { get; set; }
        public string status { get; set; }
        public string description { get; set; }
        public string artifactLocation { get; set; }
        public string associatedRun { get; set; }

        public Model(string modelName, string modelVersion, string status, string description, string artifactLocation, string associatedRun)
        {
            this.modelName = modelName;
            this.modelVersion = modelVersion;
            this.status = status;
            this.description = description;
            this.artifactLocation = artifactLocation;
            this.associatedRun = associatedRun;

        }

    }
}
