using System;
using System.Globalization;
using btr.application.SupportContext.TglJamAgg;
using Microsoft.Extensions.Options;

namespace btr.application.Portal
{
    public sealed class PresentationBusinessDateProvider : IBusinessDateProvider
    {
        private readonly IOptions<PresentationOptions> _options;
        private readonly ITglJamDal _tglJamDal;

        public PresentationBusinessDateProvider(
            IOptions<PresentationOptions> options,
            ITglJamDal tglJamDal)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _tglJamDal = tglJamDal ?? throw new ArgumentNullException(nameof(tglJamDal));
        }

        public bool IsPresentationActive
        {
            get
            {
                var value = _options.Value ?? new PresentationOptions();
                return value.Enabled;
            }
        }

        public DateTime Today
        {
            get
            {
                var value = _options.Value ?? new PresentationOptions();
                if (!value.Enabled)
                    return _tglJamDal.Now.Date;

                if (string.IsNullOrWhiteSpace(value.BusinessDate))
                {
                    throw new InvalidOperationException(
                        "Presentation.Enabled is true but Presentation.BusinessDate is not configured. " +
                        "Set BusinessDate to an ISO date (yyyy-MM-dd) in appsettings.json.");
                }

                if (!DateTime.TryParseExact(
                        value.BusinessDate.Trim(),
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.None,
                        out var businessDate))
                {
                    throw new InvalidOperationException(
                        $"Presentation.BusinessDate '{value.BusinessDate}' is invalid. Use ISO format yyyy-MM-dd.");
                }

                return businessDate.Date;
            }
        }
    }
}
