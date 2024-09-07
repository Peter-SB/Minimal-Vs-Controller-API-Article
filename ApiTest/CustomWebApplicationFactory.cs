using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Shared.Data;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<localDb>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add a new DbContext using the in-memory SQLite database
            services.AddDbContext<localDb>(options =>
            {
                options.UseSqlite("DataSource=:memory:");
            });

            // Ensure database is created for each test
            var sp = services.BuildServiceProvider();
            using (var scope = sp.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<localDb>();
                db.Database.EnsureCreated();
            }
        });
    }
}
