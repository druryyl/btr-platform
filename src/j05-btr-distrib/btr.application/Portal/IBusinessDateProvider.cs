using System;

namespace btr.application.Portal
{
    public interface IBusinessDateProvider
    {
        /// <summary>Calendar date used for business calculations (time component is midnight).</summary>
        DateTime Today { get; }

        bool IsPresentationActive { get; }
    }
}
