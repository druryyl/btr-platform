using System;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.application.ReportingContext.EntityAnalyticsAgg.Services;
using FluentAssertions;
using Xunit;

namespace btr.test.ReportingContext
{
    public class PeerGroupRuleCatalogTest
    {
        [Fact]
        public void GetRulesForEntityType_Customer_ReturnsWilayahAndKlasifikasi()
        {
            var rules = PeerGroupRuleCatalog.GetRulesForEntityType(EntityTypeCode.Customer);

            rules.Should().HaveCount(2);
            rules.Should().Contain(r => r.RuleId == PeerGroupResolver.CustomerWilayah && r.IsDefault);
            rules.Should().Contain(r => r.RuleId == PeerGroupResolver.CustomerKlasifikasi && !r.IsDefault);
        }

        [Fact]
        public void GetRulesForEntityType_Item_ReturnsPrincipalAndCategory()
        {
            var rules = PeerGroupRuleCatalog.GetRulesForEntityType(EntityTypeCode.Item);

            rules.Should().HaveCount(2);
            rules.Should().Contain(r => r.RuleId == PeerGroupResolver.ItemPrincipal && r.IsDefault);
            rules.Should().Contain(r => r.RuleId == PeerGroupResolver.ItemCategory && !r.IsDefault);
        }

        [Fact]
        public void TryResolveRule_ValidCustomerKlasifikasi_ReturnsDefinition()
        {
            var rule = PeerGroupRuleCatalog.TryResolveRule(
                EntityTypeCode.Customer,
                PeerGroupResolver.CustomerKlasifikasi);

            rule.Should().NotBeNull();
            rule.DisplayLabel.Should().Be("Klasifikasi");
            rule.DimensionLabel.Should().Be("Klasifikasi");
        }

        [Fact]
        public void TryResolveRule_InvalidRule_ReturnsNull()
        {
            PeerGroupRuleCatalog.TryResolveRule(EntityTypeCode.Customer, "not-a-rule")
                .Should().BeNull();
        }

        [Fact]
        public void TryResolveRule_WrongEntityType_ReturnsNull()
        {
            PeerGroupRuleCatalog.TryResolveRule(
                    EntityTypeCode.Item,
                    PeerGroupResolver.CustomerKlasifikasi)
                .Should().BeNull();
        }

        [Fact]
        public void ResolveEffectiveRuleId_RequestedOverride_UsesOverride()
        {
            var entityTypes = new EntityTypeRegistry();
            new EntityAnalyticsPlatformRegistrar().Register(entityTypes, null, null);

            var ruleId = PeerGroupRuleCatalog.ResolveEffectiveRuleId(
                EntityTypeCode.Customer,
                PeerGroupResolver.CustomerKlasifikasi,
                entityTypes);

            ruleId.Should().Be(PeerGroupResolver.CustomerKlasifikasi);
        }

        [Fact]
        public void ResolveEffectiveRuleId_NullRequest_UsesRegistrationDefault()
        {
            var entityTypes = new EntityTypeRegistry();
            new EntityAnalyticsPlatformRegistrar().Register(entityTypes, null, null);

            var ruleId = PeerGroupRuleCatalog.ResolveEffectiveRuleId(
                EntityTypeCode.Customer,
                null,
                entityTypes);

            ruleId.Should().Be(PeerGroupResolver.CustomerWilayah);
        }

        [Fact]
        public void ResolveEffectiveRuleId_NullRequest_UsesItemPrincipalDefault()
        {
            var entityTypes = new EntityTypeRegistry();
            new EntityAnalyticsPlatformRegistrar().Register(entityTypes, null, null);

            var ruleId = PeerGroupRuleCatalog.ResolveEffectiveRuleId(
                EntityTypeCode.Item,
                null,
                entityTypes);

            ruleId.Should().Be(PeerGroupResolver.ItemPrincipal);
        }

        [Fact]
        public void ResolveEffectiveRuleId_InvalidRequest_Throws()
        {
            var entityTypes = new EntityTypeRegistry();
            new EntityAnalyticsPlatformRegistrar().Register(entityTypes, null, null);

            Action act = () => PeerGroupRuleCatalog.ResolveEffectiveRuleId(
                EntityTypeCode.Customer,
                "bogus-rule",
                entityTypes);

            act.Should().Throw<ArgumentException>()
                .WithMessage("*bogus-rule*");
        }
    }
}
