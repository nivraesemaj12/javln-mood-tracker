using System;

namespace InterviewProjectTemplate
{
    public class MoodEntry
    {
        public Guid Id { get; set; }
        public string UserIdentifier { get; set; } = string.Empty;
        public MoodRating Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}