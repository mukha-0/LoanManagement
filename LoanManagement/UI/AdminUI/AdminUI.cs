using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace LoanManagement.UI.AdminUI
{
    public class AdminUI
    {
         void ADMINUI()
        {
            Console.WriteLine($"Admin UI");
            Console.WriteLine($"1. Manage Users");
            Console.WriteLine($"2. Manage Customers");
            Console.WriteLine($"3. Manage Loans");
            Console.WriteLine($"4. Manage Repayments");
            Console.WriteLine($"5. Manage Transactions");
            Console.WriteLine($"6. Exit");
            Console.Write($"Enter your choice: ");
            var choice = Console.ReadLine();
            if ( choice != null )
            {
                switch (choice)
                {
                    case "1":

                        break;
                    case "2":
                        Console.WriteLine($"Manage Customers");
                        break;
                    case "3":
                        Console.WriteLine($"Manage Loans");
                        break;
                    case "4":
                        Console.WriteLine($"Manage Repayments");
                        break;
                    case "5":
                        Console.WriteLine($"Manage Transactions");
                        break;
                    case "6":
                        Console.WriteLine($"Exit");
                        break;
                    default:
                        Console.WriteLine($"Invalid choice");
                        break;
                }
            }
            else
            {
                Console.WriteLine($"Invalid choice");
            }
        }
    }
}
