using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models.Snapshot;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PeerGroupResolverTest
    {
        [Fact]
        public void BuildPeerGroupIndex_CustomerWilayah_GroupsByDimension()
        {
            var population = new List<EntityPopulationRow>
            {
                new EntityPopulationRow { EntityId = "C001", IsActive = true, DimensionValue = "Jakarta" },
                new EntityPopulationRow { EntityId = "C002", IsActive = true, DimensionValue = "Jakarta" },
                new EntityPopulationRow { EntityId = "C003", IsActive = true, DimensionValue = "Bandung" }
            };

            var index = PeerGroupResolver.BuildPeerGroupIndex(PeerGroupResolver.CustomerWilayah, population);

            index["C001"].Should().BeEquivalentTo(new[] { "C001", "C002" });
            index["C003"].Should().BeEquivalentTo(new[] { "C003" });
        }

        [Fact]
        public void BuildPeerGroupIndex_CustomerKlasifikasi_GroupsByDimension()
        {
            var population = new List<EntityPopulationRow>
            {
                new EntityPopulationRow { EntityId = "C001", IsActive = true, DimensionValue = "A" },
                new EntityPopulationRow { EntityId = "C002", IsActive = true, DimensionValue = "A" },
                new EntityPopulationRow { EntityId = "C003", IsActive = true, DimensionValue = "B" }
            };

            var index = PeerGroupResolver.BuildPeerGroupIndex(PeerGroupResolver.CustomerKlasifikasi, population);

            index["C001"].Should().BeEquivalentTo(new[] { "C001", "C002" });
            index["C003"].Should().BeEquivalentTo(new[] { "C003" });
        }

        [Fact]
        public void ResolveDimensionKpiId_CustomerKlasifikasi_ReturnsKlasifikasiMetaId()
        {
            PeerGroupResolver.ResolveDimensionKpiId(PeerGroupResolver.CustomerKlasifikasi)
                .Should().Be(EntityAnalyticsMetaKpiIds.Klasifikasi);
        }

        [Fact]
        public void BuildPeerGroupIndex_ItemPrincipal_GroupsByDimension()
        {
            var population = new List<EntityPopulationRow>
            {
                new EntityPopulationRow { EntityId = "I001", IsActive = true, DimensionValue = "Alpha Principal" },
                new EntityPopulationRow { EntityId = "I002", IsActive = true, DimensionValue = "Alpha Principal" },
                new EntityPopulationRow { EntityId = "I003", IsActive = true, DimensionValue = "Beta Principal" }
            };

            var index = PeerGroupResolver.BuildPeerGroupIndex(PeerGroupResolver.ItemPrincipal, population);

            index["I001"].Should().BeEquivalentTo(new[] { "I001", "I002" });
            index["I003"].Should().BeEquivalentTo(new[] { "I003" });
        }

        [Fact]
        public void ResolveDimensionKpiId_ItemPrincipal_ReturnsSupplierName()
        {
            PeerGroupResolver.ResolveDimensionKpiId(PeerGroupResolver.ItemPrincipal)
                .Should().Be(EntityAnalyticsMetaKpiIds.SupplierName);
        }

        [Fact]
        public void ResolveForEntity_SmallPeerGroup_IsNotSufficient()
        {
            var population = new List<EntityPopulationRow>
            {
                new EntityPopulationRow { EntityId = "C001", IsActive = true, DimensionValue = "Solo" },
                new EntityPopulationRow { EntityId = "C002", IsActive = true, DimensionValue = "Solo" }
            };
            var index = PeerGroupResolver.BuildPeerGroupIndex(PeerGroupResolver.CustomerWilayah, population);

            var resolution = PeerGroupResolver.ResolveForEntity("C001", PeerGroupResolver.CustomerWilayah, index, population);

            resolution.PeerGroupSize.Should().Be(2);
            resolution.IsSufficient.Should().BeFalse();
        }

        [Fact]
        public void ResolveForEntity_FivePeers_IsSufficient()
        {
            var population = Enumerable.Range(1, 5)
                .Select(i => new EntityPopulationRow
                {
                    EntityId = $"C00{i}",
                    IsActive = true,
                    DimensionValue = "Medan"
                })
                .ToList();
            var index = PeerGroupResolver.BuildPeerGroupIndex(PeerGroupResolver.CustomerWilayah, population);

            var resolution = PeerGroupResolver.ResolveForEntity("C001", PeerGroupResolver.CustomerWilayah, index, population);

            resolution.PeerGroupSize.Should().Be(5);
            resolution.IsSufficient.Should().BeTrue();
        }
    }
}
