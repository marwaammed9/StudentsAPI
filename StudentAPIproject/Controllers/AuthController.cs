using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Stu_Bussniss;
using Stu_DataAccess;
using StudentAPIproject.DTOs.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.RateLimiting;

using System.Text;

namespace StudentAPIproject.Controllers
{
    [Route("api/[controller]")]
    [ApiController]


    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;


        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }



        // This endpoint handles user login.
        // It verifies credentials and returns a JWT token if login succeeds.
        [HttpPost("login")]
        [EnableRateLimiting("AuthLimiter")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]



        public IActionResult Login([FromBody] LoginRequest request)
        {


            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            // Step 1: Find the student by email from the in-memory data store.
            // Email acts as the unique login identifier.
            var student = clsStudent.FindByEmail(request.Email);


            // If no student is found with the given email,
            // return 401 Unauthorized without revealing which field was wrong.
            if (student == null)
            {
                _logger.LogWarning(
          "Failed login attempt (email not found). Email={Email}, IP={IP}",
          request.Email, ip);
                return Unauthorized("Invalid credentials");
            }


            // Step 2: Verify the provided password against the stored hash.
            // BCrypt handles hashing and salt internally.
            bool isValidPassword =
                BCrypt.Net.BCrypt.Verify(request.Password, student.Password);


            // If the password does not match the stored hash,
            // return 401 Unauthorized.
            if (!isValidPassword)
            {
                _logger.LogWarning("Failed login attempt (bad password). Email={Email}, IP={IP}",
                          request.Email, ip
                             );

                return Unauthorized("Invalid credentials");

            }


            // Step 3: Create claims that represent the authenticated user's identity.
            // These claims will be embedded inside the JWT.
            var claims = new[]
            {
                // Unique identifier for the student
                new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),


                // Student email address
                new Claim(ClaimTypes.Email, student.Email),


                // Role (Student or Admin) used later for authorization
                new Claim(ClaimTypes.Role, student.Role)
            };


            // Step 4: Create the symmetric security key used to sign the JWT.
            // This key must match the key used in JWT validation middleware.
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));


            // Step 5: Define the signing credentials.
            // This specifies the algorithm used to sign the token.
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);


            // Step 6: Create the JWT token.
            // The token includes issuer, audience, claims, expiration, and signature.
            var token = new JwtSecurityToken(
                issuer: "StudentApi",
                audience: "StudentApiUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: creds
            );


            // Step 7: Return the serialized JWT token to the client.
            // The client will send this token with future requests.
            // Step 7: Serialize the JWT into a string.
            // This is what the client will send in the Authorization header.
            var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

            // Step 8: Create refresh token (secure random).
            // Refresh token is used to request a new access token later.
            var refreshToken = clsStudent.GenerateRefreshToken(request.Email);
            if (refreshToken == null)
                return StatusCode(500, "Could not create refresh token");


            _logger.LogInformation(
        "Successful login. UserId={UserId}, Email={Email}, IP={IP}",
        student.Id,
        student.Email,
        ip
    );

            // Step 10: Return both tokens to the client.
            // AccessToken is used for API calls.
            // RefreshToken is used only for renewing sessions.
            return Ok(new TokenResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        [EnableRateLimiting("AuthLimiter")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(typeof(string), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]



        public IActionResult Refresh([FromBody] RefreshRequest refreshRequest)
        {

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var student = clsStudent.FindByEmail(refreshRequest.Email);


            var rt = clsStudent.GetRefreshToken(refreshRequest.Email);
            if (rt == null || rt.RefreshTokenHash == null)
            {
                _logger.LogWarning(
          "Invalid refresh attempt (email not found). Email={Email}, IP={IP}",
          refreshRequest.Email,
          ip
      );

                return Unauthorized("Invalid refresh request");

            }



            if (rt.RevokedAt != null)
            {
                _logger.LogWarning(
           "Refresh attempt using revoked token. UserId={UserId}, Email={Email}, IP={IP}",
           student.Id,
           student.Email,
           ip
       );
                return Unauthorized("Refresh token is revoked");
            }

            if (rt.ExpiresAt == null || rt.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning(
          "Refresh attempt using expired token. UserId={UserId}, Email={Email}, IP={IP}",
          student.Id,
          student.Email,
          ip
      );
                return Unauthorized("Refresh token expired");
            }



            if (!BCrypt.Net.BCrypt.Verify(refreshRequest.RefreshToken, rt.RefreshTokenHash))
            {
                _logger.LogWarning(
           "Invalid refresh token attempt. UserId={UserId}, Email={Email}, IP={IP}",
           student.Id,
           student.Email,
           ip
       );
                return Unauthorized("Invalid refresh token");

            }



            // Issue NEW access token (same claims & signing settings as login)
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, student.Id.ToString()),
        new Claim(ClaimTypes.Email, student.Email),
        new Claim(ClaimTypes.Role, student.Role)
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
                issuer: "StudentApi",
                audience: "StudentApiUsers",
                claims: claims,
                //expires: DateTime.UtcNow.AddMinutes(30),
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: creds
            );

            var newAccessToken = new JwtSecurityTokenHandler().WriteToken(jwt);


            // Rotation: replace refresh token
            string newRefreshToken = clsStudent.GenerateRefreshToken(student.Email);

            if (newRefreshToken == null)
            {
                return StatusCode(500, "Could not create refresh token");
            }

            // ✅ Optional low-noise success log (safe)
            _logger.LogInformation(
                "Refresh succeeded. UserId={UserId}, Email={Email}, IP={IP}",
                student.Id,
                student.Email,
                ip
            );

            return Ok(new TokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });

        }



        //Logout Endpoint
        [HttpPost("logout")]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public IActionResult Logout([FromBody] LogoutRequest request)
        {

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var student = clsStudent.FindByEmail(request.Email);
            StudentRefreshTokenDTO RefeshToken = clsStudent.GetRefreshToken(request.Email);

            if (student == null)
                return Ok(); // Do not reveal if user exists

            if (RefeshToken == null || RefeshToken.RefreshTokenHash == null || string.IsNullOrEmpty(request.RefreshToken))
                return Ok(); // nothing to revoke


            bool refreshValid = BCrypt.Net.BCrypt.Verify(request.RefreshToken, RefeshToken.RefreshTokenHash);
            if (!refreshValid)
                return Ok();

            if (!student.TokenRevoked())
            {
                return StatusCode(500, "Could not log out");

            }


            _logger.LogInformation(
             "LogOut succeeded. UserId={UserId}, Email={Email}, IP={IP}",
             student.Id,
             student.Email,
             ip
         );
            return Ok("Logged out successfully");

        }


    }

}

