namespace ApartmentManager.ViewModels.Tenant
{
    public class TenantsViewModel
    {
        public int TenantId { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public int Deposit { get; set; }
        public int Balance { get; set; }
        public DateOnly DueDate { get; set; }
        public DateOnly? MoveOutDate { get; set; }
        public int DaysDue { get; set; }
        public bool Paid { get; set; }
        public bool MovingOut { get; set; }
        public bool UseAdvance { get; set; }
    }
}
