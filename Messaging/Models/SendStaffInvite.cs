namespace MITCRMS.Implementation.Messaging.Models
{
    public class SendStaffInvite : Base
    {
        public string Title { get; set; }
        public string SignupLink { get; set; }
        public string Roles { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
