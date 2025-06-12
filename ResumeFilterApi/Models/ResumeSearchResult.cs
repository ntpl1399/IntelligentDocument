namespace ResumeFilterApi.Models
{
    public class ResumeSearchResult
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public List<string> Skills { get; set; }
        public double? Experience { get; set; }

        public string Education { get; set; }

        public string resumeUrl { get; set; }
    }
}
