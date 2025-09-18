using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LoanManagement.UI.Bots.Admin
{
    public class AdminBotHandler
    {
        private readonly TelegramBotClient _botClient;

        public AdminBotHandler(TelegramBotClient botClient)
        {
            _botClient = botClient;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Telegram.Bot.Types.Update update, CancellationToken cancellationToken)
        {
            // Ensure Message is not null  
            if (update.Message is not null)
            {
                // Ensure the message type is Text  
                if (update.Message.Type == MessageType.Text)
                {
                    // Call your command processor  
                    await AdminBotCommands.ProcessCommand(_botClient, update.Message);
                }
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }

        internal async Task HandleErrorAsync(TelegramBotClient botClient, ITelegramBotClient exception, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
