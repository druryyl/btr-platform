using System.Collections.Generic;
using btr.nuna.Domain;

namespace btr.application.ReportingContext.PrincipalAnalyticsAgg.Contracts
{
    public interface IPrincipalContributionEvidenceDal
    {
        IReadOnlyList<PrincipalContributionEvidenceLine> ListContributionEvidence(Periode periode);
    }

    /// <summary>
    /// One Faktur Item line attributed to a Principal through the item master,
    /// with the commercial owner taken from Faktur.SalesPersonId.
    /// Field-activity performer attribution is not used.
    /// </summary>
    public class PrincipalContributionEvidenceLine
    {
        public string FakturId { get; set; }

        public string FakturItemId { get; set; }

        public string SalesPersonId { get; set; }

        public string SalesPersonCode { get; set; }

        public string SalesPersonName { get; set; }

        public string ItemSupplierId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal SubTotal { get; set; }

        public decimal DiscRp { get; set; }
    }
}
