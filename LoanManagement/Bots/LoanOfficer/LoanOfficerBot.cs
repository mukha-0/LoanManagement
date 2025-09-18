using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace LoanManagement.UI.Bots.LoanOfficer
{
    public class LoanOfficerBot
    {
        private readonly TelegramBotClient _botClient;
        private readonly LoanOfficerBotHandler _handler;

        public LoanOfficerBot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _handler = new LoanOfficerBotHandler(_botClient);
        }

        public void Start()
        {
            var cts = new CancellationTokenSource();

            _botClient.StartReceiving(
                updateHandler: _handler.HandleUpdateAsync,
                errorHandler: _handler.HandleErrorAsync,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Loan Officer Bot is running...");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
