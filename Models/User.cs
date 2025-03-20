namespace HLM_Web_APi.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string PasswordHash { get; set; }
        public int RoleID { get; set; }

        public string RoleName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class HospitalDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string LicenseNumber { get; set; }
        public int CreatedBy { get; set; }
    }

    public class DoctorDto
    {
        public string Name { get; set; }
        public string Specialization { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int HospitalID { get; set; }
        public int ConsultationFee { get; set; }
    }

    public class PatientDto
    {
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int HospitalID { get; set; }
    }

    public class AppointmentDto
    {
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string AppointmentTime { get; set; } // Time in HH:mm:ss format
        public string Status { get; set; }
        public decimal ConsultationFee { get; set; }
    }

    public class AppointmentWithPatient
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int HospitalID { get; set; }
        public int DoctorID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Status { get; set; }
        public decimal AppointmentFee { get; set; }
    }

}
