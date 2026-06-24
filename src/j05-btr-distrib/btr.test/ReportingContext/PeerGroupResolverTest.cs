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
