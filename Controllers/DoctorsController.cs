using HLM_Web_APi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HLM_Web_APi.Controllers
{
    [Route("api/doctors")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {

        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public DoctorsController(SqlConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        [Authorize]
        [HttpGet("list")]
     
        public IActionResult GetDoctors()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT DoctorID, Name, Specialization, PhoneNumber, Email, HospitalID, RegisterDateTime,ConsultationFee FROM Doctors";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<object> doctors = new List<object>();

                        while (reader.Read())
                        {
                            doctors.Add(new
                            {
                                DoctorID = reader["DoctorID"],
                                Name = reader["Name"],
                                Specialization = reader["Specialization"],
                                PhoneNumber = reader["PhoneNumber"],
                                Email = reader["Email"],
                                HospitalID = reader["HospitalID"],
                                RegisterDateTime = reader["RegisterDateTime"],
                                ConsultationFee = reader["ConsultationFee"]
                            });
                        }

                        return Ok(doctors);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ GET doctor by ID
        [HttpGet("{id}")]
        public IActionResult GetDoctorById(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT DoctorID, Name, Specialization, PhoneNumber, Email, HospitalID, RegisterDateTime, ConsultationFee FROM Doctors WHERE DoctorID = @DoctorID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DoctorID", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new
                                {
                                    DoctorID = reader["DoctorID"],
                                    Name = reader["Name"],
                                    Specialization = reader["Specialization"],
                                    PhoneNumber = reader["PhoneNumber"],
                                    Email = reader["Email"],
                                    HospitalID = reader["HospitalID"],
                                    RegisterDateTime = reader["RegisterDateTime"],
                                     ConsultationFee = reader["ConsultationFee"]
                                });
                            }
                        }
                    }
                }

                return NotFound(new { message = "Doctor not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ POST add new doctor
        [HttpPost("add")]
        public IActionResult AddDoctor([FromBody] DoctorDto doctor)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    // Check if PhoneNumber or Email already exists
                    string checkQuery = "SELECT COUNT(*) FROM Doctors WHERE Email = @Email OR PhoneNumber = @PhoneNumber";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", doctor.Email);
                        checkCmd.Parameters.AddWithValue("@PhoneNumber", doctor.PhoneNumber);

                        int existingCount = (int)checkCmd.ExecuteScalar();
                        if (existingCount > 0)
                        {
                            return BadRequest(new { message = "Error: Phone Number or Email already exists!" });
                        }
                    }

                    // Insert new doctor
                    string query = "INSERT INTO Doctors (Name, Specialization, PhoneNumber, Email, HospitalID, RegisterDateTime,ConsultationFee) " +
                                   "VALUES (@Name, @Specialization, @PhoneNumber, @Email, @HospitalID,GETDATE(),@ConsultationFee)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", doctor.Name);
                        cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization);
                        cmd.Parameters.AddWithValue("@PhoneNumber", doctor.PhoneNumber);
                        cmd.Parameters.AddWithValue("@Email", doctor.Email);
                        cmd.Parameters.AddWithValue("@HospitalID", doctor.HospitalID);
                        cmd.Parameters.AddWithValue("@ConsultationFee", doctor.ConsultationFee);
                        int result = cmd.ExecuteNonQuery();
                        return result > 0
                            ? Ok(new { message = "Doctor added successfully!" })
                            : BadRequest(new { message = "Failed to add doctor" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ PUT update doctor
        [HttpPut("update/{id}")]
public IActionResult UpdateDoctor(int id, [FromBody] DoctorDto doctor)
{
    try
    {
        using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
        {
            conn.Open();

            string query = "UPDATE Doctors " +
                "SET Name = @Name, Specialization = @Specialization, PhoneNumber = @PhoneNumber, Email = @Email, ConsultationFee = @ConsultationFee " + // Fixed space issue
                "WHERE DoctorID = @DoctorID AND HospitalID = @HospitalID;";

            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@DoctorID", id);
                cmd.Parameters.AddWithValue("@Name", doctor.Name);
                cmd.Parameters.AddWithValue("@Specialization", doctor.Specialization);
                cmd.Parameters.AddWithValue("@PhoneNumber", doctor.PhoneNumber);
                cmd.Parameters.AddWithValue("@Email", doctor.Email);
                cmd.Parameters.AddWithValue("@HospitalID", doctor.HospitalID);
                cmd.Parameters.AddWithValue("@ConsultationFee", doctor.ConsultationFee);

                int result = cmd.ExecuteNonQuery();
                return result > 0
                    ? Ok(new { message = "Doctor updated successfully!" })
                    : NotFound(new { message = "Doctor not found" });
            }
        }
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error: " + ex.Message });
    }
}

        // ✅ DELETE doctor
        [HttpDelete("delete/{id}")]
        public IActionResult DeleteDoctor(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Doctors WHERE DoctorID = @DoctorID AND HospitalID = @HospitalID;";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@DoctorID", id);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0
                            ? Ok(new { message = "Doctor deleted successfully!" })
                            : NotFound(new { message = "Doctor not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        [HttpGet("by-hospital/{hospitalId}")]
        public IActionResult GetDoctorsByHospital(int hospitalId)
        {
            try
            {
                List<object> doctors = new List<object>(); // Properly initialize the list

                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = @"
            SELECT DoctorID, Name, Specialization, PhoneNumber, Email, 
                   HospitalID, RegisterDateTime, ConsultationFee 
            FROM Doctors 
            WHERE HospitalID = @HospitalID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@HospitalID", hospitalId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                doctors.Add(new
                                {
                                    id = reader["DoctorID"],
                                    name = reader["Name"],
                                    specialization = reader["Specialization"],
                                    phoneNumber = reader["PhoneNumber"],
                                    email = reader["Email"],
                                    hospitalId = reader["HospitalID"],
                                    registerDateTime = reader["RegisterDateTime"] != DBNull.Value ? Convert.ToDateTime(reader["RegisterDateTime"]) : (DateTime?)null,
                                    consultationFee = reader["ConsultationFee"] != DBNull.Value ? Convert.ToDecimal(reader["ConsultationFee"]) : 0m
                                });
                            }
                        }
                    }
                }

                // Always return OK with an empty list if no doctors are found
                return Ok(new
                {
                    message = doctors.Count > 0 ? "Success" : "No doctors found for this hospital",
                    data = doctors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

    }
}

