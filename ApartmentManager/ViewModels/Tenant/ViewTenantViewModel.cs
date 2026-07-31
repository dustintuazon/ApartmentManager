using ApartmentManager.ViewModels.Transaction;

namespace ApartmentManager.ViewModels.Tenant
{
    public class ViewTenantViewModel
    {
        public int TenantId { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;
        public DateOnly MoveInDate { get; set; }
        public DateOnly? MoveOutDate { get; set; }
        public int Monthly { get; set; }
        public int Balance { get; set; }
        public int Deposit { get; set; }
    }
}
