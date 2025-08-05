using HLM_Web_APi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Security.Claims;

namespace HLM_Web_APi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SubscriptionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("plans")]
        public IActionResult GetPlans()
        {
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            SqlCommand cmd = new("Sp_GetAllSubscriptionPlans", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            conn.Open();
            var reader = cmd.ExecuteReader();
            var plans = new List<object>();
            while (reader.Read())
            {
                plans.Add(new
                {
                    PlanId = reader.GetInt32(0),
                    PlanName = reader.GetString(1),
                    Description = reader.GetString(2),
                    DurationDays = reader.GetInt32(3),
                    Price = reader.GetDecimal(4)
                });
            }
            return Ok(plans);
        }

        [HttpGet("isExpired")]
        public IActionResult IsSubscriptionExpired()
        {
            var userId = GetUserIdFromToken(); // from JWT
            //var userId = 43;
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            SqlCommand cmd = new("Sp_CheckSubscriptionExpiry", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", userId);
            conn.Open();
            var result = cmd.ExecuteScalar();
            return Ok((int)result == 1);
        }

        [HttpPost("subscribe10x")]
        public IActionResult Subscribe([FromBody] int planId)
        {
            var userId = GetUserIdFromToken();
            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
            SqlCommand cmd = new("Sp_SubscribeToPlan", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@PlanId", planId);
            conn.Open();
            cmd.ExecuteNonQuery();
            return Ok();
        }


        [HttpPost("subscribe")]
        public IActionResult Subscribe([FromBody] SubscriptionRequest req)
        {
            int userId = GetUserIdFromToken(); // ✅ Secure way to get UserID from token

            using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));

            try
            {
                // 1. First, Add the subscription using the stored procedure
                using (SqlCommand cmd = new("Sp_AddUserSubscription", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@UserID", userId);
                    cmd.Parameters.AddWithValue("@PlanID", req.PlanId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }

                // 2. Then, fetch the most recent subscription for the user
                using (SqlCommand fetchCmd = new("Sp_GetUserSubscription", conn))
                {
                    fetchCmd.CommandType = CommandType.StoredProcedure;
                    fetchCmd.Parameters.AddWithValue("@UserID", userId);

                    conn.Open();
                    using SqlDataReader reader = fetchCmd.ExecuteReader();

                    var subscriptions = new List<object>();

                    while (reader.Read())
                    {
                        subscriptions.Add(new
                        {
                            SubscriptionID = reader["SubscriptionID"],
                            UserID = reader["UserID"],
                            PlanID = reader["PlanID"],
                            StartDate = reader["StartDate"],
                            EndDate = reader["EndDate"],
                            IsActive = reader["IsActive"]
                        });
                    }

                    return Ok(new
                    {
                        message = "Subscription added successfully.",
                        subscription = subscriptions
                    });
                }
            }
            catch (SqlException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }




        [HttpGet("user-subscription")]
        public IActionResult GetUserSubscription()
        {
            int userId = GetUserIdFromToken();

            var result = new List<object>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("Sp_GetUserSubscription", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new
                        {
                            SubscriptionID = reader["SubscriptionID"],
                            UserID = reader["UserID"],
                            PlanID = reader["PlanID"],
                            StartDate = reader["StartDate"],
                            EndDate = reader["EndDate"],
                            IsActive = reader["IsActive"],
                            PlanName = reader["PlanName"],
                            DurationDays = reader["DurationDays"],
                            Price = reader["Price"]
                        });
                    }
                }
            }

            if (result.Count == 0)
                return NotFound("No active subscription found for this user");

            return Ok(result);
        }


        [HttpGet("user-subscription/{userId}")]
        public IActionResult GetUserSubscription(int userId)
        {
            var result = new List<object>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand("Sp_GetUserSubscription", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@UserID", userId);

                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new
                        {
                            SubscriptionID = reader["SubscriptionID"],
                            UserID = reader["UserID"],
                            PlanID = reader["PlanID"],
                            StartDate = reader["StartDate"],
                            EndDate = reader["EndDate"],
                            IsActive = reader["IsActive"],
                            PlanName = reader["PlanName"],
                            DurationDays = reader["DurationDays"],
                            Price = reader["Price"]
                        });
                    }
                }
            }

            if (result.Count == 0)
                return NotFound($"No active subscription found for user ID {userId}");

            return Ok(result);
        }

        private int GetUserIdFromToken()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userId!);
        }

        //[HttpGet("isExpired")]
        //public IActionResult IsSubscriptionExpired10x()
        //{
        //    var userId = GetUserIdFromToken();

        //    using SqlConnection conn = new(_configuration.GetConnectionString("DefaultConnection"));
        //    SqlCommand cmd = new("Sp_CheckSubscriptionExpiry", conn)
        //    {
        //        CommandType = CommandType.StoredProcedure
        //    };
        //    cmd.Parameters.AddWithValue("@UserId", userId);
        //    conn.Open();

        //    var result = cmd.ExecuteScalar();
        //    bool isExpired = (int)result == 1;

        //    return Ok(isExpired);
        //}

    }


}
