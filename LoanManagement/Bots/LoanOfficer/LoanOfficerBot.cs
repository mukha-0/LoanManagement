// LoanManagement/Bots/LoanOfficerBot/LoanOfficerBot.cs
using System;
using System.Threading;
using LoanManagement.Service.Services.AllEntries.Admin;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using Telegram.Bot;

namespace LoanManagement.Bots.LoanOfficerBot
{
    public class LoanOfficerBot
    {
        private readonly TelegramBotClient _botClient;
        private readonly LoanOfficerHandler _handler;

        public LoanOfficerBot(string token, ILoanOfficerService officerService, ILoanService loanService, IAdminService adminService, IRepaymentService repaymentService)
        {
            _botClient = new TelegramBotClient(token);
            _handler = new LoanOfficerHandler(_botClient, officerService, loanService, adminService, repaymentService);
        }

        public void Start()
        {
            var cts = new CancellationTokenSource();
            _botClient.StartReceiving(
                updateHandler: _handler.HandleUpdateAsync,
                errorHandler: _handler.HandleErrorAsync,
                cancellationToken: cts.Token
            );
            Console.WriteLine("LoanOfficer Bot running. Press Enter to stop.");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
