using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PeerGroupLabelFormatterTest
    {
        [Fact]
        public void Format_CustomerWilayah_IncludesCountAndDimension()
        {
            PeerGroupLabelFormatter.Format(
                    EntityTypeCode.Customer,
                    PeerGroupResolver.CustomerWilayah,
                    47,
                    "Jakarta Pusat")
                .Should().Be("47 customers in Wilayah: Jakarta Pusat");
        }

        [Fact]
        public void Format_CustomerKlasifikasi_IncludesCountAndDimension()
        {
            PeerGroupLabelFormatter.Format(
                    EntityTypeCode.Customer,
                    PeerGroupResolver.CustomerKlasifikasi,
                    47,
                    "A")
                .Should().Be("47 customers in Klasifikasi: A");
        }

        [Fact]
        public void Format_ItemPrincipal_IncludesCountAndDimension()
        {
            PeerGroupLabelFormatter.Format(
                    EntityTypeCode.Item,
                    PeerGroupResolver.ItemPrincipal,
                    32,
                    "Alpha Principal")
                .Should().Be("32 items in Principal: Alpha Principal");
        }

        [Fact]
        public void Format_ItemCategory_IncludesCountAndDimension()
        {
            PeerGroupLabelFormatter.Format(
                    EntityTypeCode.Item,
                    PeerGroupResolver.ItemCategory,
                    32,
                    "OTC")
                .Should().Be("32 items in Category: OTC");
        }

        [Fact]
        public void Format_SalesmanAllActive_UsesActiveLabel()
        {
            PeerGroupLabelFormatter.Format(
                    EntityTypeCode.Salesman,
                    PeerGroupResolver.SalesmanAllActive,
                    15,
                    null)
                .Should().Be("15 active salesmen");
        }

        [Fact]
        public void Format_SupplierAllActive_UsesActiveLabel()
        {
            PeerGroupLabelFormatter.Format(
                    EntityTypeCode.Supplier,
                    PeerGroupResolver.SupplierAllActive,
                    1,
                    null)
                .Should().Be("1 active supplier");
        }

        [Fact]
        public void Format_EmptyPeerGroup_ReturnsNoPeersAvailable()
        {
            PeerGroupLabelFormatter.Format(
                    EntityTypeCode.Customer,
                    PeerGroupResolver.CustomerWilayah,
                    0,
                    "Jakarta Pusat")
                .Should().Be("No peers available");
        }
    }
}
