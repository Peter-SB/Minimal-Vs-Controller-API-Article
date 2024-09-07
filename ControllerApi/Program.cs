using Microsoft.EntityFrameworkCore;
using Shared.Data;


namespace ControllerApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<localDb>(opt => {
            opt.UseSqlite("Data Source=:memory:");
        }, ServiceLifetime.Singleton); // Needed to keep the in memory database from being disposed

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<localDb>();
            dbContext.Database.OpenConnection();   // Manually open the connection
            dbContext.Database.EnsureCreated();    // Create the schema in the in-memory database
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
