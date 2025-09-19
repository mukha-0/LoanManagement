// LoanManagement/Bots/UserBot/UserBot.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace LoanManagement.Bots.UserBot
{
    public class UserBot
    {
        private readonly TelegramBotClient _bot;
        private readonly UserBotHandler _handler;

        public UserBot(string token, IUserService userService, ILoanService loanService, IRepaymentService repaymentService)
        {
            _bot = new TelegramBotClient(token);
            _handler = new UserBotHandler(_bot, userService, loanService, repaymentService);
        }

        public void Start()
        {
            var cts = new CancellationTokenSource();
            _bot.StartReceiving(_handler.HandleUpdateAsync, _handler.HandleErrorAsync, cancellationToken: cts.Token);
            Console.WriteLine("User Bot running. Press Enter to stop.");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
