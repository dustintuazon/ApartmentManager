using System.ComponentModel.DataAnnotations;

namespace ApartmentManager.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateOnly MoveInDate { get; set; }
        public DateOnly? MoveOutDate { get; set; }
        public int MonthsPaid { get; set; } = 0;
        public int Balance { get; set; } = 0;
        public int Deposit { get; set; } = 0;
        public int RoomId { get; set; }
        public Room? Room { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
    }

    //<form>
    //    <select asp-for="Status" asp-items="Html.GetEnumSelectList<Status>()">
    //        <option value = "" > Select status</option>
    //    </select>
    //</form>
}
