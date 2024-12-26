using System.Text;
using DotnetAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("http://localhost:3000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    options.AddPolicy("ProdCors", (corsBuilder) =>
    {
        corsBuilder.WithOrigins("https://myprodapp.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// For repository flow EF
builder.Services.AddScoped<IUserRepository, UserRepository>();

// For jwt
string? tokenKeyString = builder.Configuration.GetSection("AppSettings:TokenKey").Value;

SymmetricSecurityKey tokenKey = new SymmetricSecurityKey(
        Encoding.UTF8.GetBytes(
            tokenKeyString != null ? tokenKeyString : ""
        )
);

TokenValidationParameters tokenValidationParameters = new TokenValidationParameters()
{
    IssuerSigningKey = tokenKey, // The key used to create the token
    ValidateIssuer = false,
    ValidateIssuerSigningKey = false,
    ValidateAudience = false
};

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = tokenValidationParameters;
    });


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseCors("ProdCors");
    app.UseHttpsRedirection();
}

// app.MapGet("/weatherforecast", () =>
// {
// })
// .WithName("GetWeatherForecast")
// .WithOpenApi();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
