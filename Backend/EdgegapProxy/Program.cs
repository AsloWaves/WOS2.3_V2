using EdgegapProxy.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Edgegap service
builder.Services.AddHttpClient<EdgegapService>();
builder.Services.AddSingleton<EdgegapService>();

// Register Database and JWT services
builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<JwtService>();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("JWT Secret not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "wos-game-server",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "wos-game-client",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Add CORS for Unity clients
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowUnity", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Test database connection on startup
var dbService = app.Services.GetRequiredService<DatabaseService>();
var dbConnected = await dbService.TestConnectionAsync();
if (dbConnected)
{
    Console.WriteLine("[Database] ✅ PostgreSQL connection successful");
}
else
{
    Console.WriteLine("[Database] ❌ PostgreSQL connection failed - check configuration");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowUnity");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }));

Console.WriteLine("[Server] 🚀 WOS Edgegap Proxy started");
Console.WriteLine("[Server] 📡 Server discovery: /api/servers");
Console.WriteLine("[Server] 🔐 Authentication: /api/auth/*");
Console.WriteLine("[Server] 🎒 Inventory: /api/inventory/*");
Console.WriteLine("[Server] 📦 Items: /api/items/definitions");

app.Run();
