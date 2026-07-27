using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Objects.AzureML
{
    class TrainingJob
    {

        public string jobName { get; set; }
        public string jobStatus { get; set; }
        public string experiment { get; set; }
        public string computeTarget { get; set; }
        public string jobType { get; set; }
        public string jobOutputType { get; set; }
        public string environment { get; set; }
        public string jobVars { get; set; }
        public string datastoreUri { get; set; }


        public TrainingJob(string jobName, string jobStatus, string experiment, string computeTarget, string jobType, string jobOutputType, string environment, string jobVars, string datastoreUri)
        {
            this.jobName = jobName;
            this.jobStatus = jobStatus;
            this.experiment = experiment;
            this.computeTarget = computeTarget;
            this.jobType = jobType;
            this.jobOutputType = jobOutputType;
            this.environment = environment;
            this.jobVars = jobVars;
            this.datastoreUri = datastoreUri;

        }

    }
}
