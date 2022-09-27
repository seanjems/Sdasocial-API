using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using sdakccapi.Dtos.SignalRDto;
using sdakccapi.Hubs;
using sdakccapi.Infrastructure;
using sdakccapi.Models.Entities;
using System.Configuration;
using System.Text;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var currentPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
// Add services to the container.
//private const string _defaultCorsPolicyName = "localhost";

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<Dictionary<string, UserConnectionDto>>(opts => new Dictionary<string, UserConnectionDto>());
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();


var _logger = new LoggerConfiguration()
    .WriteTo.File($"{currentPath.Replace("sdakccapi.dll","")}\\Logs\\Logs.txt", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information()
    .CreateLogger();
builder.Logging.AddSerilog(_logger);
builder.Services.AddSwaggerGen(setup =>
{
    // Include 'SecurityScheme' to use JWT Authentication
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

});
builder.Services.AddDbContext<sdakccapiDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:DevConnection"]);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy(
       "localhost",
           builders =>
           {
               builders
                               .WithOrigins(builder.Configuration.GetSection("AllowedOrigins").Get<string[]>())
                               .AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
           });
});

builder.Services.AddIdentity<AppUser, IdentityRole>
            (options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = true;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<sdakccapiDbContext>()
            .AddDefaultTokenProviders();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/chat")))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//    .AddJwtBearer(options =>
//    {

//        options.Events = new JwtBearerEvents
//        {
//            OnMessageReceived = context =>
//            {
//                var accessToken = context.Request.Query["access_token"];

//                // If the request is for our hub...
//                var path = context.HttpContext.Request.Path;
//                if (!string.IsNullOrEmpty(accessToken) &&
//                    (path.StartsWithSegments("/chat")))
//                {
//                    // Read the token out of the query string
//                    context.Token = accessToken;
//                }
//                return Task.CompletedTask;
//            }
//        };
//        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
//        {
//            ValidateIssuer = true,
//            ValidateAudience = true,
//            ValidateLifetime = true,
//            ValidateIssuerSigningKey = true,
//            ValidIssuer = builder.Configuration["Jwt:Issuer"],
//            ValidAudience = builder.Configuration["Jwt:Audience"],
//            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
//        };
//    });
builder.Services.AddMvc().AddControllersAsServices();
builder.Services.AddControllers().AddControllersAsServices();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
app.UseStaticFiles();
app.UseCors("localhost");
app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthentication();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chat");
});

//app.MapControllers();

app.Run();
