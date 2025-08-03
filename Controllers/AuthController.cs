using HLM_Web_APi.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using System.Data;
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

        [HttpGet("roles-hospitals")]
        public IActionResult GetRolesAndHospitals()
        {
            List<Role> roles = new List<Role>();
            List<Hospital> hospitals = new List<Hospital>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Fetch Roles
                    using (SqlCommand roleCmd = new SqlCommand("Sp_GetAllRoles", conn))
                    {
                        roleCmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = roleCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roles.Add(new Role
                                {
                                    RoleId = Convert.ToInt32(reader["RoleId"]),
                                    RoleName = reader["RoleName"].ToString()
                                });
                            }
                        }
                    }

                    // Fetch Hospitals
                    using (SqlCommand hospitalCmd = new SqlCommand("Sp_GetAllHospitals", conn))
                    {
                        hospitalCmd.CommandType = CommandType.StoredProcedure;
                        using (SqlDataReader reader = hospitalCmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                hospitals.Add(new Hospital
                                {
                                    HospitalID = Convert.ToInt32(reader["HospitalID"]),
                                    Name = reader["Name"].ToString(),
                                    Address = reader["Address"].ToString(),
                                    City = reader["City"].ToString(),
                                    State = reader["State"].ToString(),
                                    ZipCode = reader["ZipCode"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    LicenseNumber = reader["LicenseNumber"].ToString(),
                                    CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                    CreatedBy = Convert.ToInt32(reader["CreatedBy"])
                                });
                            }
                        }
                    }

                    return Ok(new
                    {
                        roles = roles,
                        hospitals = hospitals
                    });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, "Internal Server Error: " + ex.Message);
                }
            }
        }




        // ✅ Register Endpoint
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterUserDto user)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_RegisterUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@RoleID", user.RoleID);
                        cmd.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                        SqlParameter outputIdParam = new SqlParameter("@UserID", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputIdParam);

                        cmd.ExecuteNonQuery();

                        int userId = (int)outputIdParam.Value;

                        if (userId == -1)
                        {
                            return BadRequest(new { message = "User with this email or phone number already exists!" });
                        }

                        return Ok(new { message = "User registered successfully!", userId = userId });
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

                    using (SqlCommand cmd = new SqlCommand("Sp_LoginUserByEmail", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Email", user.Email);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword = reader["PasswordHash"].ToString();
                                if (!string.IsNullOrEmpty(storedPassword) && storedPassword.StartsWith("$2") &&
                                    BCrypt.Net.BCrypt.Verify(user.Password, storedPassword))
                                {
                                    var token = GenerateJwtToken(reader["UserID"].ToString(), reader["Email"].ToString(), reader["RoleID"].ToString());
                                    return Ok(new
                                    {
                                        message = "Login successful!",
                                        token,
                                        roleID = reader["RoleID"],
                                        roleName = reader["RoleName"],
                                        userId = reader["UserID"],
                                        userType = "Admin"
                                    });
                                }
                                return Unauthorized(new { message = "Invalid email or password" });
                            }
                        }
                    }
                    return Unauthorized(new { message = "Invalid email or password" });
                }
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
