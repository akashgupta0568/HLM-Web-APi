using ClosedXML.Excel;
using HLM_Web_APi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Data.Common;
using System.Globalization;

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
                    string query = "SELECT AppointmentID, PatientID, DoctorID, AppointmentDate, AppointmentTime, Status, AppointmentFee FROM Appointments";

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
                                ConsultationFee = reader["AppointmentFee"]
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
                    string query = "INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, AppointmentTime, Status, AppointmentFee) " +
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

        [HttpPost("DirectaddAppPay")]
        public IActionResult PaymentAndAppointment([FromBody] AppointmentWithPatient appointment)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction()) // 🔹 Start Transaction
                    {
                        try
                        {
                            // 🔹 Insert a new patient (No check for existing patient)
                            string insertPatientQuery = @"
                    INSERT INTO Patients (Name, FatherHusbandName, Phone, Email, Gender, Age, HospitalID, CreatedAt) 
                    OUTPUT INSERTED.PatientID 
                    VALUES (@FullName, @FatherHusbandName, @PhoneNumber, @Email, @Gender, @Age, @HospitalID, GETDATE())";

                            int patientID;
                            using (SqlCommand insertCmd = new SqlCommand(insertPatientQuery, conn, transaction))
                            {
                                insertCmd.Parameters.AddWithValue("@FullName", appointment.FullName);
                                insertCmd.Parameters.AddWithValue("@FatherHusbandName", appointment.FatherHusbandName);
                                insertCmd.Parameters.AddWithValue("@PhoneNumber", appointment.PhoneNumber);
                                insertCmd.Parameters.AddWithValue("@Email", appointment.Email);
                                insertCmd.Parameters.AddWithValue("@Gender", appointment.Gender);
                                insertCmd.Parameters.AddWithValue("@Age", appointment.Age);
                                insertCmd.Parameters.AddWithValue("@HospitalID", appointment.HospitalID);

                                patientID = (int)insertCmd.ExecuteScalar();
                            }

                            // 🔹 Insert appointment
                            string insertAppointmentQuery = @"
                    INSERT INTO Appointments (PatientID, DoctorID, AppointmentDate, AppointmentTime, Status, AppointmentFee, Date, Week, HospitalID) 
                    OUTPUT INSERTED.AppointmentID 
                    VALUES (@PatientID, @DoctorID, @AppointmentDate, @AppointmentTime, @Status, @AppointmentFee, @AppointmentDate, DATENAME(WEEKDAY, @AppointmentDate), @HospitalID)";

                            int appointmentID;
                            using (SqlCommand cmd = new SqlCommand(insertAppointmentQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@PatientID", patientID);
                                cmd.Parameters.AddWithValue("@DoctorID", appointment.DoctorID);
                                cmd.Parameters.AddWithValue("@AppointmentDate", appointment.AppointmentDate);
                                cmd.Parameters.AddWithValue("@AppointmentTime", appointment.AppointmentTime);
                                cmd.Parameters.AddWithValue("@Status", appointment.Status);
                                cmd.Parameters.AddWithValue("@AppointmentFee", appointment.AppointmentFee);
                                cmd.Parameters.AddWithValue("@HospitalID", appointment.HospitalID);

                                appointmentID = (int)cmd.ExecuteScalar();
                            }

                            if (appointmentID > 0)
                            {
                                // 🔹 Insert payment
                                string insertPaymentQuery = @"
                        INSERT INTO Payments (AppointmentID, PatientID, DoctorID, HospitalID, Amount, PaymentDate, PaymentStatus, PaymentMethod, TransactionID, CreatedAt) 
                        VALUES (@AppointmentID, @PatientID, @DoctorID, @HospitalID, @Amount, @PaymentDate, @PaymentStatus, @PaymentMethod, @TransactionID, GETDATE())";

                                using (SqlCommand paymentCmd = new SqlCommand(insertPaymentQuery, conn, transaction))
                                {
                                    paymentCmd.Parameters.AddWithValue("@AppointmentID", appointmentID);
                                    paymentCmd.Parameters.AddWithValue("@PatientID", patientID);
                                    paymentCmd.Parameters.AddWithValue("@DoctorID", appointment.DoctorID);
                                    paymentCmd.Parameters.AddWithValue("@HospitalID", appointment.HospitalID);
                                    paymentCmd.Parameters.AddWithValue("@Amount", appointment.AppointmentFee);
                                    paymentCmd.Parameters.AddWithValue("@PaymentDate", DateTime.Now);
                                    paymentCmd.Parameters.AddWithValue("@PaymentStatus", "Completed");
                                    paymentCmd.Parameters.AddWithValue("@PaymentMethod", appointment.PaymentMethod);
                                    paymentCmd.Parameters.AddWithValue("@TransactionID", appointment.TransactionID);

                                    paymentCmd.ExecuteNonQuery();
                                }

                                // 🔹 Commit transaction if everything is successful
                                transaction.Commit();
                                return Ok(new { message = "Appointment and Payment added successfully!", PatientID = patientID, AppointmentID = appointmentID });
                            }
                            else
                            {
                                // 🔹 Rollback transaction if appointment insertion failed
                                transaction.Rollback();
                                return BadRequest(new { message = "Failed to add appointment" });
                            }
                        }
                        catch (Exception ex)
                        {
                            // 🔹 Rollback transaction in case of any error
                            transaction.Rollback();
                            return StatusCode(500, new { message = "Error: " + ex.Message });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        [HttpPost("GetAppointments")]
        public IActionResult GetAppointments([FromBody] AppointmentRequest request)
        {
            try
            {
                List<AppointmentResponse> appointments = new List<AppointmentResponse>();

                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("GetAppointmentsByDoctorAndHospital", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@HospitalID", request.HospitalId);
                        cmd.Parameters.AddWithValue("@DoctorID", request.DoctorId);
                        cmd.Parameters.AddWithValue("@FromDate", (request.FromDate ?? DateTime.Today).Date);
                        cmd.Parameters.AddWithValue("@ToDate", (request.ToDate ?? DateTime.Today).Date);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                appointments.Add(new AppointmentResponse
                                {
                                    HospitalID = reader.GetInt32(0),
                                    HospitalName = reader["HospitalName"] as string,
                                    AppointmentID = reader.GetInt32(2),
                                    AppointmentDate = (DateTime)(reader["AppointmentDate"] as DateTime?),
                                    AppointmentTime = reader["AppointmentTime"] != DBNull.Value
                                        ? DateTime.Today.Add(reader.GetTimeSpan(4)).ToString("hh:mm tt", CultureInfo.InvariantCulture)
                                        : null,
                                    AppointmentStatus = reader["AppointmentStatus"] as string,
                                    PatientID = (int)(reader["PatientID"] as int?),
                                    PatientName = reader["PatientName"] as string,
                                    PatientPhone = reader["PatientPhone"] as string,
                                    FatherHusbandName = reader["FatherHusbandName"] as string,
                                    DoctorID = (int)(reader["DoctorID"] as int?),
                                    DoctorName = reader["DoctorName"] as string,
                                    ConsultationFee = reader.GetDecimal(reader.GetOrdinal("ConsultationFee")),
                                    Specialization = reader["Specialization"] as string,
                                    DoctorPhone = reader["DoctorPhone"] as string,
                                    Amount = reader["Amount"] as decimal? ?? null,
                                    PaymentID = reader["PaymentID"] as int?,
                                    PaymentDate = reader["PaymentDate"] as DateTime?,
                                    PaymentStatus = reader["PaymentStatus"] as string,
                                    PaymentMethod = reader["PaymentMethod"] as string
                                });
                            }
                        }
                    }
                }

                return Ok(new
                {
                    message = appointments.Count == 0 ? "No appointments available for today." : "Appointments retrieved successfully.",
                    data = appointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        [HttpPost("DownloadAppointments")]
        public IActionResult DownloadAppointments([FromBody] AppointmentRequest request)
        {
            try
            {
                List<AppointmentResponse> appointments = new List<AppointmentResponse>();

                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("GetAppointmentsByDoctorAndHospital", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@HospitalID", request.HospitalId);
                        cmd.Parameters.AddWithValue("@DoctorID", request.DoctorId);
                        cmd.Parameters.AddWithValue("@FromDate", (request.FromDate ?? DateTime.Today).Date);
                        cmd.Parameters.AddWithValue("@ToDate", (request.ToDate ?? DateTime.Today).Date);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                appointments.Add(new AppointmentResponse
                                {
                                    HospitalID = reader.GetInt32(0),
                                    HospitalName = reader["HospitalName"] as string,
                                    AppointmentID = reader.GetInt32(2),
                                    AppointmentDate = (DateTime)(reader["AppointmentDate"] as DateTime?),
                                    AppointmentTime = reader["AppointmentTime"] != DBNull.Value
                                        ? DateTime.Today.Add(reader.GetTimeSpan(4)).ToString("hh:mm tt", CultureInfo.InvariantCulture)
                                        : null,
                                    AppointmentStatus = reader["AppointmentStatus"] as string,
                                    PatientID = (int)(reader["PatientID"] as int?),
                                    PatientName = reader["PatientName"] as string,
                                    PatientPhone = reader["PatientPhone"] as string,
                                    FatherHusbandName = reader["FatherHusbandName"] as string,
                                    DoctorID = (int)(reader["DoctorID"] as int?),
                                    DoctorName = reader["DoctorName"] as string,
                                    ConsultationFee = reader.GetDecimal(reader.GetOrdinal("ConsultationFee")),
                                    Specialization = reader["Specialization"] as string,
                                    DoctorPhone = reader["DoctorPhone"] as string,
                                    Amount = reader["Amount"] as decimal? ?? null,
                                    PaymentID = reader["PaymentID"] as int?,
                                    PaymentDate = reader["PaymentDate"] as DateTime?,
                                    PaymentStatus = reader["PaymentStatus"] as string,
                                    PaymentMethod = reader["PaymentMethod"] as string
                                });
                            }
                        }
                    }
                }

                if (appointments.Count == 0)
                {
                    return NotFound(new { message = "No appointments available." });
                }

                // Create Excel file
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Appointments");

                    // Create Header
                    worksheet.Cell(1, 1).Value = "Hospital ID";
                    worksheet.Cell(1, 2).Value = "Hospital Name";
                    worksheet.Cell(1, 3).Value = "Appointment ID";
                    worksheet.Cell(1, 4).Value = "Appointment Date";
                    worksheet.Cell(1, 5).Value = "Appointment Time";
                    worksheet.Cell(1, 6).Value = "Status";
                    worksheet.Cell(1, 7).Value = "Patient Name";
                    worksheet.Cell(1, 8).Value = "Patient Phone";
                    worksheet.Cell(1, 9).Value = "Doctor Name";
                    worksheet.Cell(1, 10).Value = "Specialization";
                    worksheet.Cell(1, 11).Value = "Consultation Fee";
                    worksheet.Cell(1, 12).Value = "Patient Amount";
                    worksheet.Cell(1, 13).Value = "Payment Status";
                    worksheet.Cell(1, 14).Value = "Payment Method";
                    worksheet.Cell(1, 15).Value = "Payment Date";

                    // Fill Data
                    int row = 2;
                    foreach (var appointment in appointments)
                    {
                        worksheet.Cell(row, 1).Value = appointment.HospitalID;
                        worksheet.Cell(row, 2).Value = appointment.HospitalName;
                        worksheet.Cell(row, 3).Value = appointment.AppointmentID;
                        worksheet.Cell(row, 4).Value = appointment.AppointmentDate.ToString("yyyy-MM-dd");
                        worksheet.Cell(row, 5).Value = appointment.AppointmentTime;
                        worksheet.Cell(row, 6).Value = appointment.AppointmentStatus;
                        worksheet.Cell(row, 7).Value = appointment.PatientName;
                        worksheet.Cell(row, 8).Value = appointment.PatientPhone;
                        worksheet.Cell(row, 9).Value = appointment.DoctorName;
                        worksheet.Cell(row, 10).Value = appointment.Specialization;
                        worksheet.Cell(row, 11).Value = appointment.ConsultationFee;
                        worksheet.Cell(row, 12).Value = appointment.Amount;
                        worksheet.Cell(row, 13).Value = appointment.PaymentStatus;
                        worksheet.Cell(row, 14).Value = appointment.PaymentMethod;
                        worksheet.Cell(row, 15).Value = appointment.PaymentDate?.ToString("yyyy-MM-dd");
                        row++;
                    }

                    // Auto-adjust columns for better readability
                    worksheet.Columns().AdjustToContents();

                    // Save to MemoryStream
                    using (var stream = new System.IO.MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Appointments.xlsx");
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

