using Microsoft.Extensions.Options;

namespace btr.application.Portal
{
    public sealed class PresentationModeService : IPresentationModeService
    {
        public PresentationModeService(IOptions<PresentationOptions> options)
        {
            var value = options?.Value ?? new PresentationOptions();
            IsEnabled = value.Enabled;
        }

        public bool IsEnabled { get; }
    }
}
