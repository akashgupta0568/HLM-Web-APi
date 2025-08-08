namespace HLM_Web_APi.DTO
{
    public class Models
    {
    }
    public class RegisterUserDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public int CreatedByAdminID { get; set; }
        public int HospitalID { get; set; }
        public string Username { get; set; }
    }

    public class LoginUserDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class Hospital
    {
        public int HospitalID { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CreatedBy { get; set; }
    }

    // Models/MenuDto.cs
    public class MenuDto
    {
        public int MenuId { get; set; }
        public string Title { get; set; }
        public string Icon { get; set; }
        public string Link { get; set; }
        public int SortOrder { get; set; }
    }

    // Models/RoleDto.cs
    public class RoleDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    // Models/PermissionDto.cs
    public class PermissionDto
    {
        public int PermissionId { get; set; }
        public string PermissionKey { get; set; }
        public string Description { get; set; }
        public bool IsGranted { get; set; }
    }

    // Models/RolePermissionUpdate.cs
    public class RolePermissionUpdate
    {
        public int RoleId { get; set; }
        public string PermissionKey { get; set; }
        public bool IsGranted { get; set; }
    }

}
