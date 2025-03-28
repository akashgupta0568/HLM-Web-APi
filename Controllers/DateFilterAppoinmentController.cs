using HLM_Web_APi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HLM_Web_APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DateFilterAppoinmentController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _configuration;

        public DateFilterAppoinmentController(SqlConnection connection, IConfiguration configuration)
        {
            _connection = connection;
            _configuration = configuration;
        }

        [HttpGet("filter")]
        public async Task<IActionResult> GetAppointmentsByDateRange(DateTime startDate, DateTime endDate)
        {
            List<FilterAppointment> appointments = new List<FilterAppointment>();

            try
            {
                using (SqlConnection conn = new SqlConnection(_connection.ConnectionString))
                {
                    await conn.OpenAsync();
                    using (SqlCommand cmd = new SqlCommand("GetAppointmentsByDateRange", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                        cmd.Parameters.AddWithValue("@EndDate", endDate);

                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                appointments.Add(new FilterAppointment
                                {
                                    AppointmentID = reader.GetInt32(0),
                                    AppointmentDate = reader.GetDateTime(1),
                                    WeekDay = reader.GetString(2),
                                    Status = reader.GetString(3),
                                    PatientID = reader.GetInt32(4),
                                    DoctorID = reader.GetInt32(5),
                                    AppointmentTime = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    AppointmentFee = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                                    Date = reader.GetDateTime(8)
                                });
                            }
                        }
                    }
                }

                return Ok(appointments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error fetching data", error = ex.Message });
            }
        }
    }
}
