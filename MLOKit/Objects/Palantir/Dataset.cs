using System;

namespace MLOKit.Objects.Palantir
{
    class Dataset
    {
        public string datasetName { get; set; }
        public string datasetID { get; set; }
        public string datasetRID { get; set; }
        public string path { get; set; }
        public string type { get; set; }
        public string visibility { get; set; }
        public string dateCreated { get; set; }
        public string dateUpdated { get; set; }
        public string parentFolderRid { get; set; }

        public Dataset(string datasetName, string datasetID, string datasetRID, string path, string type, string visibility, string dateCreated, string dateUpdated, string parentFolderRid)
        {
            this.datasetName = datasetName;
            this.datasetID = datasetID;
            this.datasetRID = datasetRID;
            this.path = path;
            this.type = type;
            this.visibility = visibility;
            this.dateCreated = dateCreated;
            this.dateUpdated = dateUpdated;
            this.parentFolderRid = parentFolderRid;
        }
    }
}
