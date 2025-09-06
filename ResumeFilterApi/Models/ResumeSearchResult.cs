namespace ResumeFilterApi.Models
{
    public class ResumeSearchResult
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string skills { get; set; }
        public string experience { get; set; }

        public string education { get; set; }

        public string resumeUrl { get; set; }
    }
}
