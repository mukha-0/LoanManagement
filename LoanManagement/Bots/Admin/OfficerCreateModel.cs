





namespace LoanManagement.Bots.AdminBot
{
    internal class OfficerCreateModel : Service.Services.AllEntries.LoanOfficer.Models.OfficerCreateModel
    {
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
    }
}