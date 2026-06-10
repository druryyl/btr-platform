using System.Collections.Generic;
using System.Linq;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.domain.SalesContext.VisitPlanAgg;
using FluentAssertions;
using Xunit;

namespace btr.test.SalesContext
{
    public class EffectiveVisitPlanResolverTest
    {
        private readonly EffectiveVisitPlanResolver _resolver = new EffectiveVisitPlanResolver();

        [Fact]
        public void Resolve_AddException_IncludesCustomer()
        {
            var basePlan = new List<VisitPlanModel>
            {
                new VisitPlanModel { CustomerId = "C001", NoUrut = 1, CustomerName = "Alpha" }
            };
            var exceptions = new List<VisitPlanExceptionModel>
            {
                new VisitPlanExceptionModel
                {
                    ExceptionType = VisitPlanExceptionTypeEnum.Add.ToString(),
                    CustomerId = "C002",
                    CustomerName = "Beta"
                }
            };

            var result = _resolver.Resolve(basePlan, exceptions);

            result.Should().HaveCount(2);
            result.Should().Contain(x => x.CustomerId == "C002" && x.Origin == "Added");
        }

        [Fact]
        public void Resolve_RemoveException_ExcludesCustomer()
        {
            var basePlan = new List<VisitPlanModel>
            {
                new VisitPlanModel { CustomerId = "C001", NoUrut = 1 },
                new VisitPlanModel { CustomerId = "C002", NoUrut = 2 }
            };
            var exceptions = new List<VisitPlanExceptionModel>
            {
                new VisitPlanExceptionModel
                {
                    ExceptionType = VisitPlanExceptionTypeEnum.Remove.ToString(),
                    CustomerId = "C001"
                }
            };

            var result = _resolver.Resolve(basePlan, exceptions);

            result.Should().ContainSingle().Which.CustomerId.Should().Be("C002");
        }

        [Fact]
        public void Resolve_ReplaceException_SwapsCustomer()
        {
            var basePlan = new List<VisitPlanModel>
            {
                new VisitPlanModel { CustomerId = "C001", NoUrut = 1, CustomerName = "Alpha" }
            };
            var exceptions = new List<VisitPlanExceptionModel>
            {
                new VisitPlanExceptionModel
                {
                    ExceptionType = VisitPlanExceptionTypeEnum.Replace.ToString(),
                    CustomerId = "C001",
                    ReplacementCustomerId = "C009",
                    ReplacementCustomerName = "Zeta"
                }
            };

            var result = _resolver.Resolve(basePlan, exceptions).ToList();

            result.Should().ContainSingle();
            result[0].CustomerId.Should().Be("C009");
            result[0].Origin.Should().Be("Replaced");
        }

        [Fact]
        public void Resolve_EmptyBaseWithAdd_ReturnsAddedCustomer()
        {
            var exceptions = new List<VisitPlanExceptionModel>
            {
                new VisitPlanExceptionModel
                {
                    ExceptionType = VisitPlanExceptionTypeEnum.Add.ToString(),
                    CustomerId = "C001",
                    CustomerName = "Alpha"
                }
            };

            var result = _resolver.Resolve(new List<VisitPlanModel>(), exceptions);

            result.Should().ContainSingle().Which.Origin.Should().Be("Added");
        }

        [Fact]
        public void Resolve_DuplicateAdd_DoesNotDuplicateCustomer()
        {
            var basePlan = new List<VisitPlanModel>
            {
                new VisitPlanModel { CustomerId = "C001", NoUrut = 1 }
            };
            var exceptions = new List<VisitPlanExceptionModel>
            {
                new VisitPlanExceptionModel
                {
                    ExceptionType = VisitPlanExceptionTypeEnum.Add.ToString(),
                    CustomerId = "C001"
                }
            };

            var result = _resolver.Resolve(basePlan, exceptions);

            result.Should().ContainSingle();
        }
    }
}
