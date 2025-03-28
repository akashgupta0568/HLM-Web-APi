using HLM_Web_APi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HLM_Web_APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public PaymentsController(SqlConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }
        [HttpGet]
        public IActionResult GetPayments()
        {
            List<object> payments = new List<object>();

            using (SqlConnection con = new SqlConnection(_connection.ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = "SELECT * FROM Payments";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(new
                            {
                                PaymentID = reader["PaymentID"],
                                AppointmentID = reader["AppointmentID"],
                                PatientID = reader["PatientID"],
                                DoctorID = reader["DoctorID"],
                                HospitalID = reader["HospitalID"],
                                Amount = reader["Amount"],
                                PaymentDate = reader["PaymentDate"],  // Added PaymentDate
                                PaymentStatus = reader["PaymentStatus"],
                                PaymentMethod = reader["PaymentMethod"],
                                TransactionID = reader["TransactionID"],
                                CreatedAt = reader["CreatedAt"]
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }

            return Ok(payments);
        }

        [HttpPost]
        public IActionResult CreatePayment([FromBody] Payment payment)
        {
            using (SqlConnection con = new SqlConnection(_connection.ConnectionString))
            {
                try
                {
                    con.Open();
                    string query = @"
                INSERT INTO Payments (AppointmentID, PatientID, DoctorID, HospitalID, Amount, PaymentDate, PaymentStatus, PaymentMethod, TransactionID)
                VALUES (@AppointmentID, @PatientID, @DoctorID, @HospitalID, @Amount, @PaymentDate, @PaymentStatus, @PaymentMethod, @TransactionID)";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@AppointmentID", payment.AppointmentID);
                        cmd.Parameters.AddWithValue("@PatientID", payment.PatientID);
                        cmd.Parameters.AddWithValue("@DoctorID", payment.DoctorID);
                        cmd.Parameters.AddWithValue("@HospitalID", payment.HospitalID);
                        cmd.Parameters.AddWithValue("@Amount", payment.Amount);
                        cmd.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);  // Added PaymentDate
                        cmd.Parameters.AddWithValue("@PaymentStatus", payment.PaymentStatus);
                        cmd.Parameters.AddWithValue("@PaymentMethod", payment.PaymentMethod ?? (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@TransactionID", payment.TransactionID ?? (object)DBNull.Value);

                        int result = cmd.ExecuteNonQuery();
                        if (result > 0)
                            return Ok("Payment created successfully");
                        else
                            return BadRequest("Failed to create payment");
                    }
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Internal Server Error: {ex.Message}");
                }
            }
        }

    }
}
