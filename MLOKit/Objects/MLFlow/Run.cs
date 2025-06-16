using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Objects.MLFlow
{
    class Run
    {
        public string runName { get; set; }
        public string runID { get; set; }
        public string userID { get; set; }
        public string status { get; set; }
        public string artifactLocation { get; set; }
        public string associatedExperiment { get; set; }

        public Run(string runName, string runID, string userID, string status, string artifactLocation, string associatedExperiment)
        {
            this.runName = runName;
            this.runID = runID;
            this.userID = userID;
            this.status = status;
            this.artifactLocation = artifactLocation;
            this.associatedExperiment = associatedExperiment;

        }


    }
}
