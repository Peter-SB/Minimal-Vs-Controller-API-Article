using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared.Data;
using System;

public static class DbContextFactory
{
    public static localDb CreateInMemoryContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open(); // Keeps the connection alive for the in-memory database

        var options = new DbContextOptionsBuilder<localDb>()
            .UseSqlite(connection)
            .Options;

        var context = new localDb(options);
        context.Database.EnsureCreated(); // Ensures the database schema is created
        return context;
    }
}
