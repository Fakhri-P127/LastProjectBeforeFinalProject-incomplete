using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.IdentityModel.Tokens;
using System.Text;
using LastGrind.Persistance.Configurations.Authentication;
namespace LastGrind.WebApi.Installers
{
    public static class JwtBearerInstaller
    {
        public static IServiceCollection AddJwtBearerInstaller(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = new JwtSettings();
            //services.Configure<JwtSetting>(configuration.GetSection("JwtSettings"));
            configuration.Bind(nameof(jwtSettings), jwtSettings);
            services.AddSingleton(jwtSettings);
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSettings.Secret)),
                ValidateIssuer = false,
                ValidateAudience = false,
                RequireExpirationTime = false,
                ValidateLifetime = true,
                ClockSkew=TimeSpan.Zero
            };
            services.AddSingleton(tokenValidationParameters);
            
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(opt =>
            {
                opt.SaveToken = true;
                opt.TokenValidationParameters = tokenValidationParameters;

            });

            return services;
        }
    }
}
