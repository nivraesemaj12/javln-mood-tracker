using Microsoft.EntityFrameworkCore;

namespace InterviewProjectTemplate
{
    public class MoodTrackerDbContext : DbContext
    {
        public MoodTrackerDbContext(DbContextOptions<MoodTrackerDbContext> options)
            : base(options)
        {
        }

        public DbSet<MoodEntry> MoodEntries { get; set; }
    }
}