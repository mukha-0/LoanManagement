// LoanManagement/Bots/AdminBot/AdminBot.cs
using System;
using System.Threading;
using LoanManagement.Service.Services.AllEntries.Admin;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.Reports;
using LoanManagement.UI.Bots.Admin;
using Telegram.Bot;

namespace LoanManagement.Bots.AdminBot
{
    public class AdminBot
    {
        private readonly TelegramBotClient _botClient;
        private readonly AdminBotHandler _handler;

        public AdminBot(string token,
            IUserService userService,
            ILoanService loanService,
            IRepaymentService repaymentService,
            IReportService reportService,
            IAdminService adminService,
            ILoanOfficerService loanOfficerService)
        {
            _botClient = new TelegramBotClient(token);
            _handler = new AdminBotHandler(_botClient, userService, loanService, repaymentService, reportService, adminService, loanOfficerService);
        }

        public void Start()
        {
            var cts = new CancellationTokenSource();
            _botClient.StartReceiving(
                updateHandler: _handler.HandleUpdateAsync,
                errorHandler: _handler.HandleErrorAsync,
                cancellationToken: cts.Token
            );
            Console.WriteLine("Admin Bot running. Press Enter to stop.");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
