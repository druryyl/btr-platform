using btr.application.BrgContext.BrgAgg;
using btr.application.BrgContext.BrgBarcodeAgg;
using btr.domain.BrgContext.BrgBarcodeAgg;
using btr.infrastructure.BrgContext.BrgAgg;
using btr.infrastructure.BrgContext.BrgBarcodeAgg;
using btr.infrastructure.Helpers;
using btr.nuna.Domain;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace j07_btrade_sync.Shared
{
    //  IR-08 — the synchronization client references btr.application /
    //  btr.infrastructure directly and invokes the Main Office application
    //  command in-process, so authoritative validation (BR-011, P-01) exists
    //  in exactly one place.
    public class MainOfficeCommandExecutor
    {
        private readonly IMediator _mediator;

        public MainOfficeCommandExecutor()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IOptions<DatabaseOptions>>(
                Options.Create(new DatabaseOptions()));
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(
                typeof(ProcessBarcodeRegistrationRequestHandler).Assembly));
            services.AddScoped<DateTimeProvider>();
            services.AddScoped<IValidator<BrgBarcodeModel>, BrgBarcodeValidator>();
            services.AddScoped<IBrgDal, BrgDal>();
            services.AddScoped<IBrgSatuanDal, BrgSatuanDal>();
            services.AddScoped<IBrgBarcodeDal, BrgBarcodeDal>();
            services.AddScoped<IBrgBarcodeBuilder, BrgBarcodeBuilder>();
            services.AddScoped<IBrgBarcodeWriter, BrgBarcodeWriter>();

            var provider = services.BuildServiceProvider();
            _mediator = provider.GetRequiredService<IMediator>();
        }

        public Task<ProcessBarcodeRegistrationRequestResponse> ProcessBarcodeRegistration(
            string barcodeValue, string brgId, string satuan, string userId)
        {
            return _mediator.Send(new ProcessBarcodeRegistrationRequestCommand(
                barcodeValue, brgId, satuan, userId));
        }
    }
}
