using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Service.Services.AllEntries.LoanOfficer;
using LoanManagement.Service.Services.AllEntries.Users;
using LoanManagement.Service.Services.Loans;
using LoanManagement.Service.Services.Reports;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using LoanManagement.Service.Services.AllEntries.Users.Models;
using LoanManagement.Service.Services.AllEntries.LoanOfficer.Models;
using LoanManagement.Domain.Enums;
using LoanManagement.Service.Services.Repayments;
using LoanManagement.Service.Services.Reports.Models;

namespace LoanManagement.UI.Bots.Admin
{

    public enum AdminService
    {
        None,
        UserService,
        LoanService,
        PaymentService,
        ReportService
    }

    public static class AdminBotState
    {
        public static AdminService CurrentService { get; set; } = AdminService.None;
    }

    public static class AdminBotCommands
    {
        public static LoanService loanService = new LoanService();
        public static UserService userService = new UserService();
        public static LoanOfficerService officerService = new LoanOfficerService();
        public static ReportService reportService = new ReportService();
        public static RepaymentService RepaymentService = new RepaymentService();
        public static async Task ProcessCommand(TelegramBotClient botClient, Message message)
        {
            string text = message.Text.Trim();

            if (text == "/start")
            {
                AdminBotState.CurrentService = AdminService.None;
                await ShowMainMenu(botClient, message.Chat.Id);
                return;
            }

            switch (AdminBotState.CurrentService)
            {
                case AdminService.None:
                    await HandleServiceSelection(botClient, message.Chat.Id, text);
                    break;
                case AdminService.UserService:
                    await HandleUserService(botClient, message.Chat.Id, text);
                    break;
                case AdminService.LoanService:
                    await HandleLoanService(botClient, message.Chat.Id, text);
                    break;
                case AdminService.PaymentService:
                    await HandlePaymentService(botClient, message.Chat.Id, text);
                    break;
                case AdminService.ReportService:
                    await HandleReportService(botClient, message.Chat.Id, text);
                    break;
            }
        }

        #region Main Menu
        private static async Task ShowMainMenu(TelegramBotClient botClient, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "User Service", "Loan Service" },
                new KeyboardButton[] { "Payment Service", "Report Service" }
            })
            { ResizeKeyboard = true };

            await botClient.SendMessage(chatId, "Select a service to manage:", replyMarkup: keyboard);
        }
        #endregion

        #region Service Selection
        private static async Task HandleServiceSelection(TelegramBotClient botClient, long chatId, string text)
        {
            switch (text)
            {
                case "User Service":
                    AdminBotState.CurrentService = AdminService.UserService;
                    await ShowUserServiceMenu(botClient, chatId);
                    break;
                case "Loan Service":
                    AdminBotState.CurrentService = AdminService.LoanService;
                    await ShowLoanServiceMenu(botClient, chatId);
                    break;
                case "Payment Service":
                    AdminBotState.CurrentService = AdminService.PaymentService;
                    await ShowPaymentServiceMenu(botClient, chatId);
                    break;
                case "Report Service":
                    AdminBotState.CurrentService = AdminService.ReportService;
                    await ShowReportServiceMenu(botClient, chatId);
                    break;
                default:
                    await botClient.SendMessage(chatId, "Unknown command. Use /start to see main menu.");
                    break;
            }
        }
        #endregion

        #region User Service
        private static async Task ShowUserServiceMenu(TelegramBotClient botClient, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "Create User", "View Users" },
                new KeyboardButton[] { "Create Loan Officer", "View Loan Officers" },
                new KeyboardButton[] { "Back" }
            })
            { ResizeKeyboard = true };

            await botClient.SendMessage(chatId, "User Service Menu:", replyMarkup: keyboard);
        }

        private static async Task HandleUserService(TelegramBotClient botClient, long chatId, string text)
        {
            switch (text)
            {
                case "Create User":
                    await CreateUser(botClient, chatId);
                    break;
                case "View Users":
                    await ViewUsers(botClient, chatId);
                    break;
                case "Create Loan Officer":
                    await CreateLoanOfficer(botClient, chatId);
                    break;
                case "View Loan Officers":
                    await ViewLoanOfficers(botClient, chatId);
                    break;
                case "Back":
                    AdminBotState.CurrentService = AdminService.None;
                    await ShowMainMenu(botClient, chatId);
                    break;
                default:
                    await botClient.SendMessage(chatId, "Unknown command in User Service. Use 'Back' to return.");
                    break;
            }
        }

        private static async Task CreateLoanOfficer(TelegramBotClient botClient, long chatId)
        {

        }

        private static async Task CreateUser(TelegramBotClient botClient, long chatId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Loan Service
        private static async Task ShowLoanServiceMenu(TelegramBotClient botClient, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "View All Loans", "View Pending Loans" },
                new KeyboardButton[] { "Back" }
            })
            { ResizeKeyboard = true };

            await botClient.SendMessage(chatId, "Loan Service Menu:", replyMarkup: keyboard);
        }

        private static async Task HandleLoanService(TelegramBotClient botClient, long chatId, string text)
        {
            switch (text)
            {
                case "View All Loans":
                    await ViewAllLoans(botClient, chatId);
                    break;
                case "View Pending Loans":
                    await ViewPendingLoans(botClient, chatId);
                    break;
                case "Back":
                    AdminBotState.CurrentService = AdminService.None;
                    await ShowMainMenu(botClient, chatId);
                    break;
                default:
                    await botClient.SendMessage(chatId, "Unknown command in Loan Service. Use 'Back' to return.");
                    break;
            }
        }
        #endregion

        #region Payment Service
        private static async Task ShowPaymentServiceMenu(TelegramBotClient botClient, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "View All Payments" },
                new KeyboardButton[] { "Back" }
            })
            { ResizeKeyboard = true };

            await botClient.SendMessage(chatId, "Payment Service Menu:", replyMarkup: keyboard);
        }

        private static async Task HandlePaymentService(TelegramBotClient botClient, long chatId, string text)
        {
            switch (text)
            {
                case "View All Payments":
                    await ViewAllPayments(botClient, chatId);
                    break;
                case "Back":
                    AdminBotState.CurrentService = AdminService.None;
                    await ShowMainMenu(botClient, chatId);
                    break;
                default:
                    await botClient.SendMessage(chatId, "Unknown command in Payment Service. Use 'Back' to return.");
                    break;
            }
        }
        #endregion

        #region Report Service
        private static async Task ShowReportServiceMenu(TelegramBotClient botClient, long chatId)
        {
            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new KeyboardButton[] { "Generate Report" },
                new KeyboardButton[] { "Back" }
            })
            { ResizeKeyboard = true };

            await botClient.SendMessage(chatId, "Report Service Menu:", replyMarkup: keyboard);
        }

        private static async Task HandleReportService(TelegramBotClient botClient, long chatId, string text)
        {
            switch (text)
            {
                case "Generate Report":
                    await GenerateReport(botClient, chatId);
                    break;
                case "Back":
                    AdminBotState.CurrentService = AdminService.None;
                    await ShowMainMenu(botClient, chatId);
                    break;
                default:
                    await botClient.SendMessage(chatId, "Unknown command in Report Service. Use 'Back' to return.");
                    break;
            }
        }

        private static async Task<ReportsCreateModel> GenerateReport(TelegramBotClient botClient, long chatId)
        {
            var model = new ReportsCreateModel
            {
                Loans = (IEnumerable<Domain.Entities.Loan>)await reportService.GetLoansByStatusAsync(LoanStatus.Approved),
                TopBorrowers = (IEnumerable<Domain.Entities.Loan>)await reportService.GetTopBorrowersAsync(5),
                TotalApprovedLoans = await reportService.GetTotalApprovedLoansAsync(),
                TotalLoanAmount = ((IEnumerable<Domain.Entities.Loan>)await reportService.GetLoansByStatusAsync(LoanStatus.Approved)).Sum(l => l.Amount),
                TotalLoans = ((IEnumerable<Domain.Entities.Loan>)await reportService.GetLoansByStatusAsync(LoanStatus.Approved)).Count(),
                ReportGeneratedOn = DateTime.UtcNow,
                UserId = (int)chatId,
                User = await userService.GetUserByIdAsync((int)chatId)
            };
            return model;

        }
        #endregion

        #region Service Actions (Example placeholders)
        private static async Task CreateUser(TelegramBotClient botClient, long chatId, UserRegisterModel userRegisterModel)
        {
            await botClient.SendMessage(chatId, "Create User function called.");
            await userService.RegisterUserAsync(new UserRegisterModel
            {
                UserName = userRegisterModel.UserName,
                FirstName = userRegisterModel.FirstName,
                LastName = userRegisterModel.LastName,
                PasswordHash = userRegisterModel.PasswordHash,
                Role = Role.User
            });
        }

        private static async Task ViewUsers(TelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(chatId, "View Users function called.");
            await userService.GetAllUsersAsync();
        }

        private static async Task CreateLoanOfficer(TelegramBotClient botClient, long chatId, OfficerCreateModel officerCreateModel)
        {
            await botClient.SendMessage(chatId, "Create Loan Officer function called.");
            var model = new OfficerCreateModel
            {
                FirstName = officerCreateModel.FirstName,
                LastName = officerCreateModel.LastName,
                Email = officerCreateModel.Email,
                PhoneNumber = officerCreateModel.PhoneNumber,
                OfficerId = officerCreateModel.OfficerId
            };
            await officerService.CreateNewLoanOfficer(model);
        }

        private static async Task ViewLoanOfficers(TelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(chatId, "View Loan Officers function called.");
            await officerService.GetAllOfficers();
        }

        private static async Task ViewAllLoans(TelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(chatId, "View All Loans function called.");
            await loanService.GetAllLoansAsync();
        }

        private static async Task ViewPendingLoans(TelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(chatId, "View Pending Loans function called.");
            await loanService.GetPendingLoans();
        }

        private static async Task ViewAllPayments(TelegramBotClient botClient, long chatId)
        {
            await botClient.SendMessage(chatId, "View All Payments function called.");
            await RepaymentService.GetAllRepayments();
        }

        private static async Task GenerateReport(TelegramBotClient botClient, long chatId, ReportsCreateModel model)
        {
            await botClient.SendMessage(chatId, "Generate Report function called.");
            var reportModel = await GenerateReport(botClient, chatId);
            await reportService.GenerateReport(botClient, chatId, reportModel);
        }
        #endregion
    }
}
