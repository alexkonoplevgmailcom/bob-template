using Abstractions.DTO;
using Abstractions.Interfaces;
using BFB.BusinessServices;
using BFB.DataAccess.MSSQL;
using BFB.DataAccess.MSSQL.Entities;
using BFB.DataAccess.Mongo;
using BFB.DataAccess.Mongo.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register MSSQL services with the environment information
builder.Services.AddMSSQL(builder.Configuration, builder.Environment);

// Register MongoDB services with the environment information
bool useMongoDb = true;
try
{
    // Attempt to create a MongoDB client to check connectivity
    var connectionString = builder.Configuration.GetConnectionString("MongoConnection") ?? "mongodb://localhost:27017";
    var databaseName = builder.Configuration.GetSection("MongoDB:DatabaseName").Value ?? "BankDatabase";
    var client = new MongoClient(connectionString);
    
    // Try a more specific operation to see if MongoDB is accessible
    var database = client.GetDatabase(databaseName);
    var collections = database.ListCollectionsAsync().Result.ToList(); // Will throw if can't connect
    
    // If we get here, MongoDB is available
    builder.Services.AddMongoDB(builder.Configuration, builder.Environment);
    Console.WriteLine($"MongoDB connection successful to database '{databaseName}' - MongoDB services registered");
}
catch (Exception ex)
{
    useMongoDb = false;
    Console.WriteLine($"MongoDB connection failed: {ex.Message}");
    Console.WriteLine("Running without MongoDB support. Branch data will not be available.");
    
    // Register a dummy implementation for IBankBranchRepository
    builder.Services.AddSingleton<IBankBranchRepository, DummyBankBranchRepository>();
}

// Register business services
builder.Services.AddBusinessServices();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed data for in-memory database and MongoDB
if (app.Environment.IsDevelopment())
{
    // Seed SQL data
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
            IsActive = true,
            BankId = 1,
            BranchId = 1
        });
        
        dbContext.Accounts.Add(new Account
        {
            Id = 2,
            AccountNumber = "ACC-002",
            OwnerName = "Jane Smith",
            Balance = 15000.50m,
            AccountTypeId = (int)AccountType.Savings,
            CreatedDate = DateTime.Now.AddMonths(-6),
            IsActive = true,
            BankId = 1,
            BranchId = 2
        });
        
        dbContext.SaveChanges();
        
        // Seed MongoDB data only if MongoDB is available
        if (useMongoDb)
        {
            try
            {
                var mongoContext = scope.ServiceProvider.GetRequiredService<BankMongoDbContext>();
                
                // Check if branches collection is empty before seeding
                var branchCount = mongoContext.Branches.CountDocuments(Builders<Branch>.Filter.Empty);
                
                if (branchCount == 0)
                {
                    mongoContext.Branches.InsertMany(new List<Branch>
                    {
                        new Branch
                        {
                            Id = 1,
                            BankId = 1,
                            BranchName = "Downtown Branch",
                            Address = "123 Main Street",
                            City = "New York",
                            State = "NY",
                            ZipCode = "10001",
                            PhoneNumber = "212-555-1234",
                            IsActive = true,
                            CreatedDate = DateTime.Now.AddYears(-3)
                        },
                        new Branch
                        {
                            Id = 2,
                            BankId = 1,
                            BranchName = "Uptown Branch",
                            Address = "456 Park Avenue",
                            City = "New York",
                            State = "NY",
                            ZipCode = "10022",
                            PhoneNumber = "212-555-5678",
                            IsActive = true,
                            CreatedDate = DateTime.Now.AddYears(-2)
                        },
                        new Branch
                        {
                            Id = 3,
                            BankId = 2,
                            BranchName = "West Side Branch",
                            Address = "789 Broadway",
                            City = "New York",
                            State = "NY",
                            ZipCode = "10019",
                            PhoneNumber = "212-555-9012",
                            IsActive = true,
                            CreatedDate = DateTime.Now.AddYears(-1)
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error seeding MongoDB data: {ex.Message}");
            }
        }
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

// Dummy implementation of IBankBranchRepository for when MongoDB is not available
public class DummyBankBranchRepository : IBankBranchRepository
{
    private readonly List<BankBranch> _branches = new List<BankBranch>
    {
        new BankBranch
        {
            Id = 1,
            BankId = 1,
            BranchName = "Downtown Branch (Dummy)",
            Address = "123 Main Street",
            City = "New York",
            State = "NY",
            ZipCode = "10001",
            PhoneNumber = "212-555-1234",
            IsActive = true,
            CreatedDate = DateTime.Now.AddYears(-3)
        },
        new BankBranch
        {
            Id = 2,
            BankId = 1,
            BranchName = "Uptown Branch (Dummy)",
            Address = "456 Park Avenue",
            City = "New York",
            State = "NY",
            ZipCode = "10022",
            PhoneNumber = "212-555-5678",
            IsActive = true,
            CreatedDate = DateTime.Now.AddYears(-2)
        },
        new BankBranch
        {
            Id = 3,
            BankId = 2,
            BranchName = "West Side Branch (Dummy)",
            Address = "789 Broadway",
            City = "New York",
            State = "NY",
            ZipCode = "10019",
            PhoneNumber = "212-555-9012",
            IsActive = true,
            CreatedDate = DateTime.Now.AddYears(-1)
        }
    };

    public Task<IEnumerable<BankBranch>> GetAllBranchesAsync() => Task.FromResult((IEnumerable<BankBranch>)_branches);

    public Task<BankBranch?> GetBranchByIdAsync(int id) => 
        Task.FromResult(_branches.FirstOrDefault(b => b.Id == id));

    public Task<IEnumerable<BankBranch>> GetBranchesByBankIdAsync(int bankId) => 
        Task.FromResult(_branches.Where(b => b.BankId == bankId));

    public Task<BankBranch> CreateBranchAsync(BankBranch branch)
    {
        branch.Id = _branches.Max(b => b.Id) + 1;
        _branches.Add(branch);
        return Task.FromResult(branch);
    }

    public Task<bool> UpdateBranchAsync(int id, BankBranch branch)
    {
        var existingIndex = _branches.FindIndex(b => b.Id == id);
        if (existingIndex == -1) return Task.FromResult(false);
        
        _branches[existingIndex] = branch;
        return Task.FromResult(true);
    }

    public Task<bool> DeleteBranchAsync(int id)
    {
        var existingIndex = _branches.FindIndex(b => b.Id == id);
        if (existingIndex == -1) return Task.FromResult(false);
        
        _branches.RemoveAt(existingIndex);
        return Task.FromResult(true);
    }
}
