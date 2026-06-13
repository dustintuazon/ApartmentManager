using ApartmentManager.DTOs;

namespace ApartmentManager.Interfaces
{
    public interface IPaymentService
    {
        Task ProcessPayment(ProcessPaymentDto processPaymentDto);
    }
}
