using Telegram.Bot;
using LoanManagement.Bots.AdminBot;
using LoanManagement.Bots.LoanOfficerBot;
using LoanManagement.Bots.UserBot;
using LoanManagement.Service.Services.AllEntries.Admin;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.AllEntries;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.Reports;

namespace LoanManagement.Bots
{

    public static class BotRunner
    {
        private readonly UserService _userService;
        private readonly LoanOfficerService _loanOfficerService;
        private readonly AdminService _adminService;

        public BotRunner()
        {
            _userService = new UserService();
            _loanOfficerService = new LoanOfficerService();
            _adminService = new AdminService(_adminService);
        }

        private const string AdminToken = "8120631864:AAG6AczJc4SRuwV_MGet8iE9nGp0gLZ3MTQ";
        private const string OfficerToken = "8428221151:AAF98HGV920PtzHzQIyOlMhFmvWbMd8f7EU";
        private const string UserToken = "8360128523:AAEp4QApXZYCNBmFb55MV_phCBZo0eHOblo";
        public static void RunBot()
        {
            RunAdminBot();
            RunLoanOfficerBot();
            RunUserBot();

            Console.WriteLine("All bots are running...");
            Console.ReadLine();
        }

        private static void RunAdminBot()
        {
            var adminBotClient = new TelegramBotClient(AdminToken);

            var adminCommands = new AdminBotCommands(
                new AdminService(),
                new UserService(),
                new LoanService(),
                new RepaymentService(),
                new ReportService(),
                new LoanOfficerService()
            );

            var cts = new CancellationTokenSource();

            adminBotClient.StartReceiving(
                updateHandler: adminCommands.HandleUpdateAsync,
                errorHandler: adminCommands.HandleErrorAsync,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Admin Bot started.");
        }

        private static void RunLoanOfficerBot()
        {
            var officerBotClient = new TelegramBotClient(OfficerToken);

            var officerCommands = new LoanOfficerBotCommands(
                new LoanService(),
                new LoanOfficerService(),
                new RepaymentService()
            );

            var cts = new CancellationTokenSource();

            officerBotClient.StartReceiving(
                updateHandler: officerCommands.HandleUpdateAsync,
                errorHandler: officerCommands.HandleErrorAsync,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Loan Officer Bot started.");
        }

        private static void RunUserBot()
        {
            var userBotClient = new TelegramBotClient(UserToken);

            var userCommands = new UserBotCommands(
                new UserService(),
                new LoanService(),
                new RepaymentService(),
                new LoanOfficerService()
            );

            var cts = new CancellationTokenSource();

            userBotClient.StartReceiving(
                updateHandler: userCommands.HandleUpdateAsync,
                errorHandler: userCommands.HandleErrorAsync,
                cancellationToken: cts.Token
            );

            Console.WriteLine("User Bot started.");
        }
    }
}
