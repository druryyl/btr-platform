using System.Collections.Generic;
using btr.nuna.Domain;

namespace btr.application.SalesContext.FakturInfo
{
    public interface IFakturPrincipalOmzetDal
    {
        IReadOnlyList<FakturPrincipalOmzetDto> ListOmzetBySalesPersonPrincipal(Periode periode);
    }

    public class FakturPrincipalOmzetDto
    {
        public string SalesPersonId { get; set; }

        public string SupplierId { get; set; }

        public string SupplierName { get; set; }

        public decimal CompletedOmzet { get; set; }
    }
}
