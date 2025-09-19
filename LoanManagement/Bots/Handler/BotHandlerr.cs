using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace LoanManagement.UI.Bots.Handler
{
    public class BotHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly Func<ITelegramBotClient, Update, Task> _updateCallback;

        public BotHandler(ITelegramBotClient botClient, Func<ITelegramBotClient, Update, Task> updateCallback)
        {
            _botClient = botClient;
            _updateCallback = updateCallback;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message?.Text != null)
                {
                    await _updateCallback(botClient, update);
                }
                else if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery?.Data != null)
                {
                    await _updateCallback(botClient, update);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling update: {ex.Message}");
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
    }
}
