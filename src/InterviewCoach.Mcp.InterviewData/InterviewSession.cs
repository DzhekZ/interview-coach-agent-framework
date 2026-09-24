//using MongoDB.Bson;
//using MongoDB.Bson.Serialization.Attributes;

namespace InterviewCoach.Mcp.InterviewData;

public class InterviewSession
{
    //[BsonId]
    //[BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }
    public string? ResumeLink { get; set; }
    public string? ResumeText { get; set; }
    public bool ProceedWithoutResume { get; set; }
    public string? JobDescriptionLink { get; set; }
    public string? JobDescriptionText { get; set; }
    public bool ProceedWithoutJobDescription { get; set; }
    public string? Transcript { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
