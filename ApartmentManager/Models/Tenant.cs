using System.ComponentModel.DataAnnotations;

namespace ApartmentManager.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly MoveInDate { get; set; }
        public DateOnly? MoveOutDate { get; set; }
        public Status Status { get; set; }
        public int MonthsPaid { get; set; } = 1;
        public int Balance { get; set; }
        public int? RoomId { get; set; }
        public Room? Room { get; set; }
        public ICollection<Transaction>? Transactions { get; set; }
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
    }

    public enum Status
    {
        [Display(Name = "Moving in")]MovingIn = 0,
        [Display(Name = "Settled")]Settled = 1,
        [Display(Name = "Moving out")]MovingOut = 2,
        [Display(Name = "Moved out")]MovedOut = 3
    }

    //<form>
    //    <select asp-for="Status" asp-items="Html.GetEnumSelectList<Status>()">
    //        <option value = "" > Select status</option>
    //    </select>
    //</form>
}
