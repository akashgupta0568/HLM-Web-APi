using HLM_Web_APi.DataAccess;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton(new SqlConnection(connectionString));
builder.Services.AddSingleton<DbHelper>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKey123!");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

//var corsPolicy = "_allowSpecificOrigins";
var corsPolicy = "_allowAll";
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy(corsPolicy, policy =>
//    {
//        policy.WithOrigins("http://localhost:4200") // Allow frontend URL
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddPolicy("_allowAll", policy =>
    {
        policy.AllowAnyOrigin()    // ✅ Allow all domains
              .AllowAnyHeader()    // ✅ Allow all headers
              .AllowAnyMethod();   // ✅ Allow all methods (GET, POST, etc.)
    });
});

// Add Authorization
builder.Services.AddAuthorization();

var app = builder.Build();



// Configure the HTTP request pipeline
//if (app.Environment.IsDevelopment())
//{
    
//}

app.UseSwagger();
app.UseSwaggerUI();

// Enable Global Exception Handling
app.UseExceptionHandler(app =>
{
    app.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("An unexpected error occurred.");
    });
});

app.UseHttpsRedirection();

// Enable Authentication & Authorization
app.UseCors(corsPolicy);
app.UseAuthentication();  // ? Make sure this is before UseAuthorization()
app.UseAuthorization();

app.MapControllers();

app.Run();