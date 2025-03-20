using HLM_Web_APi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HLM_Web_APi.Controllers
{
    [Route("api/patients")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public PatientsController(SqlConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        // ✅ GET all patients
        [HttpGet("list")]
        public IActionResult GetPatients()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT PatientID, Name, Phone, Email, Gender, DateOfBirth, HospitalID, CreatedAt FROM Patients";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<object> patients = new List<object>();

                        while (reader.Read())
                        {
                            patients.Add(new
                            {
                                PatientID = reader["PatientID"],
                                Name = reader["Name"],
                                Phone = reader["Phone"],
                                Email = reader["Email"],
                                Gender = reader["Gender"],
                                DateOfBirth = reader["DateOfBirth"],
                                HospitalID = reader["HospitalID"],
                                CreatedAt = reader["CreatedAt"]
                            });
                        }

                        return Ok(patients);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ GET patient by ID
        [HttpGet("{id}")]
        public IActionResult GetPatientById(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT PatientID, Name, Phone, Email, Gender, DateOfBirth, HospitalID, CreatedAt FROM Patients WHERE PatientID = @PatientID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new
                                {
                                    PatientID = reader["PatientID"],
                                    Name = reader["Name"],
                                    Phone = reader["Phone"],
                                    Email = reader["Email"],
                                    Gender = reader["Gender"],
                                    DateOfBirth = reader["DateOfBirth"],
                                    HospitalID = reader["HospitalID"],
                                    CreatedAt = reader["CreatedAt"]
                                });
                            }
                        }
                    }
                }

                return NotFound(new { message = "Patient not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ POST add new patient
        [HttpPost("add")]
        public IActionResult AddPatient([FromBody] PatientDto patient)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    // Check if Phone or Email already exists
                    string checkQuery = "SELECT COUNT(*) FROM Patients WHERE Email = @Email OR Phone = @Phone";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Email", patient.Email);
                        checkCmd.Parameters.AddWithValue("@Phone", patient.Phone);

                        int existingCount = (int)checkCmd.ExecuteScalar();
                        if (existingCount > 0)
                        {
                            return BadRequest(new { message = "Error: Phone Number or Email already exists!" });
                        }
                    }

                    // Insert new patient
                    string query = "INSERT INTO Patients (Name, Phone, Email, Gender, DateOfBirth, HospitalID, CreatedAt) " +
                                   "VALUES (@Name, @Phone, @Email, @Gender, @DateOfBirth, @HospitalID, GETDATE())";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Name", patient.Name);
                        cmd.Parameters.AddWithValue("@Phone", patient.Phone);
                        cmd.Parameters.AddWithValue("@Email", patient.Email);
                        cmd.Parameters.AddWithValue("@Gender", patient.Gender);
                        cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);
                        cmd.Parameters.AddWithValue("@HospitalID", patient.HospitalID);
                        int result = cmd.ExecuteNonQuery();
                        return result > 0
                            ? Ok(new { message = "Patient added successfully!" })
                            : BadRequest(new { message = "Failed to add patient" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ PUT update patient
        [HttpPut("update/{id}")]
        public IActionResult UpdatePatient(int id, [FromBody] PatientDto patient)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    string query = "UPDATE Patients " +
                                   "SET Name = @Name, Phone = @Phone, Email = @Email, Gender = @Gender, DateOfBirth = @DateOfBirth, HospitalID = @HospitalID " +
                                   "WHERE PatientID = @PatientID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", id);
                        cmd.Parameters.AddWithValue("@Name", patient.Name);
                        cmd.Parameters.AddWithValue("@Phone", patient.Phone);
                        cmd.Parameters.AddWithValue("@Email", patient.Email);
                        cmd.Parameters.AddWithValue("@Gender", patient.Gender);
                        cmd.Parameters.AddWithValue("@DateOfBirth", patient.DateOfBirth);
                        cmd.Parameters.AddWithValue("@HospitalID", patient.HospitalID);
                        int result = cmd.ExecuteNonQuery();
                        return result > 0
                            ? Ok(new { message = "Patient updated successfully!" })
                            : NotFound(new { message = "Patient not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ DELETE patient
        [HttpDelete("delete/{id}")]
        public IActionResult DeletePatient(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    string query = "DELETE FROM Patients WHERE PatientID = @PatientID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", id);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0
                            ? Ok(new { message = "Patient deleted successfully!" })
                            : NotFound(new { message = "Patient not found" });
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
