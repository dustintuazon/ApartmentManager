namespace ApartmentManager.ViewModels.Room
{
    public class ViewRoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public int Monthly { get; set; }
        public int MonthsDeposit { get; set; }
        public int MonthsAdvance { get; set; }
        public string? OccupiedBy { get; set; } = string.Empty;
    }
}
