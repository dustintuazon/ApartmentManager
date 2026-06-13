using ApartmentManager.Data;
using ApartmentManager.DTOs;
using ApartmentManager.Interfaces;
using ApartmentManager.Models;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManager.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _appDbContext;

        public PaymentService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task ProcessPayment(ProcessPaymentDto processPaymentDto)
        {
            var transaction = new Transaction
            {
                Amount = processPaymentDto.Amount,
                Date = processPaymentDto.DatePaid,
                ModeOfPayment = processPaymentDto.MOP,
                Purpose = processPaymentDto.Purpose,
                ReferenceNumber = processPaymentDto.ReferenceNumber,
                TenantId = processPaymentDto.TenantId,
                UserId = processPaymentDto.UserId
            };
            await _appDbContext.Transactions.AddAsync(transaction);

            var tenant = await _appDbContext.Tenants.FindAsync(processPaymentDto.TenantId);
            var roomMonthly = await _appDbContext.Rooms.Where(r => r.Id == tenant.RoomId).Select(r => r.Monthly).FirstOrDefaultAsync();

            if (processPaymentDto.Purpose == Purpose.Deposit)
            {
                tenant.Deposit += processPaymentDto.Amount;
            }
            else if(processPaymentDto.Purpose == Purpose.Monthly || processPaymentDto.Purpose == Purpose.Advance)
            {
                var months = 1;
                if(processPaymentDto.Amount % roomMonthly == 0)
                {
                    months = processPaymentDto.Amount / roomMonthly;
                }
                var difference = processPaymentDto.Amount - roomMonthly * months;
                tenant.Balance -= difference;
                tenant.MonthsPaid += months;
            }
            else if(processPaymentDto.Purpose == Purpose.Balance)
            {
                tenant.Balance -= processPaymentDto.Amount;
            }


            await _appDbContext.SaveChangesAsync();
        }
    }
}
