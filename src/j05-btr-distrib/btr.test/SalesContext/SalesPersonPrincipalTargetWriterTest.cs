using btr.application.SalesContext.SalesPersonPrincipalTargetAgg;
using btr.application.SalesContext.SalesPersonPrincipalTargetAgg.Contracts;
using btr.domain.SalesContext.SalesPersonAgg;
using btr.domain.SalesContext.SalesPersonPrincipalTargetAgg;
using btr.domain.SalesContext.SalesPersonSupplierAgg;
using btr.nuna.Domain;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace btr.test.SalesContext
{
    public class SalesPersonPrincipalTargetWriterTest
    {
        [Fact]
        public void Save_RejectsNegativeAmount()
        {
            var sut = CreateWriter(
                new SalesPersonModel("SP1") { SalesPersonName = "Budi" },
                new[] { new SalesPersonSupplierModel { SalesPersonId = "SP1", SupplierId = "S1" } });

            Action act = () => sut.Save("SP1", 2026, 1, new[]
            {
                new SalesPersonPrincipalTargetViewDto("S1", "C1", "Principal 1", -1m)
            });

            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void Save_RejectsUnassignedPrincipal()
        {
            var sut = CreateWriter(
                new SalesPersonModel("SP1") { SalesPersonName = "Budi" },
                new[] { new SalesPersonSupplierModel { SalesPersonId = "SP1", SupplierId = "S1" } });

            Action act = () => sut.Save("SP1", 2026, 1, new[]
            {
                new SalesPersonPrincipalTargetViewDto("S9", "C9", "Other", 100m)
            });

            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*not assigned*");
        }

        [Fact]
        public void Save_UpsertsAssignedPrincipalTargets()
        {
            var targetDal = new StubPrincipalTargetDal();
            var sut = CreateWriter(
                new SalesPersonModel("SP1") { SalesPersonName = "Budi" },
                new[] { new SalesPersonSupplierModel { SalesPersonId = "SP1", SupplierId = "S1" } },
                targetDal);

            sut.Save("SP1", 2026, 1, new[]
            {
                new SalesPersonPrincipalTargetViewDto("S1", "C1", "Principal 1", 500m)
            });

            targetDal.UpsertedRows.Should().HaveCount(1);
            targetDal.UpsertedRows[0].TargetAmount.Should().Be(500m);
        }

        private static SalesPersonPrincipalTargetWriter CreateWriter(
            SalesPersonModel salesPerson,
            IEnumerable<SalesPersonSupplierModel> assignments,
            StubPrincipalTargetDal targetDal = null)
        {
            targetDal = targetDal ?? new StubPrincipalTargetDal();
            var supplierDal = new StubSalesPersonSupplierDal(assignments);
            var salesPersonDal = new StubSalesPersonDal(salesPerson);
            return new SalesPersonPrincipalTargetWriter(targetDal, supplierDal, salesPersonDal);
        }

        private sealed class StubPrincipalTargetDal : ISalesPersonPrincipalTargetDal
        {
            public List<SalesPersonPrincipalTargetModel> UpsertedRows { get; } =
                new List<SalesPersonPrincipalTargetModel>();

            public IEnumerable<SalesPersonPrincipalTargetModel> ListBySalesPersonPeriod(
                string salesPersonId, int year, int month)
                => Enumerable.Empty<SalesPersonPrincipalTargetModel>();

            public IEnumerable<SalesPersonPrincipalTargetModel> ListByPeriod(int year, int month)
                => Enumerable.Empty<SalesPersonPrincipalTargetModel>();

            public void Upsert(IEnumerable<SalesPersonPrincipalTargetModel> rows)
            {
                UpsertedRows.AddRange(rows);
            }

            public decimal SumBySalesPersonPeriod(string salesPersonId, int year, int month) => 0m;

            public IReadOnlyDictionary<string, decimal> SumByPeriod(int year, int month)
                => new Dictionary<string, decimal>();
        }

        private sealed class StubSalesPersonSupplierDal : btr.application.SalesContext.SalesPersonSupplierAgg.Contracts.ISalesPersonSupplierDal
        {
            private readonly List<SalesPersonSupplierModel> _items;

            public StubSalesPersonSupplierDal(IEnumerable<SalesPersonSupplierModel> items)
            {
                _items = items.ToList();
            }

            public void Insert(IEnumerable<SalesPersonSupplierModel> listModel) =>
                throw new NotSupportedException();

            public void Delete(ISalesPersonKey key) => throw new NotSupportedException();

            public IEnumerable<SalesPersonSupplierModel> ListData(ISalesPersonKey filter)
            {
                return _items.Where(x => x.SalesPersonId == filter.SalesPersonId);
            }
        }

        private sealed class StubSalesPersonDal : btr.application.SalesContext.SalesPersonAgg.Contracts.ISalesPersonDal
        {
            private readonly SalesPersonModel _salesPerson;

            public StubSalesPersonDal(SalesPersonModel salesPerson)
            {
                _salesPerson = salesPerson;
            }

            public IEnumerable<SalesPersonModel> ListData() => new[] { _salesPerson };

            public void Insert(SalesPersonModel model) => throw new NotSupportedException();

            public void Update(SalesPersonModel model) => throw new NotSupportedException();

            public void Delete(ISalesPersonKey key) => throw new NotSupportedException();

            public SalesPersonModel GetData(ISalesPersonKey key)
            {
                return string.Equals(key.SalesPersonId, _salesPerson.SalesPersonId, StringComparison.OrdinalIgnoreCase)
                    ? _salesPerson
                    : null;
            }
        }
    }
}
