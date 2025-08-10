using HLM_Web_APi.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace HLM_Web_APi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternalUsersController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public InternalUsersController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("register")]
        public IActionResult RegisterInternalUser([FromBody] InternalUserDto user)
        {
            try
            {
                // ✅ Generate hashed password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password);

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_AddInternalUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Map parameters to stored procedure
                        cmd.Parameters.AddWithValue("@HospitalId", user.HospitalId);
                        cmd.Parameters.AddWithValue("@AdminRoleID", user.AdminRoleID);
                        cmd.Parameters.AddWithValue("@Name", user.Name);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Phone", user.Phone);
                        cmd.Parameters.AddWithValue("@JoiningDate", user.JoiningDate);
                        cmd.Parameters.AddWithValue("@Salary", user.Salary);
                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@PasswordHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@CreatedByUserId", user.CreatedByUserId);
                        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                        cmd.Parameters.AddWithValue("@AssignedByAdminRole", user.AssignedByAdminRole);
                        cmd.Parameters.AddWithValue("@Password", user.Password); 

                        SqlParameter outputIdParam = new SqlParameter("@InternalUserId", SqlDbType.Int)
                        {
                            Direction = ParameterDirection.Output
                        };
                        cmd.Parameters.Add(outputIdParam);

                        cmd.ExecuteNonQuery();

                        int internalUserId = (int)outputIdParam.Value;

                        if (internalUserId == -1)
                        {
                            return BadRequest(new { message = "User with this email, username, or phone already exists!" });
                        }

                        return Ok(new
                        {
                            message = "Internal user registered successfully!",
                            internalUserId = internalUserId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        [HttpGet("GetAll")]
        public IActionResult GetAllInternalUsers()
        {
            try
            {
                List<InternalUserDto> users = new List<InternalUserDto>();

                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_GetAllInternalUsers", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new InternalUserDto
                                {
                                    HospitalId = Convert.ToInt32(reader["HospitalId"]),
                                    AdminRoleID = Convert.ToInt32(reader["AdminRoleID"]),
                                    Name = reader["Name"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Phone = reader["Phone"].ToString(),
                                    JoiningDate = Convert.ToDateTime(reader["JoiningDate"]),
                                    Salary = Convert.ToDecimal(reader["Salary"]),
                                    Username = reader["Username"].ToString(),
                                    CreatedByUserId = Convert.ToInt32(reader["CreatedByUserId"]),
                                    IsActive = Convert.ToBoolean(reader["IsActive"]),
                                    AssignedByAdminRole = reader["AssignedByAdminRole"].ToString()
                                });
                            }
                        }
                    }
                }

                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateInternalUser(int id, [FromBody] InternalUserDto user)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_UpdateInternalUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.AddWithValue("@InternalUserId", id);
                        cmd.Parameters.AddWithValue("@HospitalId", user.HospitalId);
                        cmd.Parameters.AddWithValue("@AdminRoleID", user.AdminRoleID);
                        cmd.Parameters.AddWithValue("@Name", user.Name);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@Phone", user.Phone);
                        cmd.Parameters.AddWithValue("@JoiningDate", user.JoiningDate);
                        cmd.Parameters.AddWithValue("@Salary", user.Salary);
                        cmd.Parameters.AddWithValue("@Username", user.Username);
                        cmd.Parameters.AddWithValue("@CreatedByUserId", user.CreatedByUserId);
                        cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                        cmd.Parameters.AddWithValue("@AssignedByAdminRole", user.AssignedByAdminRole);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            return NotFound(new { message = "User not found" });
                    }
                }

                return Ok(new { message = "Internal user updated successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public IActionResult DeleteInternalUser(int id)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();

                    using (SqlCommand cmd = new SqlCommand("sp_DeleteInternalUser", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@InternalUserId", id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                            return NotFound(new { message = "User not found" });
                    }
                }

                return Ok(new { message = "Internal user deleted successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error: " + ex.Message });
            }
        }


    }
}
