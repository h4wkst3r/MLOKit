using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Objects.AzureML
{
    class Compute
    {

        public string computeName { get; set; }
        public string publicIP { get; set; }
        public string sshUsername { get; set; }
        public string sshPort { get; set; }
        public string sshPubKey { get; set; }
        public string state { get; set; }
        public bool containsJupyterEnv { get; set; }
        public string jupyterEndpoint { get; set; }


        public Compute(string computeName, string publicIP, string sshUsername, string sshPort, string sshPubKey, string state, bool containsJupyterEnv, string jupyterEndpoint)
        {
            this.computeName = computeName;
            this.publicIP = publicIP;
            this.sshUsername = sshUsername;
            this.sshPort = sshPort;
            this.sshPubKey = sshPubKey;
            this.state = state;
            this.containsJupyterEnv = containsJupyterEnv;
            this.jupyterEndpoint = jupyterEndpoint;


        }


    }
}
