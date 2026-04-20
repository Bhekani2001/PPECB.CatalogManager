using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.Core.Enums
{
    public enum TransactionType
    {
        Receipt = 1,    // Stock in
        Issue = 2,      // Stock out
        Transfer = 3,   // Move between warehouses
        Adjustment = 4, // Stock count adjustment
        Return = 5      // Customer return
    }
}
