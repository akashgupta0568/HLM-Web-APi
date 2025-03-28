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
        public int DoctorID { get; set; }
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
        public int PatientID { get; set; }
        public int Age { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUserName { get; set; }
        public int CreatedByRoleID { get; set; }
        public string CreatedByHospitalUserName { get; set; }
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
        public string? Name { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }  // ✅ Optional
        public DateTime? DateOfBirth { get; set; }  // ✅ Optional
        public int HospitalID { get; set; }  // ❌ Required (Assuming every appointment must have a hospital)
        public int DoctorID { get; set; }  // ❌ Required
        public int? PatientID { get; set; }  // ✅ Optional
        public DateOnly AppointmentDate { get; set; }  // ❌ Required (Every appointment must have a date)
        public string? AppointmentTime { get; set; }  // ✅ Optional
        public string? Status { get; set; }  // ✅ Optional
        public decimal? AppointmentFee { get; set; }  // ✅ Optional
        public string? PaymentMethod { get; set; }  // ✅ Optional
        public string? TransactionID { get; set; }  // ✅ Optional
        public int? HospitalUserByID { get; set; }  // ✅ Optional
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // ✅ Default to current time
    }




    public class Payment
    {
        public int AppointmentID { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public int HospitalID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }  // Added PaymentDate
        public string PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionID { get; set; }
    }

    public class FilterAppointment
    {
        public int AppointmentID { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string WeekDay { get; set; }
        public string Status { get; set; }
        public int PatientID { get; set; }
        public int DoctorID { get; set; }
        public string AppointmentTime { get; set; }
        public decimal? AppointmentFee { get; set; }
        public DateTime Date { get; set; }
    }

}
