namespace MITCRMS.Models.DTOs.Users
{
    public class CompleteStaffSignupRequestModel
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
