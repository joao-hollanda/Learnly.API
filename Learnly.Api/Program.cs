using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Learnly.Application.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

#region Aplicações + Repositórios
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);
#endregion

#region CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins(
                "https://learnly-edu.vercel.app",
                "http://localhost:3000"
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

    });
});
#endregion

#region JWT
var secretKey = builder.Configuration["jwt:secretKey"];
if (string.IsNullOrEmpty(secretKey))
    throw new InvalidOperationException("JWT SecretKey não configurada");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["jwt:issuer"],
        ValidAudience = builder.Configuration["jwt:audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        ),
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (string.IsNullOrEmpty(context.Token))
            {
                var token = context.Request.Cookies["jwt"];
                if (!string.IsNullOrEmpty(token))
                {
                    context.Token = token;
                }
            }
            return System.Threading.Tasks.Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception is SecurityTokenExpiredException ||
                context.Exception is SecurityTokenException)
            {
                var isProduction = !context.Request.Host.Host.Contains("localhost");
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = isProduction || context.Request.IsHttps,
                    SameSite = SameSiteMode.None,
                    Expires = DateTime.UtcNow.AddDays(-1)
                };
                context.Response.Cookies.Append("jwt", "", cookieOptions);
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }
    };
});
#endregion

#region Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API Learnly",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Digite: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

#region Validators
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Learnly.Api.Validators.LoginValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<Learnly.Application.Validators.UsuarioValidator>();
#endregion

#region Controllers
builder.Services.AddControllers();
#endregion

var app = builder.Build();

#region Pipeline
app.UseCors("AllowReact");

app.UseMiddleware<Learnly.Api.Middlewares.ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
#endregion

app.Run();