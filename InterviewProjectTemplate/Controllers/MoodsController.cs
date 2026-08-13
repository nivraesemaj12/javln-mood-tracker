using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InterviewProjectTemplate.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MoodsController : ControllerBase
    {
    private readonly MoodTrackerDbContext _dbContext;
    private readonly IConfiguration _configuration;
    private const string CookieName = "moodTrackerUserId";
    private const string AdminKeyHeader = "X-Admin-Key";

    public MoodsController(MoodTrackerDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

        [HttpPost]
        public async Task<IActionResult> SubmitMood([FromBody] SubmitMoodRequest request)
        {
            // Identify this browser via cookie, creating one if it doesn't exist yet
            if (!Request.Cookies.TryGetValue(CookieName, out var userId))
            {
                userId = Guid.NewGuid().ToString();
                Response.Cookies.Append(CookieName, userId, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    HttpOnly = false
                });
            }

            var today = DateTime.UtcNow.Date;
            var alreadySubmitted = await _dbContext.MoodEntries.AnyAsync(m =>
                m.UserIdentifier == userId && m.CreatedAtUtc.Date == today);

            if (alreadySubmitted)
            {
                return BadRequest(new { message = "You've already logged your mood today." });
            }

            var entry = new MoodEntry
            {
                Id = Guid.NewGuid(),
                UserIdentifier = userId!,
                Rating = request.Rating,
                Comment = request.Comment,
                CreatedAtUtc = DateTime.UtcNow
            };

            _dbContext.MoodEntries.Add(entry);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Mood submitted successfully." });
        }
        [HttpGet("admin")]
       public async Task<IActionResult> GetAllMoods([FromHeader(Name = "X-Admin-Key")] string? adminKey)
        {
            var expectedKey = _configuration["AdminApiKey"];
            if (string.IsNullOrEmpty(adminKey) || adminKey != expectedKey)
            {
                return Unauthorized(new { message = "Missing or invalid admin key." });
            }

            var entries = await _dbContext.MoodEntries
                .OrderByDescending(m => m.CreatedAtUtc)
                .ToListAsync();

            return Ok(entries);
        }
    }
    

    public class SubmitMoodRequest
    {
        public MoodRating Rating { get; set; }
        public string? Comment { get; set; }
    }
}