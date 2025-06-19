using API.Services;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace API.Extensions
{
    public static class AuthenticationExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            // Register AuthService
            services.AddScoped<AuthService>();
            
            // Add ASP.NET Identity services
            services.AddIdentity<ApplicationUser, IdentityRole>(options => 
            {
                // Configure identity options if needed
                options.SignIn.RequireConfirmedAccount = true;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager();
            
            // Configure JWT Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)
                    )
                };
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>("ApiKey", options => { });

            // Register ApiKeyValidator service
            services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();

            // Add authorization services
            services.AddAuthorization(options =>
            {
                options.AddPolicy("JobApiKey", policy =>
                {
                    policy.AuthenticationSchemes.Add("ApiKey");
                    policy.RequireClaim("JobAccess", "true");
                });
            });
            
            return services;
        }
    }

    public class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private const string ApiKeyHeaderName = "X-API-Key";
        private readonly IApiKeyValidator _apiKeyValidator;

        public ApiKeyAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IApiKeyValidator apiKeyValidator)
            : base(options, logger, encoder, clock)
        {
            _apiKeyValidator = apiKeyValidator;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Try to get API Key from headers
            if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var providedApiKey = apiKeyHeaderValues.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            if (!_apiKeyValidator.IsValid(providedApiKey))
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "ApiKeyUser"),
                new Claim("JobAccess", "true")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }

    public interface IApiKeyValidator
    {
        bool IsValid(string providedApiKey);
    }

    public class ApiKeyValidator : IApiKeyValidator
    {
        private readonly IConfiguration _configuration;

        public ApiKeyValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool IsValid(string providedApiKey)
        {
            if (string.IsNullOrWhiteSpace(providedApiKey))
            {
                return false;
            }

            string jobKey = _configuration["ApiKeys:JobKey"] ?? "";

            // Compare against the job API key
            return !string.IsNullOrEmpty(jobKey) && providedApiKey.Equals(jobKey);
        }
    }
}