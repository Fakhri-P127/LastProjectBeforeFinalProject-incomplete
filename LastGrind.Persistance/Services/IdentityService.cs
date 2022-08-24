using LastGrind.Application.Interfaces;
using LastGrind.Domain.Entities;
using LastGrind.Persistance.Configurations.Authentication;
using LastGrind.Persistance.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LastGrind.Persistance.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly AppDbContext _context;
      

        public IdentityService(UserManager<IdentityUser> userManager, JwtSettings jwtSettings,
            TokenValidationParameters tokenValidationParameters,AppDbContext context
           )
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _tokenValidationParameters = tokenValidationParameters;
            _context = context;
           
        }

        public async Task<AuthenticationResult> RegisterAsync(string email, string password)
        {
            
            IdentityUser user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessages = new[] { "User with this email already exists" },
                };
            }

            user = new()
            {
                Email = email,
                UserName = email[..8]
            };

            IdentityResult createdUser = await _userManager.CreateAsync(user, password);
            if (!createdUser.Succeeded)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessages = createdUser.Errors.Select(x => x.Description)
                };
            }
            var result = await _userManager.AddToRoleAsync(user, "Admin");
            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessages = result.Errors.Select(x => x.Description)
                };
            }
            //await _userManager.AddClaimAsync(user, new Claim("tags.view", "true"));
             return await GenerateAuthenticationResultForUserAsync(user);
        }

        public async Task<AuthenticationResult> LoginAsync(string email,string password)
        {
            IdentityUser user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessages = new[] { "This user does not exist" },
                };
            }
            bool hasValidPassword = await _userManager.CheckPasswordAsync(user, password);
            if(!hasValidPassword) return new AuthenticationResult
            {
                Success = false,
                ErrorMessages = new[] { "User or password is incorrect." },
            };

            return await GenerateAuthenticationResultForUserAsync(user);
        }
        public async Task<AuthenticationResult> RefreshTokenAsync(string token, string refreshToken)
        {

            var claimPrincipal = GetPrincipalFromToken(token);
            if (claimPrincipal == null) return new AuthenticationResult
            {
                Success = false,
                ErrorMessages = new[] { "Invalid token error" }
            };

            var expiryDateUnix = long.Parse(claimPrincipal.Claims.FirstOrDefault(x => x.Type 
            == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            if (expiryDateUtc > DateTime.UtcNow)
            {
                return new AuthenticationResult { ErrorMessages = new[] { "This token hasn't expired yet" } };
            }

            string jti = claimPrincipal.Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token.ToString() == refreshToken);
            
            if(storedRefreshToken == null)  return new AuthenticationResult 
            { 
                ErrorMessages = new[] { "This refresh token does not exist" }
            };
           
            if (DateTime.UtcNow > storedRefreshToken.ExpiryDate) return new AuthenticationResult
            {
                ErrorMessages = new[] { "This refresh token has expired" }
            };
            if(storedRefreshToken.Invalidated) return new AuthenticationResult
            {
                ErrorMessages = new[] { "This refresh token has been invalidated" }
            };
            if(storedRefreshToken.Used) return new AuthenticationResult
            {
                ErrorMessages = new[] { "This refresh token has been used" }
            };
            if(storedRefreshToken.JwtId != jti) return new AuthenticationResult
            {
                ErrorMessages = new[] { "This refresh token does not match this Jwt" }
            };

            storedRefreshToken.Used = true;
            _context.RefreshTokens.Update(storedRefreshToken);
            await _context.SaveChangesAsync();

            IdentityUser user = await _userManager.FindByIdAsync(claimPrincipal.Claims
                .FirstOrDefault(x=>x.Type== "id").Value);
            return await GenerateAuthenticationResultForUserAsync(user);
        }
        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                _tokenValidationParameters.ValidateLifetime = false;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, _tokenValidationParameters
                    , out SecurityToken validatedToken);
                //_tokenValidationParameters.ValidateLifetime = true;
                if (!IsJwtWithValidSecurityAlghoritm(validatedToken)) return null;

                return principal;
            }
            catch
            {
                //SecurityTokenExpiredException
                return null;
            }
        }
        private bool IsJwtWithValidSecurityAlghoritm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256
                , StringComparison.InvariantCultureIgnoreCase);
        }
        public async Task<AuthenticationResult> GenerateAuthenticationResultForUserAsync(IdentityUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    //////////////////
                    new Claim(JwtRegisteredClaimNames.Exp, _jwtSettings.TokenLifetime.ToString()),
                    new Claim("id", user.Id),
                });

            //var userClaims = await _userManager.GetClaimsAsync(user);
            //claims.AddClaims(userClaims);
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key)
                , SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6),
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthenticationResult
            {
                Success = true,
                Token = tokenHandler.WriteToken(token),
                RefreshToken=refreshToken.Token.ToString()
            };
        }

       
    }
}
