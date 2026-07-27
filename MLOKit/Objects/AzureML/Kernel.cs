using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Objects.AzureML
{
    class Kernel
    {


        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }

        public Kernel(string id, string name, string state)
        {
            this.id = id;
            this.name = name;
            this.state = state;

        }

    }
}
