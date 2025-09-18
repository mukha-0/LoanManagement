using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace LoanManagement.UI.Bots.User
{
    public class UserBot
    {
        private readonly TelegramBotClient _botClient;
        private readonly UserBotHandler _handler;

        public UserBot(string token)
        {
            _botClient = new TelegramBotClient(token);
            _handler = new UserBotHandler(_botClient);
        }

        public void Start()
        {
            var cts = new CancellationTokenSource();

            _botClient.StartReceiving(
                updateHandler: _handler.HandleUpdateAsync,
                errorHandler: _handler.HandleErrorAsync,
                cancellationToken: cts.Token
            );

            Console.WriteLine("User Bot is running...");
            Console.ReadLine();
            cts.Cancel();
        }
    }
}
