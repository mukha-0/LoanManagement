using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace LoanManagement.UI.Bots.Admin
{
    public class AdminBot
    {
        private readonly TelegramBotClient _botClient;
        private readonly AdminBotHandler _handler;

        public AdminBot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _handler = new AdminBotHandler(_botClient);
        }

        public void Start()
        {
            var cts = new CancellationTokenSource();

            _botClient.StartReceiving(
                updateHandler: _handler.HandleUpdateAsync,
                errorHandler: _handler.HandleErrorAsync,
                cancellationToken: cts.Token
            );

            Console.WriteLine("Admin Bot is running...");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
