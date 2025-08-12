namespace MLOKit.Objects.Palantir
{
    class Ontology
    {
        public string displayName { get; set; }
        public string apiName { get; set; }
        public string rid { get; set; }
        public string description { get; set; }

        public Ontology(string displayName, string apiName, string rid, string description)
        {
            this.displayName = displayName;
            this.apiName = apiName;
            this.rid = rid;
            this.description = description;
        }
    }
}
