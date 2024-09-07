using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Controller Api Demo", Version = "v1" });
        });


        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<localDb>();
            dbContext.Database.OpenConnection();
            dbContext.Database.EnsureCreated();  // Create the schema in the in-memory database
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
