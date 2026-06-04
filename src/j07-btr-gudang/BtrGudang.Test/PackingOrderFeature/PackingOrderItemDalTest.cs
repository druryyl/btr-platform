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
    public class PackingOrderItemDalTest
    {
        private readonly PackingOrderItemDal _sut;

        public PackingOrderItemDalTest()
        {
            _sut = new PackingOrderItemDal(ConnStringHelper.GetTestEnv());
        }

        private static PackingOrderItemDto Faker(int noUrut)
        {
            var result = new PackingOrderItemDto
            {
                PackingOrderId = "A",
                NoUrut = noUrut,
                BrgId = "B" + noUrut,
                BrgCode = "C" + noUrut,
                BrgName = "Barang " + noUrut,
                Kategori = "Kategori",
                Supplier = "SUpplier",
                QtyBesar = 10 + noUrut,
                SatBesar = "PCS",
                QtyKecil = 100 + noUrut,
                SatKecil = "UNIT",
                DepoId = "JOG", 
                PrintLogId = "01AN4Z07BY79KA1307SR9X4MV3"
            };

            return result;
        }

        [Fact]
        public void InsertTest()
        {
            using (var trans = TransHelper.NewScope())
            {
                var listDto = new List<PackingOrderItemDto>
            {
                Faker(1),
                Faker(2),
                Faker(3)
            };

                _sut.Insert(listDto);
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
        public void ListDataTest()
        {
            var expected = new List<PackingOrderItemDto>
        {
            Faker(1),
            Faker(2),
            Faker(3)
        };

            using (var trans = TransHelper.NewScope())
            {
                _sut.Insert(expected);
                var actual = _sut.ListData(PackingOrderModel.Key("A"));
                actual.Should().BeEquivalentTo(expected);
            }
        }
    }
}
