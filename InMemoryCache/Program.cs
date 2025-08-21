using InMemoryCache.Data;
using InMemoryCache.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the core In-Memory Cache service.
// This makes the IMemoryCache interface available for dependency injection.
builder.Services.AddMemoryCache();

// Register our custom services and repositories.
// We use Scoped lifetime, meaning a new instance is created for each web request.
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<ProductService>();

// Register MongoDB client using connection string from configuration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var mongoSection = configuration.GetSection("MongoDB");
    var connectionString = mongoSection["ConnectionURI"];
    return new MongoClient(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
