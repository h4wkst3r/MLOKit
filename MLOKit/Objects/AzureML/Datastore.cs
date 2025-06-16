using System;

namespace MLOKit.Objects.AzureML
{
    class Datastore
    {

        public string accountName { get; set; }
        public string containerName { get; set; }
        public string endpoint { get; set; }
        public string datastoreCredential { get; set; }
        public string subscriptionId { get; set; }
        public string datstoreName { get; set; }
        public string datStoreType { get; set; }




        public Datastore(string accountName, string containerName, string endpoint, string datastoreCredential, string subscriptionId, string datstoreName, string datStoreType)
        {
            this.accountName = accountName;
            this.containerName = containerName;
            this.endpoint = endpoint;
            this.datastoreCredential = datastoreCredential;
            this.subscriptionId = subscriptionId;
            this.datstoreName = datstoreName;
            this.datStoreType = datStoreType;




        }
    }
}
