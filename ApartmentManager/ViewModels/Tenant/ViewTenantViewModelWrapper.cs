using ApartmentManager.ViewModels.Transaction;

namespace ApartmentManager.ViewModels.Tenant
{
    public class ViewTenantViewModelWrapper
    {
        public ViewTenantViewModel ViewTenantViewModel { get; set; } = new();
        public MoveOutDateViewModel MoveOutDateViewModel { get; set; } = new();
        public PaginatedList<TransactionsViewModel>? Transactions { get; set; }
    }
}
