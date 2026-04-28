using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using MITCRMS.Contract.Services;
using MITCRMS.Interface.Repository;
using MITCRMS.Models.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace MITCRMS.Identity
{

    public class IdentityService : IIdentityService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUserRepository _userRepository;

        public IdentityService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration, IPasswordHasher<User> passwordHasher,
            IUserRepository userRepository)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _configuration = configuration ?? throw new ArgumentException(nameof(configuration));
            _passwordHasher = passwordHasher ?? throw new ArgumentException(nameof(passwordHasher));
            _userRepository = userRepository;
        }

        //public bool CheckPasswordAsync(User user, string password)
        //{
        //    var hashPassword = HashPasswordAsync(password);
        //    if (user.PasswordHash == hashPassword)
        //    {
        //        return true;
        //    }
        //    return false;
        //}

        public string GetUniqueKey(int size)
        {
            char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();
            using var crypto = RandomNumberGenerator.Create();
            byte[] data = new byte[size];
            crypto.GetBytes(data);
            /*using (var crypto = new RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }*/
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }
            return result.ToString();
        }

        public Task<User> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public async Task<User> FindUserAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentNullException(nameof(email));
            }
            //var user = await _gateway.GetUserAsync(userName);
            var user = await _userRepository.Get<User>(u => u.Email == email);
            if (user == null)
            {
#pragma warning disable CS8603 
                return null;
#pragma warning restore CS8603 
            }
            //return user;
            return new User
            {

                Email = user.Email,

            };
        }

        public string GenerateSalt()
        {
            using var crypto = RandomNumberGenerator.Create();
            byte[] buffer = new byte[10];
            crypto.GetBytes(buffer);
            return Convert.ToBase64String(buffer);
        }

        public string GenerateToken(User user, IEnumerable<string> roles)
        {
#pragma warning disable CS8604
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("JwtTokenSettings:TokenKey").Value));
#pragma warning restore CS8604 
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512);
            IList<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
            var token = new JwtSecurityToken("", "", claims,
                DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(Convert.ToInt32(_configuration.GetSection("JwtTokenSettings:TokenExpiryPeriod").Value)),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public JwtSecurityToken GetClaims(string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                if (token.StartsWith("B"))
                {
                    token = token.Split(" ")[1];
                }
                var handler = new JwtSecurityTokenHandler();

                var decodedToken = handler.ReadToken(token) as JwtSecurityToken;

#pragma warning disable CS8603 
                return decodedToken;
#pragma warning restore CS8603 
            }
            Console.WriteLine(token);
#pragma warning disable CS8603 
            return null;
#pragma warning restore CS8603
        }

        public string GetClaimValue(string type)
        {
#pragma warning disable CS8602 
            return _httpContextAccessor.HttpContext.User.FindFirst(type).Value;
#pragma warning restore CS8602 
        }

#pragma warning disable CS8625
        public string GetPasswordHash(string password, string salt = null)
#pragma warning restore CS8625 
        {
            if (string.IsNullOrEmpty(salt))
            {
                return _passwordHasher.HashPassword(new User(), password);
            }
            return _passwordHasher.HashPassword(new User(), $"{password}{salt}");
        }

        public async Task<IList<string>> GetRolesAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var roles = await _userRepository.GetUserAndRoles(user.Id);

            return roles.UserRoles.Select(role => role.Role.RoleName).ToList();
        }

        public string GetUserIdentity()
        {
#pragma warning disable CS8603
            return _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.UniqueName)?.Value;
#pragma warning restore CS8603 
        }

        //private string HashPasswordAsync(string password)
        //{
        //    using (var md5Hash = MD5.Create())
        //    {
        //        var sourceBytes = Encoding.UTF8.GetBytes(password);
        //        var hashBytes = md5Hash.ComputeHash(sourceBytes);
        //        var hash = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        //        return hash.ToLower();
        //    }
        //}



        public IEnumerable<Claim> ValidateToken(string jwtToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
#pragma warning disable CS8604
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtTokenSettings:TokenKey").Value);
#pragma warning restore CS8604

            try
            {
                // Set the validation parameters for the token
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key.ToArray()),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    //ValidIssuer = _configuration.GetSection("JwtTokenSettings:TokenIssuer").Value,
                    ClockSkew = TimeSpan.Zero
                };

                // Validate the token and extract the claims
                var claimsPrincipal = tokenHandler.ValidateToken(jwtToken, validationParameters, out var validatedToken);
                var claims = ((JwtSecurityToken)validatedToken).Claims;

                // Return the claims
                return claims;
            }
            catch (Exception)
            {
#pragma warning disable CS8603
                return null;
#pragma warning restore CS8603
            }
        }

        public async Task<User> GetLoggedInUser()
        {
            // Get the current HttpContext
            var httpContext = _httpContextAccessor.HttpContext;

            // Check if a user is authenticated
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            if (httpContext.User.Identity.IsAuthenticated)
            {
                // Retrieve the user's unique identifier (e.g., user ID) from claims
                var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
                var user = await _userRepository.Get<User>(u => u.Email == email);

                return user;



            }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            // If no user is authenticated, return null
            throw new BadHttpRequestException("Unable to get logged in user");
        }
    }
}
