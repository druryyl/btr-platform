using Bilreg.Infrastructure.Shared.Helpers;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Helper.Common;
using BtrGudang.Infrastructure.PackingOrderFeature;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BtrGudang.Test.PackingOrderFeature
{
    public class PackingOrderDalTest
    {
        private readonly PackingOrderDal _sut;

        public PackingOrderDalTest()
        {
            _sut = new PackingOrderDal(ConnStringHelper.GetTestEnv());
        }

        private static PackingOrderDto Faker()
        {
            var result = new PackingOrderDto
            {
                PackingOrderId = "A",
                PackingOrderDate = new DateTime(2026, 2, 1),
                CustomerId = "C",
                CustomerCode = "D",
                CustomerName = "E",
                Alamat = "F",
                NoTelp = "G",
                FakturId = "H",
                FakturCode = "I",
                FakturDate = new DateTime(2026, 3, 1),
                AdminName = "J",
                Latitude = 1.23,
                Longitude = 4.56,
                Accuracy = 7,
                DownloadTimestamp = new DateTime(2026,2,12),
                OfficeCode = "R",
                PrintLogId = "01AN4Z07BY79KA1307SR9X4MV3"
            };

            return result;
        }

        [Fact]
        public void InsertTest()
        {
            using (var trans = TransHelper.NewScope())
            {
                _sut.Insert(Faker());
            }
        }

        [Fact]
        public void UpdateTest()
        {
            using (var trans = TransHelper.NewScope())
            {
                _sut.Update(Faker());
            }
        }

        [Fact]
        public void DeleteTest()
        {
            using (var trans = TransHelper.NewScope())
            {
                var key = PackingOrderModel.Key("A");
                _sut.Delete(key);
            }
        }

        [Fact]
        public void GetDataTest()
        {
            var expected = Faker();
            using (var trans = TransHelper.NewScope())
            {
                _sut.Insert(expected);
                var actual = _sut.GetData(PackingOrderModel.Key("A"));
                actual.Should().BeEquivalentTo(expected);
            }
        }

        [Fact]
        public void ListDataTest()
        {
            var expected = Faker();
            using (var trans = TransHelper.NewScope())
            {
                _sut.Insert(expected);
                var actual = _sut.ListData(new Periode(new DateTime(2026, 2, 1)));
                actual.Should().BeEquivalentTo(new List<PackingOrderDto> { expected });
            }
        }

    }
}
