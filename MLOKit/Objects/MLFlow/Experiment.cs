using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Objects.MLFlow
{
    class Experiment
    {

        public string experimentName { get; set; }
        public string experimentID { get; set; }
        public string status { get; set; }
        public string artifactLocation { get; set; }

        public Experiment(string experimentName, string experimentID, string status, string artifactLocation)
        {
            this.experimentName = experimentName;
            this.experimentID = experimentID;
            this.status = status;
            this.artifactLocation = artifactLocation;
 
        }
    }
}
