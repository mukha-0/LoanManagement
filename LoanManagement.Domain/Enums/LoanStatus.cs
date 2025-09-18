using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.Domain.Enums
{
    public enum LoanStatus
    {
        Pending,
        Approved,
        Rejected,
        Overdue,
        Suspended,
        Closed
    }
}
