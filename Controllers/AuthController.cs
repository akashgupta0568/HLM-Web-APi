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
                    string roleQuery = "SELECT RoleId, RoleName FROM Roles";
                    using (SqlCommand roleCmd = new SqlCommand(roleQuery, conn))
                    {
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
                    string hospitalQuery = "SELECT HospitalID, Name, Address, City, State, ZipCode, PhoneNumber, Email, LicenseNumber, CreatedAt, CreatedBy FROM Hospitals";
                    using (SqlCommand hospitalCmd = new SqlCommand(hospitalQuery, conn))
                    {
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

                    // Return both lists in JSON format
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

                    // 🔹 Insert new user and return ID
                    string query = "INSERT INTO Users (FullName, Email, PhoneNumber, PasswordHash,PlainPassword, RoleID, CreatedAt) " +
                                   "OUTPUT INSERTED.UserID " +  // Retrieve the newly inserted ID
                                   "VALUES (@FullName, @Email, @PhoneNumber, @PasswordHash, @PlainPassword, @RoleID, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@PlainPassword",user.Password);
                        cmd.Parameters.AddWithValue("@RoleID", user.RoleID);

                        object insertedId = cmd.ExecuteScalar();

                        if (insertedId != null)
                            return Ok(new { message = "User registered successfully!", userId = insertedId });
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

                    // 🔹 Check Admin Login (Users Table)
                    string adminQuery = @"
                SELECT u.UserID, u.FullName, u.Email, u.PhoneNumber, 
                       u.PasswordHash, u.RoleID, r.RoleName 
                FROM Users u
                INNER JOIN Roles r ON u.RoleID = r.RoleID
                WHERE u.Email = @Email";

                    using (SqlCommand adminCmd = new SqlCommand(adminQuery, conn))
                    {
                        adminCmd.Parameters.AddWithValue("@Email", user.Email);
                        using (SqlDataReader adminReader = adminCmd.ExecuteReader())
                        {
                            if (adminReader.Read())
                            {
                                string? storedPassword = adminReader["PasswordHash"] as string;
                                if (!string.IsNullOrEmpty(storedPassword) && storedPassword.StartsWith("$2"))
                                {
                                    if (BCrypt.Net.BCrypt.Verify(user.Password, storedPassword))
                                    {
                                        var token = GenerateJwtToken(
                                            adminReader["UserID"].ToString(),
                                            adminReader["Email"].ToString(),
                                            adminReader["RoleID"].ToString()
                                        );

                                        return Ok(new
                                        {
                                            message = "Admin Login successful!",
                                            token,
                                            roleID = adminReader["RoleID"],
                                            roleName = adminReader["RoleName"],
                                            userId = adminReader["UserID"],
                                            userType = "Admin"
                                        });
                                    }
                                }
                                return Unauthorized(new { message = "Invalid credentials" });
                            }
                        }
                    }

                    // 🔹 Check Hospital User Login (HospitalUsers Table)
                    string hospitalUserQuery = @"
                SELECT hu.UserID, hu.FullName, hu.Email, hu.PhoneNumber, 
                       hu.PasswordHash, hu.RoleID,hu.CreatedByAdminID, r.RoleName, h.Name AS HospitalName
                FROM HospitalUsers hu
                INNER JOIN Roles r ON hu.RoleID = r.RoleID
                INNER JOIN Hospitals h ON hu.HospitalID = h.HospitalID
                WHERE hu.Email = @Email";

                    using (SqlCommand hospitalCmd = new SqlCommand(hospitalUserQuery, conn))
                    {
                        hospitalCmd.Parameters.AddWithValue("@Email", user.Email);
                        using (SqlDataReader hospitalReader = hospitalCmd.ExecuteReader())
                        {
                            if (hospitalReader.Read())
                            {
                                string? storedPassword = hospitalReader["PasswordHash"] as string;
                                if (!string.IsNullOrEmpty(storedPassword) && storedPassword.StartsWith("$2"))
                                {
                                    if (BCrypt.Net.BCrypt.Verify(user.Password, storedPassword))
                                    {
                                        var token = GenerateJwtToken(
                                            hospitalReader["UserID"].ToString(),
                                            hospitalReader["Email"].ToString(),
                                            hospitalReader["RoleID"].ToString()
                                        );

                                        return Ok(new
                                        {
                                            message = "Hospital User Login successful!",
                                            token,
                                            roleID = hospitalReader["RoleID"],
                                            roleName = hospitalReader["RoleName"],
                                            HospitalUserId = hospitalReader["UserID"],
                                            hospitalName = hospitalReader["HospitalName"],
                                            CreatedByAdminID = hospitalReader["CreatedByAdminID"],
                                            userType = "HospitalUser"
                                        });
                                    }
                                }
                                return Unauthorized(new { message = "Invalid credentials" });
                            }
                        }
                    }

                    return Unauthorized(new { message = "Invalid credentials" });
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
