using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using AspNetCoreRateLimit;
// using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
// using CacheManager.Core;
// using Microsoft.OpenApi.Models;
// using Microsoft.EntityFrameworkCore;
using FluentValidation;


// using Infrastructure.Databases.Contexts;
// using Domain.Entities;
using Service.Extensions;
// using Infrastructure.Configurations;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;

using Minio;

DotNetEnv.Env.Load();

var configuration = new Microsoft.Extensions.Configuration.ConfigurationBuilder()
   .SetBasePath(Directory.GetCurrentDirectory())
   .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
   .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
   .Build();





var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
builder.Services.AddInMemoryRateLimiting();


// builder.Services.AddCacheManagerConfiguration(cfg =>
//     cfg.WithMaxRetries(10).WithRetryTimeout(500).WithUpdateMode(CacheUpdateMode.Up)
//         .WithMicrosoftMemoryCacheHandle().EnableStatistics());


builder.Services.AddApiVersioning(opt =>
{
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.ReportApiVersions = true;
    opt.DefaultApiVersion = new ApiVersion(1, 0);
})
.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});


builder.Services.Configure<RouteOptions>(options =>
{
    options.LowercaseUrls = true;
    options.LowercaseQueryStrings = true;
    options.AppendTrailingSlash = true;
});


builder.Services.AddSingleton<HtmlEncoder>(
    HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin,
                    UnicodeRanges.Arabic }));

builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// var sqlConnectionString = Environment.GetEnvironmentVariable("SQLSERVER");
// var migrationsAssembly = typeof(PostgresContext).GetTypeInfo().Assembly.GetName().Name;

// builder.Services.AddDbContext<PostgresContext>(options =>
//         options.UseSqlServer(sqlConnectionString,
//         sqlServerOptionsAction: sqlOptions =>
//         {
//             sqlOptions.MigrationsAssembly(migrationsAssembly);
//             sqlOptions.EnableRetryOnFailure(maxRetryCount: 150,
//                                             maxRetryDelay: TimeSpan.FromSeconds(10),
//                                             errorNumbersToAdd: null);
//             sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", "dbo");
//         }));
// builder.Services.AddIdentity<User, IdentityRole>()
//         .AddEntityFrameworkStores<PostgresContext>()
//         .AddDefaultTokenProviders();




builder.Services.AddServices();

builder.Services.AddResponseCaching();

// builder.Services.AddSwaggerGen(options =>
// {
//     options.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "Identity API V1" });
//     options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//     {
//         Name = "Authorization",
//         Type = SecuritySchemeType.ApiKey,
//         Scheme = "Bearer",
//         BearerFormat = "JWT",
//         In = ParameterLocation.Header,
//         Description = "JWT Authorization header using the Bearer scheme. Example: Bearer",
//     });
// });



var endpoint = Environment.GetEnvironmentVariable("MINIO_ENDPOINT");
var accessKey = Environment.GetEnvironmentVariable("MINIO_ACCESSKEY");
var secretKey = Environment.GetEnvironmentVariable("MINIO_SECRETKEY");   


builder.Services.AddMinio(config => config
    .WithEndpoint(endpoint)
    .WithCredentials(accessKey, secretKey)
    .WithSSL(secure: false));

var app = builder.Build();

app.UseForwardedHeaders();
app.UseIpRateLimiting();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();


if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}
// else
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }


app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    _ = endpoints.MapControllers();
});

app.Run();
