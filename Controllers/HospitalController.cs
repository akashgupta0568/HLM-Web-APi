using HLM_Web_APi.DTO;
using HLM_Web_APi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HLM_Web_APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public HospitalController(SqlConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        [HttpGet("list")]
        public IActionResult GetHospitals()
        {
            try
            {
                // ✅ Use 'using' to ensure proper connection handling
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT HospitalID, Name, Address, City, State, ZipCode, PhoneNumber, Email, LicenseNumber, CreatedAt FROM Hospitals";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<object> hospitals = new List<object>();

                            while (reader.Read())
                            {
                                hospitals.Add(new
                                {
                                    HospitalID = reader["HospitalID"],
                                    Name = reader["Name"],
                                    Address = reader["Address"],
                                    City = reader["City"],
                                    State = reader["State"],
                                    ZipCode = reader["ZipCode"],
                                    PhoneNumber = reader["PhoneNumber"],
                                    Email = reader["Email"],
                                    LicenseNumber = reader["LicenseNumber"],
                                    CreatedAt = reader["CreatedAt"]
                                });
                            }

                            return Ok(new { HospitalList =  hospitals });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }


        [HttpGet("{id}")]
        public IActionResult GetHospitalById(int id)
        {
            string query = "SELECT * FROM Hospitals WHERE HospitalID = @HospitalID";

            using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
            {
                try
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HospitalID", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var hospital = new
                                {
                                    HospitalID = reader["HospitalID"],
                                    Name = reader["Name"],
                                    Address = reader["Address"],
                                    City = reader["City"],
                                    State = reader["State"],
                                    ZipCode = reader["ZipCode"],
                                    PhoneNumber = reader["PhoneNumber"],
                                    Email = reader["Email"],
                                    LicenseNumber = reader["LicenseNumber"],
                                    CreatedAt = reader["CreatedAt"]
                                };
                                return Ok(hospital);
                            }
                        }
                    }
                    return NotFound(new { message = "Hospital not found" });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { message = "Error: " + ex.Message });
                }
            } // Connection is automatically closed here due to 'using'
        }

        [HttpPut("update-hospital/{id}")]
        public IActionResult UpdateHospital(int id, [FromBody] Hospital hospital)
        {
            if (hospital == null || id != hospital.HospitalID)
            {
                return BadRequest(new { message = "Invalid hospital data" });
            }

            try
            {
                _connection.Open();
                string query = @"UPDATE Hospitals 
                         SET Name = @Name, Address = @Address, City = @City, 
                             State = @State, ZipCode = @ZipCode, PhoneNumber = @PhoneNumber, 
                             Email = @Email, LicenseNumber = @LicenseNumber 
                         WHERE HospitalID = @HospitalID";

                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@HospitalID", id);
                    cmd.Parameters.AddWithValue("@Name", hospital.Name);
                    cmd.Parameters.AddWithValue("@Address", hospital.Address);
                    cmd.Parameters.AddWithValue("@City", hospital.City);
                    cmd.Parameters.AddWithValue("@State", hospital.State);
                    cmd.Parameters.AddWithValue("@ZipCode", hospital.ZipCode);
                    cmd.Parameters.AddWithValue("@PhoneNumber", hospital.PhoneNumber);
                    cmd.Parameters.AddWithValue("@Email", hospital.Email);
                    cmd.Parameters.AddWithValue("@LicenseNumber", hospital.LicenseNumber);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0
                        ? Ok(new { message = "Hospital updated successfully" })
                        : NotFound(new { message = "Hospital not found" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }
        }

        [HttpPost("add")]
        public IActionResult AddHospital([FromBody] HospitalDto hospital)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    // ✅ Check if Phone Number already exists
                    string phoneCheckQuery = "SELECT COUNT(*) FROM Hospitals WHERE PhoneNumber = @PhoneNumber";
                    using (SqlCommand phoneCheckCmd = new SqlCommand(phoneCheckQuery, conn))
                    {
                        phoneCheckCmd.Parameters.AddWithValue("@PhoneNumber", hospital.PhoneNumber);
                        int phoneExists = (int)phoneCheckCmd.ExecuteScalar();

                        if (phoneExists > 0)
                        {
                            return BadRequest(new { message = "Error: Phone Number already exists!" });
                        }
                    }

                    // ✅ Check if Email already exists
                    string emailCheckQuery = "SELECT COUNT(*) FROM Hospitals WHERE Email = @Email";
                    using (SqlCommand emailCheckCmd = new SqlCommand(emailCheckQuery, conn))
                    {
                        emailCheckCmd.Parameters.AddWithValue("@Email", hospital.Email);
                        int emailExists = (int)emailCheckCmd.ExecuteScalar();

                        if (emailExists > 0)
                        {
                            return BadRequest(new { message = "Error: Email already exists!" });
                        }
                    }

                    // ✅ Insert Hospital if no duplicate found
                    string query = "INSERT INTO Hospitals (Name, Address, City, State, ZipCode, PhoneNumber, Email, LicenseNumber, CreatedBy) " +
                                   "VALUES (@Name, @Address, @City, @State, @ZipCode, @PhoneNumber, @Email, @LicenseNumber, @CreatedBy)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", hospital.Name);
                        cmd.Parameters.AddWithValue("@Address", hospital.Address);
                        cmd.Parameters.AddWithValue("@City", hospital.City);
                        cmd.Parameters.AddWithValue("@State", hospital.State);
                        cmd.Parameters.AddWithValue("@ZipCode", hospital.ZipCode);
                        cmd.Parameters.AddWithValue("@PhoneNumber", hospital.PhoneNumber);
                        cmd.Parameters.AddWithValue("@Email", hospital.Email);
                        cmd.Parameters.AddWithValue("@LicenseNumber", hospital.LicenseNumber);
                        cmd.Parameters.AddWithValue("@CreatedBy", hospital.CreatedBy); // Store the UserID of the creator
                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                            return Ok(new { message = "Hospital added successfully!" });
                        else
                            return BadRequest(new { message = "Failed to add hospital" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        [HttpGet("createdBy/{createdBy}")]
        public IActionResult GetHospitalsByCreatedBy(int createdBy)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM Hospitals WHERE CreatedBy = @CreatedBy";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@CreatedBy", createdBy);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            List<object> hospitals = new List<object>();
                            while (reader.Read())
                            {
                                hospitals.Add(new
                                {
                                    HospitalID = reader["HospitalID"],
                                    Name = reader["Name"],
                                    Address = reader["Address"],
                                    City = reader["City"],
                                    State = reader["State"],
                                    ZipCode = reader["ZipCode"],
                                    PhoneNumber = reader["PhoneNumber"],
                                    Email = reader["Email"],
                                    LicenseNumber = reader["LicenseNumber"],
                                    CreatedAt = reader["CreatedAt"],
                                    CreatedBy = reader["CreatedBy"]
                                });
                            }
                            return Ok(new { HospitalList = hospitals });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        [HttpPost("register-hospital-user")]
        public IActionResult RegisterHospitalUser([FromBody] RegisterUserDto user)
        {
            try
            {
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    // Ensure only admin can create users
                    string checkAdminQuery = "SELECT COUNT(*) FROM Users WHERE UserID = @AdminID";
                    using (SqlCommand checkCmd = new SqlCommand(checkAdminQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@AdminID", user.CreatedByAdminID);
                        int adminCount = (int)checkCmd.ExecuteScalar();
                        if (adminCount == 0)
                        {
                            return Unauthorized(new { message = "Only admin can create hospital users." });
                        }
                    }

                    // Insert hospital user
                    string insertUserQuery = "INSERT INTO HospitalUsers (FullName, Email, PhoneNumber, PasswordHash, PlainPassword, RoleID, HospitalID, CreatedByAdminID, CreatedAt) " +
                                             "OUTPUT INSERTED.UserID VALUES (@FullName, @Email, @PhoneNumber, @PasswordHash, @PlainPassword, @RoleID, @HospitalID, @CreatedByAdminID, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(insertUserQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@FullName", user.FullName);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@PhoneNumber", user.PhoneNumber);
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@PlainPassword", user.Password);
                        cmd.Parameters.AddWithValue("@RoleID", user.RoleID);
                        cmd.Parameters.AddWithValue("@HospitalID", user.HospitalID);
                        cmd.Parameters.AddWithValue("@CreatedByAdminID", user.CreatedByAdminID);

                        object insertedId = cmd.ExecuteScalar();
                        if (insertedId == null) return BadRequest(new { message = "User registration failed" });

                        return Ok(new { message = "Hospital user registered successfully!", userId = insertedId });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }



    }
}

