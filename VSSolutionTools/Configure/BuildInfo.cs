namespace VSSolutionTools.Configure
{
    public class BuildInfo
    {
        public BuildInfo() { }
        
        public BuildInfo(string platform, string configuration, bool build) 
        {
            Platform = platform;
            Configuration = configuration;  
            Build = build;
        }

        public string? Platform { get; set; }
        public string? Configuration { get; set; }
        public bool Build { get; set; }
    }
}
