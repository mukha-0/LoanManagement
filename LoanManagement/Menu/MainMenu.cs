//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using LoanManagement.Bot;
//using LoanManagement.DataAccess.Context;
//using Microsoft.EntityFrameworkCore;
//using Telegram.Bot;
//using Telegram.Bot.Types.Enums;
//using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

//namespace LoanManagement.UI.Menu
//{
//    public class MainMenu
//    {
//        private TelegramBotClient _botClient;

//        public void Start()
//        {
//            // ✅ Use your real token
//            string token = "YOUR_USER_BOT_TOKEN_HERE";

//            _botClient = new TelegramBotClient(token);

//            var me = _botClient.GetMe().Result;
//            Console.WriteLine($"UserBot @{me.Username} is running...");

//            _botClient.StartReceiving(
//                HandleUpdateAsync,
//                HandleErrorAsync
//            );

//            Console.WriteLine("Press any key to stop...");
//            Console.ReadKey();
//        }

//        private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
//        {
//            try
//            {
//                using (var db = new AppDBContext()) // ✅ use your existing context
//                {
//                    if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
//                    {
//                        await UserBotCommands.ProcessCommand(
//                            (TelegramBotClient)botClient,
//                            update.Message,
//                            db
//                        );
//                    }
//                    else if (update.Type == UpdateType.CallbackQuery)
//                    {
//                        await UserBotCommands.ProcessCallback(
//                            (TelegramBotClient)botClient,
//                            update.CallbackQuery!,
//                            db
//                        );
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ Error: {ex.Message}");
//            }
//        }

//        private Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
//        {
//            Console.WriteLine($"⚠️ Bot error: {exception.Message}");
//            return Task.CompletedTask;
//        }
//    }
//}
