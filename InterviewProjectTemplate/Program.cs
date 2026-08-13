
using Microsoft.EntityFrameworkCore;
using MySql.EntityFrameworkCore.Extensions;

namespace InterviewProjectTemplate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddCors(o => o.AddDefaultPolicy(policy =>
            policy.WithOrigins("http://localhost:4200")
          .AllowAnyMethod()
          .AllowAnyHeader()
          .AllowCredentials()));

            builder.Services.AddControllers();
            var connectionString = builder.Configuration.GetConnectionString("MySQLConnectionString")
            ?? throw new InvalidOperationException("MySQLConnectionString is not configured.");

            // builder.Services.AddDbContext<MoodTrackerDbContext>(options =>
            // options.UseMySQL(connectionString));

            builder.Services.AddDbContext<MoodTrackerDbContext>(options =>
            options.UseMySQL(connectionString, mySqlOptions =>
            mySqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null)));
            
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            // Automatically apply any pending EF Core migrations on startup.
            // This means the database schema is always created/updated just by running
            // the app, without needing a separate manual migration step - required since
            // `docker compose up` must fully set up the app with no extra commands.
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<MoodTrackerDbContext>();
                dbContext.Database.Migrate();
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
