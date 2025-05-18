using Abstractions.Interfaces;
using BFB.BusinessServices;
using BFB.DataAccess.DB2;
using BFB.DataAccess.MSSQL;
using BFB.DataAccess.Mongo;
using BFB.DataAccess.RestApi;
using BFB.Template.Api.Extensions;
using BFB.Template.Api.Middleware;
 
using Microsoft.EntityFrameworkCore;
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
  }

// Register DB2 services for customer data access
bool useDB2 = true;
try
{
    // Register DB2 data access services
    builder.Services.AddDB2DataAccess();
    Console.WriteLine("DB2 services registered successfully");
}
catch (Exception ex)
{
    useDB2 = false;
    Console.WriteLine($"DB2 initialization failed: {ex.Message}");
    Console.WriteLine("Running without DB2 support. Customer data will not be available.");
}

// Register REST API data access services
try
{
    builder.Services.AddRestApiDataAccess(builder.Configuration, builder.Environment);
    Console.WriteLine("REST API data access services registered successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"REST API data access initialization failed: {ex.Message}");
    Console.WriteLine("Running without Transaction API support. Transaction data will not be available.");
}

// Register business services
builder.Services.AddBusinessServices();

builder.Services.AddControllers(options =>
    {
        // Add global validation filter to handle model validation
        options.Filters.Add<BFB.Template.Api.Middleware.ValidationModelHandlerAttribute>();
    })
    .AddJsonOptions(options =>
    {
        // Use camelCase for JSON property names
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.CustomSchemaIds(type => type.ToString());
    
    // Add XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
    
    // Define error responses
    options.AddServer(new Microsoft.OpenApi.Models.OpenApiServer
    {
        Description = "Banking API Server"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add global exception handler middleware
app.UseMiddleware<BFB.Template.Api.Middleware.GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
