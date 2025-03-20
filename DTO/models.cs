namespace HLM_Web_APi.DTO
{
    public class models
    {
    }
    public class RegisterUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
    }

    public class LoginUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

}
