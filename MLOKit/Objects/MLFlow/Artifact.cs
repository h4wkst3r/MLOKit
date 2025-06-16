using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Objects.MLFlow
{
    class Artifact
    {

        public string path { get; set; }
        public string isDirectory { get; set; }

        public Artifact(string path, string isDirectory)
        {
            this.path = path;
            this.isDirectory = isDirectory;

        }


    }
}
