// LoanManagement/Bots/AdminBot/AdminBotCommands.cs
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using LoanManagement.Service;
using LoanManagement.Domain;
using LoanManagement.Bots;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.Reports;
using LoanManagement.Service.Services.AllEntries.Admin;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Domain.Enums;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.AllEntries.LoanOfficer.Models;
using LoanManagement.Service.Services.AllEntries.Users.Models;
using LoanManagement.DataAccess.Context;
using Microsoft.EntityFrameworkCore; // for NotificationHub

namespace LoanManagement.Bots.AdminBot
{
    public class AdminBotCommands
    {
        private readonly TelegramBotClient _bot;
        private readonly IUserService _userService;
        private readonly ILoanService _loanService;
        private readonly IRepaymentService _repayment_service;
        private readonly IRepaymentService _repaymentService;
        private readonly IReportService _reportService;
        private readonly IAdminService _adminService;
        private readonly ILoanOfficerService _loanOfficerService;

        // per-chat state
        private readonly ConcurrentDictionary<long, string> _chatState = new();
        private readonly ConcurrentDictionary<long, object> _chatTemp = new();
        public AdminBotCommands(TelegramBotClient bot,
        IUserService userService,
        ILoanService loanService,
        IRepaymentService repaymentService,
        IReportService reportService,
        IAdminService adminService,
            ILoanOfficerService loanOfficerService)
        {
            _bot = bot;
            _userService = userService;
            _loanService = loanService;
            _repayment_service = repaymentService; // note local name spelled differently? ensure consistent
            _repaymentService = repaymentService;
            _reportService = reportService;
            _adminService = adminService;
            _loanOfficerService = loanOfficerService;
        }

        // Public entry for text messages
        public async Task ProcessMessage(Message message)
        {
            var chatId = message.Chat.Id;
            var text = message.Text?.Trim() ?? "";

            // Global reset
            if (text.Equals("/start", StringComparison.OrdinalIgnoreCase))
            {
                _chatState[chatId] = "MAIN";
                await ShowMainMenu(chatId);
                return;
            }

            // If awaiting a next input for the flow, handle state-first
            if (_chatState.TryGetValue(chatId, out var state) && state != "MAIN")
            {
                await HandleStateInput(chatId, state, text);
                return;
            }

            // If main, interpret commands (text or button text)
            if (text.Equals("User Service", StringComparison.OrdinalIgnoreCase) || text.Equals("user service", StringComparison.OrdinalIgnoreCase))
            {
                _chatState[chatId] = "USER_MENU";
                await ShowUserMenu(chatId);
                return;
            }
            if (text.Equals("Loan Service", StringComparison.OrdinalIgnoreCase) || text.Equals("loan service", StringComparison.OrdinalIgnoreCase))
            {
                _chatState[chatId] = "LOAN_MENU";
                await ShowLoanMenu(chatId);
                return;
            }
            if (text.Equals("Payment Service", StringComparison.OrdinalIgnoreCase) || text.Equals("payment service", StringComparison.OrdinalIgnoreCase))
            {
                _chatState[chatId] = "PAYMENT_MENU";
                await ShowPaymentMenu(chatId);
                return;
            }
            if (text.Equals("Report Service", StringComparison.OrdinalIgnoreCase) || text.Equals("report service", StringComparison.OrdinalIgnoreCase))
            {
                _chatState[chatId] = "REPORT_MENU";
                await ShowReportMenu(chatId);
                return;
            }

            // fallback
            await _bot.SendMessage(chatId, "Unknown command. Send /start to open menu.");
        }

        // Public entry for callback queries (inline button clicks)
        public static async Task ProcessCallback(TelegramBotClient bot, CallbackQuery callback, AppDBContext db)
        {
            var chatId = callback.Message!.Chat.Id;
            var data = callback.Data ?? "";
            var parts = data.Split(':', 2);
            var action = parts[0];
            var param = parts.Length > 1 ? parts[1] : null;

            try
            {
                switch (action)
                {
                    case "view_users":
                        var users = await db.Users.ToListAsync();
                        if (!users.Any()) { await bot.SendMessage(chatId, "No users found."); break; }
                        foreach (var user in users)
                            await bot.SendMessage(chatId, $"User #{user.UserId}: {user.UserName}");
                        break;

                    case "view_officers":
                        var officers = await db.LoanOfficers.ToListAsync();
                        if (!officers.Any()) { await bot.SendMessage(chatId, "No officers found."); break; }
                        foreach (var officer in officers)
                            await bot.SendMessage(chatId, $"Officer #{officer.OfficerId}: {officer.FirstName}: {officer.LastName}");
                        break;

                    case "delete_user":
                        if (int.TryParse(param, out var userId))
                        {
                            var user = await db.Users.FindAsync(userId);
                            if (user == null) { await bot.SendMessage(chatId, "User not found."); break; }
                            db.Users.Remove(user);
                            await db.SaveChangesAsync();
                            await bot.SendMessage(chatId, $"User {userId} deleted.");
                        }
                        break;

                    case "delete_officer":
                        if (int.TryParse(param, out var officerId))
                        {
                            var officer = await db.LoanOfficers.FindAsync(officerId);
                            if (officer == null) { await bot.SendMessage(chatId, "Officer not found."); break; }
                            db.LoanOfficers.Remove(officer);
                            await db.SaveChangesAsync();
                            await bot.SendMessage(chatId, $"Officer {officerId} deleted.");
                        }
                        break;

                    default:
                        await bot.SendMessage(chatId, "Unknown action.");
                        break;
                }
            }
            catch (Exception ex)
            {
                await bot.SendMessage(chatId, $"Error: {ex.Message}");
            }

            try { await bot.AnswerCallbackQuery(callback.Id); } catch { }
        }
        public static async Task ProcessCommand(TelegramBotClient bot, Message message, AppDBContext db)
        {
            var chatId = message.Chat.Id;
            var text = message.Text!.Trim();

            if (text == "/start")
            {
                var keyboard = new InlineKeyboardMarkup(new[]
                {
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Apply for Loan", "apply_loan"),
                        InlineKeyboardButton.WithCallbackData("My Active Loans", "active_loans")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Loan History", "loan_history"),
                        InlineKeyboardButton.WithCallbackData("Make Repayment", "repayment")
                    },
                    new []
                    {
                        InlineKeyboardButton.WithCallbackData("Profile Settings", "profile"),
                    }
                });

                await bot.SendMessage(chatId, "Welcome to the Loan System User Bot!\nChoose an option:", replyMarkup: keyboard);
            }
            else
            {
                await bot.SendMessage(chatId, "Unknown command. Use /start to open the menu.");
            }
        }


        #region UI Menus

        private async Task ShowMainMenu(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("User Service", "user_service") },
                new[] { InlineKeyboardButton.WithCallbackData("Loan Service", "loan_service") },
                new[] { InlineKeyboardButton.WithCallbackData("Payment Service", "payment_service") },
                new[] { InlineKeyboardButton.WithCallbackData("Report Service", "report_service") }
            });

            await _bot.SendMessage(chatId, "Admin main menu:", replyMarkup: keyboard);
        }

        private async Task ShowUserMenu(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Create User", "user_create"), InlineKeyboardButton.WithCallbackData("View Users", "user_view") },
                new[] { InlineKeyboardButton.WithCallbackData("Create Loan Officer", "officer_create"), InlineKeyboardButton.WithCallbackData("View Loan Officers", "officer_view") },
                new[] { InlineKeyboardButton.WithCallbackData("Delete User", "user_delete"), InlineKeyboardButton.WithCallbackData("Delete Officer", "officer_delete") },
                new[] { InlineKeyboardButton.WithCallbackData("Add New Admin", "add_admin"), InlineKeyboardButton.WithCallbackData("Give Discount to Top", "give_discount") },
                new[] { InlineKeyboardButton.WithCallbackData("Back", "back_main") }
            });
            await _bot.SendMessage(chatId, "User Service Menu:", replyMarkup: keyboard);
        }

        private async Task ShowLoanMenu(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("View All Loans", "loan_view_all"), InlineKeyboardButton.WithCallbackData("View Pending Loans", "loan_view_pending") },
                new[] { InlineKeyboardButton.WithCallbackData("Back", "back_main") }
            });
            await _bot.SendMessage(chatId, "Loan Service Menu:", replyMarkup: keyboard);
        }

        private async Task ShowPaymentMenu(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("View All Payments", "payments_view_all") },
                new[] { InlineKeyboardButton.WithCallbackData("Back", "back_main") }
            });
            await _bot.SendMessage(chatId, "Payment Service Menu:", replyMarkup: keyboard);
        }

        private async Task ShowReportMenu(long chatId)
        {
            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("Total unconfirmed loans", "report_total_unconfirmed") },
                new[] { InlineKeyboardButton.WithCallbackData("Total repaid amount", "report_total_repaid") },
                new[] { InlineKeyboardButton.WithCallbackData("Top borrowers (N)", "report_top_borrowers") },
                new[] { InlineKeyboardButton.WithCallbackData("Loans by status", "report_by_status") },
                new[] { InlineKeyboardButton.WithCallbackData("Loans by user id", "report_by_user") },
                new[] { InlineKeyboardButton.WithCallbackData("Back", "back_main") }
            });
            await _bot.SendMessage(chatId, "Report Service Menu:", replyMarkup: keyboard);
        }

        #endregion

        #region Action Implementations (calls to services)

        private async Task Action_ViewAllUsers(long chatId)
        {
            try
            {
                var users = await _adminService.GetAllUsersAsync();
                if (users == null || !users.Any())
                {
                    await _bot.SendMessage(chatId, "No users found.");
                    return;
                }
                var sb = new StringBuilder();
                foreach (var u in users) sb.AppendLine(u.ToString());
                await _bot.SendMessage(chatId, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Action_ViewAllUsers error: {ex}");
                await _bot.SendMessage(chatId, "Error fetching users.");
            }
        }

        private async Task Action_ViewAllOfficers(long chatId)
        {
            try
            {
                var officers = await _adminService.GetAllOfficersAsync();
                if (officers == null || !officers.Any())
                {
                    await _bot.SendMessage(chatId, "No officers found.");
                    return;
                }
                var sb = new StringBuilder();
                foreach (var o in officers) sb.AppendLine(o.ToString());
                await _bot.SendMessage(chatId, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Action_ViewAllOfficers error: {ex}");
                await _bot.SendMessage(chatId, "Error fetching officers.");
            }
        }

        private async Task Action_ViewAllLoans(long chatId)
        {
            try
            {
                var loans = await _loanService.GetAllLoansAsync();
                if (loans == null || !loans.Any())
                {
                    await _bot.SendMessage(chatId, "No loans.");
                    return;
                }
                var sb = new StringBuilder();
                foreach (var l in loans) sb.AppendLine($"ID:{l.Id} User:{l.CustomerId} Amount:{l.Amount} Status:{l.Status}");
                await _bot.SendMessage(chatId, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Action_ViewAllLoans error: {ex}");
                await _bot.SendMessage(chatId, "Error fetching loans.");
            }
        }

        private async Task Action_ViewPendingLoans(long chatId)
        {
            try
            {
                // use GetPendingLoans from list
                var pending = await _loanService.GetPendingLoans();
                if (pending == null || pending.Count == 0)
                {
                    await _bot.SendMessage(chatId, "No pending loans.");
                    return;
                }
                var sb = new StringBuilder();
                foreach (var p in pending) sb.AppendLine($"ID:{p.LoanId} User:{p.CustomerId} Amount:{p.Amount} Status:{p.Status} - Approve? / Send to Officer? Use inline button.");
                // include inline buttons for each loan to approve/reject/send
                foreach (var p in pending)
                {
                    var kb = new InlineKeyboardMarkup(new[]
                    {
                        new [] { InlineKeyboardButton.WithCallbackData("Approve Direct", $"loan_approve_direct:{p.LoanId}"),
                                InlineKeyboardButton.WithCallbackData("Reject Direct", $"loan_reject_direct:{p.LoanId}") },
                        new [] { InlineKeyboardButton.WithCallbackData("Send to Officer for approval", $"loan_send_to_officer:{p.LoanId}") }
                    });
                    await _bot.SendMessage(chatId, $"Loan ID:{p.LoanId} User:{p.CustomerId} Amount:{p.Amount} Status:{p.Status}", replyMarkup: kb);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Action_ViewPendingLoans error: {ex}");
                await _bot.SendMessage(chatId, "Error fetching pending loans.");
            }
        }

        private async Task Action_ViewAllPayments(long chatId)
        {
            try
            {
                var repayments = await _repaymentService.GetAllRepayments();
                if (repayments == null || !repayments.Any())
                {
                    await _bot.SendMessage(chatId, "No repayments.");
                    return;
                }
                var sb = new StringBuilder();
                foreach (var r in repayments) sb.AppendLine(r.ToString());
                await _bot.SendMessage(chatId, sb.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Action_ViewAllPayments error: {ex}");
                await _bot.SendMessage(chatId, "Error fetching repayments.");
            }
        }

        #endregion

        #region State handling for multi-step flows

        private async Task HandleStateInput(long chatId, string state, string input)
        {
            switch (state)
            {
                case "CREATE_USER_USERNAME":
                    _chatTemp[chatId] = input;
                    _chatState[chatId] = "CREATE_USER_PASSWORD";
                    await _bot.SendMessage(chatId, "Enter password for new user:");
                    break;

                case "CREATE_USER_PASSWORD":
                    {
                        var username = _chatTemp[chatId]?.ToString();
                        var password = input;
                        try
                        {
                            var model = new UserRegisterModel
                            {
                                UserName = username,
                                PasswordHash = password
                                // add extra fields if needed
                            };
                            await _userService.RegisterUserAsync(model);
                            await _bot.SendMessage(chatId, $"User {username} created.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"CREATE_USER error: {ex}");
                            await _bot.SendMessage(chatId, "Error creating user.");
                        }
                        _chatState[chatId] = "MAIN";
                        break;
                    }

                case "CREATE_OFFICER_USERNAME":
                    _chatTemp[chatId] = input;
                    _chatState[chatId] = "CREATE_OFFICER_PASSWORD";
                    await _bot.SendMessage(chatId, "Enter password for new officer:");
                    break;

                case "CREATE_OFFICER_PASSWORD":
                    {
                        var username = _chatTemp[chatId]?.ToString();
                        var password = input;
                        try
                        {
                            var officer = new OfficerCreateModel
                            {
                                Username = username,
                                Password = password
                            };
                            await _loanOfficerService.ApplyForLoanOfficer(officer); // uses ApplyForLoanOfficer to create
                            await _bot.SendMessage(chatId, $"Officer {username} created.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"CREATE_OFFICER error: {ex}");
                            await _bot.SendMessage(chatId, "Error creating officer.");
                        }
                        _chatState[chatId] = "MAIN";
                        break;
                    }

                case "DELETE_USER_ID":
                    if (int.TryParse(input, out var uid))
                    {
                        try
                        {
                            await _adminService.DeleteUserAsync(uid);
                            await _bot.SendMessage(chatId, $"User {uid} deleted.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"DELETE_USER error: {ex}");
                            await _bot.SendMessage(chatId, "Error deleting user.");
                        }
                    }
                    else await _bot.SendMessage(chatId, "Invalid user id.");
                    _chatState[chatId] = "MAIN";
                    break;

                case "DELETE_OFFICER_ID":
                    if (int.TryParse(input, out var oid))
                    {
                        try
                        {
                            await _adminService.DeleteOfficerAsync(oid);
                            await _bot.SendMessage(chatId, $"Officer {oid} deleted.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"DELETE_OFFICER error: {ex}");
                            await _bot.SendMessage(chatId, "Error deleting officer.");
                        }
                    }
                    else await _bot.SendMessage(chatId, "Invalid officer id.");
                    _chatState[chatId] = "MAIN";
                    break;

                case "ADD_ADMIN_USERNAME":
                    _chatTemp[chatId] = input;
                    _chatState[chatId] = "ADD_ADMIN_PASSWORD";
                    await _bot.SendMessage(chatId, "Enter password for new admin:");
                    break;

                case "ADD_ADMIN_PASSWORD":
                    {
                        var username = _chatTemp[chatId]?.ToString();
                        var password = input;
                        try
                        {
                            var adm = new Adminn { AdminUserName = username, AdminPassword = password };
                            await _adminService.AddNewAdmin(adm);
                            await _bot.SendMessage(chatId, $"Admin {username} added.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"ADD_ADMIN error: {ex}");
                            await _bot.SendMessage(chatId, "Error adding admin.");
                        }
                        _chatState[chatId] = "MAIN";
                        break;
                    }

                case "GIVE_DISCOUNT_TOPN":
                    if (int.TryParse(input, out var topN))
                    {
                        try
                        {
                            await _adminService.GiveDiscountToTopBorrower(topN);
                            await _bot.SendMessage(chatId, $"Discount given to top {topN} borrowers.");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"GIVE_DISCOUNT error: {ex}");
                            await _bot.SendMessage(chatId, "Error giving discount.");
                        }
                    }
                    else await _bot.SendMessage(chatId, "Invalid number.");
                    _chatState[chatId] = "MAIN";
                    break;

                // Reports multi-step
                case "REPORT_TOP_N":
                    if (int.TryParse(input, out var n))
                    {
                        var top = await _reportService.GetTopBorrowersAsync(n);
                        await _bot.SendMessage(chatId, $"Top borrowers:\n{top}");
                    }
                    else await _bot.SendMessage(chatId, "Invalid number.");
                    _chatState[chatId] = "MAIN";
                    break;

                case "REPORT_STATUS":
                    // parse status string to LoanStatus enum - adjust names to your enum
                    if (Enum.TryParse(typeof(LoanStatus), input, true, out var parsed))
                    {
                        var res = await _reportService.GetLoansByStatusAsync((LoanStatus)parsed);
                        await _bot.SendMessage(chatId, $"Loans by status:\n{res}");
                    }
                    else await _bot.SendMessage(chatId, "Invalid status.");
                    _chatState[chatId] = "MAIN";
                    break;

                case "REPORT_BY_USER":
                    if (int.TryParse(input, out var uidR))
                    {
                        var r = await _reportService.GetByUserIdAsync(uidR);
                        await _bot.SendMessage(chatId, $"Report for user {uidR}:\n{r}");
                    }
                    else await _bot.SendMessage(chatId, "Invalid user id.");
                    _chatState[chatId] = "MAIN";
                    break;

                default:
                    _chatState[chatId] = "MAIN";
                    await _bot.SendMessage(chatId, "State cleared. Send /start.");
                    break;
            }
        }

        #endregion
    }
}
