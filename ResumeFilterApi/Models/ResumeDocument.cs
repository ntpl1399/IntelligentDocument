using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;


public class ResumeDocument
{
    [SimpleField(IsKey = true)]
    public required string Id { get; set; }

    [SearchableField]
    public required string Name { get; set; }

    [SearchableField]
    public required string Email { get; set; }

    [SearchableField]
    public required string Skills { get; set; }

    [SearchableField]
    public required string Education { get; set; }

    [SearchableField]
    public required string Experience { get; set; }

    public required string ResumeUrl { get; set; }
}
