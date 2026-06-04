using btrade.application.Contract;
using btrade.domain.SalesFeature;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.application.UseCase
{
    public record CustomerLocationUpdateCommand(string CustomerId,
        double Latitude, double Longitude, double Accuracy,
        long CoordinateTimeStamp, string CoordinateUser, string ServerId) : IRequest, IServerId;

    public class CustomerLocationUpdateHandler : IRequestHandler<CustomerLocationUpdateCommand>
    {
        private readonly ICustomerDal _customerDal;

        public CustomerLocationUpdateHandler(ICustomerDal customerDal)
        {
            _customerDal = customerDal;
        }

        public Task Handle(CustomerLocationUpdateCommand request, CancellationToken cancellationToken)
        {
            var key = new CustomerKey(request.CustomerId, request.ServerId);
            var customerMayBe = _customerDal.GetData(key);
            if (!customerMayBe.HasValue)
                return Task.CompletedTask;
            
            var customer = customerMayBe.Value;
            customer = customer.SetLocation(
                request.Latitude,
                request.Longitude,
                request.Accuracy,
                request.CoordinateTimeStamp,
                request.CoordinateUser);
            _customerDal.Update(customer);
            return Task.CompletedTask;
        }
    }
}
