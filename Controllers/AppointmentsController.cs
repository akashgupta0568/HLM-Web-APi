using HLM_Web_APi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HLM_Web_APi.Controllers
{
    [Route("api/appointments")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public AppointmentsController(SqlConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        // ✅ Get all appointments
        [HttpGet("list")]
        public IActionResult GetAppointments()
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT AppointmentID, PatientID, DoctorID, AppointmentDate, AppointmentTime, Status, ConsultationFee FROM Appointments";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        List<object> appointments = new List<object>();

                        while (reader.Read())
                        {
                            appointments.Add(new
                            {
                                AppointmentID = reader["AppointmentID"],
                                PatientID = reader["PatientID"],
                                DoctorID = reader["DoctorID"],
                                AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]).ToString("yyyy-MM-dd"),
                                AppointmentTime = reader["AppointmentTime"].ToString(),
                                Status = reader["Status"],
                                ConsultationFee = reader["ConsultationFee"]
                            });
                        }

                        return Ok(appointments);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ Get appointment by ID
        [HttpGet("{id}")]
        public IActionResult GetAppointmentById(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT AppointmentID, PatientID, DoctorID, AppointmentDate, AppointmentTime, Status, ConsultationFee FROM Appointments WHERE AppointmentID = @AppointmentID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID", id);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return Ok(new
                                {
                                    AppointmentID = reader["AppointmentID"],
                                    PatientID = reader["PatientID"],
                                    DoctorID = reader["DoctorID"],
                                    AppointmentDate = Convert.ToDateTime(reader["AppointmentDate"]).ToString("yyyy-MM-dd"),
                                    AppointmentTime = reader["AppointmentTime"].ToString(),
                                    Status = reader["Status"],
                                    ConsultationFee = reader["ConsultationFee"]
                                });
                            }
                        }
                    }
                }

                return NotFound(new { message = "Appointment not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ Add a new appointment
        [HttpPost("add")]
        public IActionResult AddAppointment([FromBody] AppointmentDto appointment)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    // Check if Doctor and Patient exist
                    string checkQuery = "SELECT COUNT(*) FROM Doctors WHERE DoctorID = @DoctorID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@DoctorID", appointment.DoctorID);
                        int doctorExists = (int)checkCmd.ExecuteScalar();
                        if (doctorExists == 0)
                        {
                            return BadRequest(new { message = "Error: Doctor does not exist!" });
                        }
                    }

                    checkQuery = "SELECT COUNT(*) FROM Patients WHERE PatientID = @PatientID";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PatientID", appointment.PatientID);
                        int patientExists = (int)checkCmd.ExecuteScalar();
                        if (patientExists == 0)
                        {
                            return BadRequest(new { message = "Error: Patient does not exist!" });
                        }
                    }

                    // Insert appointment
                    string query = "INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, AppointmentTime, Status, ConsultationFee) " +
                                   "VALUES (@PatientID, @DoctorID, @AppointmentDate, @AppointmentTime, @Status, @ConsultationFee)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", appointment.PatientID);
                        cmd.Parameters.AddWithValue("@DoctorID", appointment.DoctorID);
                        cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                        cmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);
                        cmd.Parameters.AddWithValue("@Status", appointment.Status);
                        cmd.Parameters.AddWithValue("@ConsultationFee", appointment.ConsultationFee);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0
                            ? Ok(new { message = "Appointment added successfully!" })
                            : BadRequest(new { message = "Failed to add appointment" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ Update appointment
        [HttpPut("update/{id}")]
        public IActionResult UpdateAppointment(int id, [FromBody] AppointmentDto appointment)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    string query = "UPDATE Appointments " +
                                   "SET PatientID = @PatientID, DoctorID = @DoctorID, AppointmentDate = @AppointmentDate, AppointmentTime = @AppointmentTime, Status = @Status, ConsultationFee = @ConsultationFee " +
                                   "WHERE AppointmentID = @AppointmentID";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID", id);
                        cmd.Parameters.AddWithValue("@PatientID", appointment.PatientID);
                        cmd.Parameters.AddWithValue("@DoctorID", appointment.DoctorID);
                        cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                        cmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);
                        cmd.Parameters.AddWithValue("@Status", appointment.Status);
                        cmd.Parameters.AddWithValue("@ConsultationFee", appointment.ConsultationFee);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0
                            ? Ok(new { message = "Appointment updated successfully!" })
                            : NotFound(new { message = "Appointment not found" });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        // ✅ Add appointment & automatically insert patient if they don't exist
        [HttpPost("Directadd")]
        public IActionResult AddAppointment([FromBody] AppointmentWithPatient appointment)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();

                    // 🔹 Check if patient exists
                    string checkPatientQuery = "SELECT PatientID FROM Patients WHERE Phone = @PhoneNumber AND HospitalID = @HospitalID";
                    int patientID = 0;

                    using (SqlCommand checkCmd = new SqlCommand(checkPatientQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@PhoneNumber", appointment.PhoneNumber);
                        checkCmd.Parameters.AddWithValue("@HospitalID", appointment.HospitalID);
                        object result = checkCmd.ExecuteScalar();
                        if (result != null)
                        {
                            patientID = Convert.ToInt32(result); // Patient exists
                        }
                    }

                    // 🔹 If patient does not exist, insert into Patients
                    if (patientID == 0)
                    {
                        string insertPatientQuery = "INSERT INTO Patients (Name, Phone, Email, Gender, DateOfBirth, HospitalID, CreatedAt) " +
                                                    "OUTPUT INSERTED.PatientID VALUES (@FullName, @PhoneNumber, @Email, @Gender, @DateOfBirth, @HospitalID, GETDATE())";
                        using (SqlCommand insertCmd = new SqlCommand(insertPatientQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@FullName", appointment.FullName);
                            insertCmd.Parameters.AddWithValue("@PhoneNumber", appointment.PhoneNumber);
                            insertCmd.Parameters.AddWithValue("@Email", appointment.Email);
                            insertCmd.Parameters.AddWithValue("@Gender", appointment.Gender);
                            insertCmd.Parameters.AddWithValue("@DateOfBirth", appointment.DateOfBirth);
                            insertCmd.Parameters.AddWithValue("@HospitalID", appointment.HospitalID);

                            patientID = (int)insertCmd.ExecuteScalar(); // Get the new PatientID
                        }
                    }

                    // 🔹 Insert appointment
                    string insertAppointmentQuery = "INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, AppointmentTime, Status, AppointmentFee) " +
                                                    "VALUES (@PatientID, @DoctorID, @AppointmentDate, @AppointmentTime, @Status, @AppointmentFee)";

                    using (SqlCommand cmd = new SqlCommand(insertAppointmentQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@PatientID", patientID);
                        cmd.Parameters.AddWithValue("@DoctorID", appointment.DoctorID);
                        cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                        cmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);
                        cmd.Parameters.AddWithValue("@Status", appointment.Status);
                        cmd.Parameters.AddWithValue("@AppointmentFee", appointment.AppointmentFee);

                        int result = cmd.ExecuteNonQuery();
                        return result > 0
                            ? Ok(new { message = "Appointment added successfully!", PatientID = patientID })
                            : BadRequest(new { message = "Failed to add appointment" });
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
