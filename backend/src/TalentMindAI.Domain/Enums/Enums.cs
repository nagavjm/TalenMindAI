namespace TalentMindAI.Domain.Enums;

public enum UserRole
{
    User = 0,
    Admin = 1
}

public enum ResumeFileType
{
    Pdf = 0,
    Docx = 1,
    Jpg = 2,
    Png = 3
}

public enum ResumeStatus
{
    Uploaded = 0,
    OcrProcessing = 1,
    OcrCompleted = 2,
    Analyzing = 3,
    Analyzed = 4,
    Indexed = 5,
    Failed = 6
}

public enum QuestionDifficulty
{
    Beginner = 0,
    Intermediate = 1,
    Advanced = 2
}

public enum FeedbackRating
{
    NotRated = 0,
    Helpful = 1,
    NotHelpful = 2
}
