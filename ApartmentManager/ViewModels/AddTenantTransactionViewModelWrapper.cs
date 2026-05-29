using ApartmentManager.ViewModels.Tenant;
using ApartmentManager.ViewModels.Transaction;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ApartmentManager.ViewModels
{
    public class AddTenantTransactionViewModelWrapper
    {
        public CreateTransactionViewModel? CreateTransactionViewModel { get; set; }
        public CreateEditTenantViewModel? CreateEditTenantViewModel { get; set; }
        public int Deposit { get; set; }
        public int Advance { get; set; }
        public string RoomName { get; set; } = string.Empty;
    }
}
