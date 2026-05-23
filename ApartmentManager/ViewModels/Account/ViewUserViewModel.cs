namespace ApartmentManager.ViewModels.Account
{
    public class ViewUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public int NumberOfRooms { get; set; }
    }
}
