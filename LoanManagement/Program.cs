

using LoanManagement.Bot;
using LoanManagement.Bots.AdminBot;
using LoanManagement.DataAccess.Context;
using LoanManagement.UI.Bots.Admin;
using LoanManagement.UI.Bots.LoanOfficer;
using LoanManagement.UI.Bots.User;
using Telegram.Bot.Types.Enums;
using Telegram.Bot;
using LoanManagement.Bots.LoanOfficerBot;
using Telegram.Bot.Types;

namespace LoanManagement;

internal class Program
{
    static async Task Main(string[] args)
    {

        var adminBotToken = "8120631864:AAG6AczJc4SRuwV_MGet8iE9nGp0gLZ3MTQ";
        var userBotToken = "8360128523:AAEp4QApXZYCNBmFb55MV_phCBZo0eHOblo";
        var officerBotToken = "8428221151:AAF98HGV920PtzHzQIyOlMhFmvWbMd8f7EU";
        Console.WriteLine("🚀 Starting Loan System Bots...");
        // ✅ Initialize bots
        var adminBot = new TelegramBotClient(adminBotToken);
        var officerBot = new TelegramBotClient(officerBotToken);
        var userBot = new TelegramBotClient(userBotToken);

        // ✅ Start receiving updates for each bot
        adminBot.StartReceiving(
            (bot, update, token) => HandleAdminUpdate(bot, update, token),
            HandleErrorAsync
        );

        officerBot.StartReceiving(
            (bot, update, token) => HandleOfficerUpdate(bot, update, token),
            HandleErrorAsync
        );

        userBot.StartReceiving(
            (bot, update, token) => HandleUserUpdate(bot, update, token),
            HandleErrorAsync
        );

        Console.WriteLine("✅ All 3 bots are running!");
        Console.WriteLine("Press any key to stop...");
        Console.ReadKey();
    }

    // ================== UPDATE HANDLERS ==================
    private static async Task HandleAdminUpdate(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        using var db = new AppDBContext();

        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
            await AdminBotCommands.ProcessCommand((TelegramBotClient)bot, update.Message, db);

        else if (update.Type == UpdateType.CallbackQuery)
            await AdminBotCommands.ProcessCallback((TelegramBotClient)bot, update.CallbackQuery!, db);
    }

    private static async Task HandleOfficerUpdate(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        using var db = new AppDBContext();

        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
            await LoanOfficerCommands.ProcessCommand((TelegramBotClient)bot, update.Message, db);

        else if (update.Type == UpdateType.CallbackQuery)
            await LoanOfficerCommands.ProcessCallback((TelegramBotClient)bot, update.CallbackQuery!, db);
    }

    private static async Task HandleUserUpdate(ITelegramBotClient bot, Update update, CancellationToken token)
    {
        using var db = new AppDBContext();

        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
            await UserBotCommands.ProcessCommand((TelegramBotClient)bot, update.Message, db);

        else if (update.Type == UpdateType.CallbackQuery)
            await UserBotCommands.ProcessCallback((TelegramBotClient)bot, update.CallbackQuery!, db);
    }

    // ================== ERROR HANDLER ==================
    private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"⚠️ Bot error: {exception.Message}");
        return Task.CompletedTask;
    }
}
