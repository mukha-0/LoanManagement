using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LoanManagement.UI.Bots.LoanOfficer
{
    public class LoanOfficerBotHandler
    {
        private readonly TelegramBotClient _botClient;

        public LoanOfficerBotHandler(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update != null && update.Message != null && update.Message.Type == MessageType.Text)
            {
                await LoanOfficerBotCommands.ProcessCommand(_botClient, update.Message);
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
