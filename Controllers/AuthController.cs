using HLM_Web_APi.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HLM_Web_APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public AuthController(SqlConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        // ✅ Register Endpoint
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserDto user)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password); // Hash password

                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    // 🔹 Check if the user already exists (by Email or PhoneNumber)
                    string checkQuery = "SELECT COUNT(*) FROM Users WHERE Email = @Email OR PhoneNumber = @PhoneNumber";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", user.Email);
                        checkCmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);

                        int existingCount = (int)checkCmd.ExecuteScalar();
                        if (existingCount > 0)
                        {
                            return BadRequest(new { message = "User with this email or phone number already exists!" });
                        }
                    }

                    // 🔹 Insert new user
                    string query = "INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash, RoleID, CreatedAt) " +
                                   "VALUES (@FullName, @Email, @PhoneNumber, @PasswordHash, @RoleID, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@RoleID", user.RoleID);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                            return Ok(new { message = "User registered successfully!" });
                        else
                            return BadRequest(new { message = "Registration failed" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginUserDto user)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    // 🔹 Join Users with Roles to fetch RoleName
                    string query = @"SELECT u.UserID, u.FullName, u.Email, u.PhoneNumber, 
                                    u.PasswordHash, u.RoleID, r.RoleName 
                             FROM Users u
                             INNER JOIN Roles r ON u.RoleID = r.RoleID
                             WHERE u.Email = @Email";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string? storedPassword = reader["PasswordHash"] as string;

                                // 🔹 Ensure password hash is valid before verifying
                                if (!string.IsNullOrEmpty(storedPassword) && storedPassword.StartsWith("$2"))
                                {
                                    if (BCrypt.Net.BCrypt.Verify(user.Password, storedPassword))
                                    {
                                        var token = GenerateJwtToken(
                                            reader["UserID"].ToString(),
                                            reader["Email"].ToString(),
                                            reader["RoleID"].ToString()
                                        );

                                        return Ok(new
                                        {
                                            message = "Login successful!",
                                            token,
                                            roleID = reader["RoleID"],
                                            roleName = reader["RoleName"],
                                            userId = reader["UserID"]
                                        });
                                    }
                                }
                                else
                                {
                                    return StatusCode(500, new { message = "Error: Password hash is invalid or incorrectly stored" });
                                }
                            }
                        }
                    }
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }





        // ✅ Generate JWT Token
        private string GenerateJwtToken(string userId, string email, string role)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role)
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
