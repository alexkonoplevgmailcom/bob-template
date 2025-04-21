using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.BusinessServices;
using BFB.DataAccess.MSSQL;
using BFB.DataAccess.MSSQL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register MSSQL services with the environment information
builder.Services.AddMSSQL(builder.Configuration, builder.Environment);

// Register business services
builder.Services.AddBusinessServices();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed data for in-memory database
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<BankDbContext>();
        
        // Add seed data
        dbContext.Accounts.Add(new Account
        {
            Id = 1,
            AccountNumber = "ACC-001",
            OwnerName = "John Doe",
            Balance = 5000.00m,
            AccountTypeId = (int)AccountType.Checking,
            CreatedDate = DateTime.Now.AddYears(-2),
            IsActive = true
        });
        
        dbContext.Accounts.Add(new Account
        {
            Id = 2,
            AccountNumber = "ACC-002",
            OwnerName = "Jane Smith",
            Balance = 15000.50m,
            AccountTypeId = (int)AccountType.Savings,
            CreatedDate = DateTime.Now.AddMonths(-6),
            IsActive = true
        });
        
        dbContext.SaveChanges();
    }
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
