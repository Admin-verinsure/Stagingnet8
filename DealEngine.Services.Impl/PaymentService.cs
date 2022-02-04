using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;

namespace DealEngine.Services.Impl
{
    public class PaymentService : IPaymentService
    {
        IMapperSession<Payment> _paymentRepository;

        public PaymentService(IMapperSession<Payment> paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task Update(Payment payment)
        {
            await _paymentRepository.UpdateAsync(payment);            
        }

        public async Task<Payment> AddNewPayment(User createdBy, ClientProgramme clientProgramme, Merchant merchant, PaymentGateway paymentGateway)
        {
            if (string.IsNullOrWhiteSpace(clientProgramme.ToString()))
                throw new ArgumentNullException(nameof(clientProgramme));
            if (string.IsNullOrWhiteSpace(merchant.ToString()))
                throw new ArgumentNullException(nameof(merchant));
            if (string.IsNullOrWhiteSpace(paymentGateway.ToString()))
                throw new ArgumentNullException(nameof(paymentGateway));

            Payment payment = new Payment(createdBy, clientProgramme, merchant, paymentGateway);
            await _paymentRepository.AddAsync(payment);
            //check with craig best way to check
            return payment;//CheckExists(paymentGatewayWebServiceURL);
        }

        public async Task<List<Payment>> GetAllPayment()
        {
            return await _paymentRepository.FindAll().Where(p => p.DateDeleted == null).ToListAsync();
        }

        public async Task<Payment> GetPayment(Guid clientProgrammeID, Guid merchantID, Guid paymentGatewayID)
        {
            return await _paymentRepository.FindAll().FirstOrDefaultAsync(p => p.PaymentClientProgramme.Id == clientProgrammeID && p.DateDeleted == null &&
                                                             p.PaymentMerchant.Id == merchantID && p.PaymentPaymentGateway.Id == paymentGatewayID);
        }

        public async Task<Payment> GetPayment(Guid clientProgrammeID)
        {
            return await _paymentRepository.FindAll().FirstOrDefaultAsync(p => p.PaymentClientProgramme.Id == clientProgrammeID && p.DateDeleted == null);
        }

    }
}
