using Bilreg.Infrastructure.Shared.Helpers;
using BtrGudang.Domain.PackingOrderFeature;
using BtrGudang.Infrastructure.PackingOrderFeature;
using BtrGudang.Winform.Domain;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace BtrGudang.Test.PackingOrderFeature
{
    public class PackingOrderRepoTest
    {
        private readonly Mock<IPackingOrderDal> _packingOrderDalMock;
        private readonly Mock<IPackingOrderItemDal> _packingOrderItemDalMock;
        private readonly PackingOrderRepo _sut;

        public PackingOrderRepoTest()
        {
            _packingOrderDalMock = new Mock<IPackingOrderDal>();
            _packingOrderItemDalMock = new Mock<IPackingOrderItemDal>();
            _sut = new PackingOrderRepo(_packingOrderDalMock.Object, _packingOrderItemDalMock.Object);
        }

        private PackingOrderModel CreateTestModel()
        {
            var customer = new CustomerReff("C1", "C001", "Customer Test", "Alamat", "08123");
            var faktur = new FakturReff("F1", "F001", new DateTime(2026, 2, 1), "ADM", 234);
            var location = new LocationReff(1.23, 4.56, 7);
            var driver = new DriverReff("DR1", "Driver One");

            var items = new List<PackingOrderItemModel>
            {
                new PackingOrderItemModel(1,
                    new BrgReff("B1", "BRG001", "Barang 1", "Kategori", "Supplier"),
                    new QtyType(10, "PCS"),
                    new QtyType(100, "UNIT"), "JOG", "01AN4Z07BY79KA1307SR9X4MV3"),
                new PackingOrderItemModel(2,
                    new BrgReff("B2", "BRG002", "Barang 2", "Kategori", "Supplier"),
                    new QtyType(5, "PCS"),
                    new QtyType(50, "UNIT"), "JOG", "01AN4Z07BY79KA1307SR9X4MV4")
            };

            return new PackingOrderModel(
                "PO001",
                new DateTime(2026, 2, 1),
                customer,
                location,
                faktur,
                driver,
                new DateTime(2026,2,12),
                "JOG",
                "01AN4Z07BY79KA1307SR9X4MV5",
                new DateTime(3000,1,1),
                string.Empty,
                string.Empty,

                items);
        }

        [Fact]
        public void UT01_SaveChanges_WhenEntityExists_ShouldUpdateAndReInsertItems()
        {
            // Arrange
            var model = CreateTestModel();
            var key = PackingOrderModel.Key("PO001");
            var dto = PackingOrderDto.FromModel(model);
            var itemDtos = model.ListItem
                .Select(x => PackingOrderItemDto.FromModel(x, model.PackingOrderId))
                .ToList();

            _packingOrderDalMock
                .Setup(x => x.GetData(It.Is<IPackingOrderKey>(k => k.PackingOrderId == "PO001")))
                .Returns(dto);
            _packingOrderItemDalMock
                .Setup(x => x.ListData(It.Is<IPackingOrderKey>(k => k.PackingOrderId == "PO001")))
                .Returns(itemDtos);

            // Act
            _sut.SaveChanges(model);

            // Assert
            _packingOrderDalMock.Verify(x => x.Update(It.IsAny<PackingOrderDto>()), Times.Once);
            _packingOrderDalMock.Verify(x => x.Insert(It.IsAny<PackingOrderDto>()), Times.Never);
            _packingOrderItemDalMock.Verify(x => x.Delete(model), Times.Once);
            _packingOrderItemDalMock.Verify(x => x.Insert(It.IsAny<IEnumerable<PackingOrderItemDto>>()), Times.Once);
        }

        [Fact]
        public void UT02_SaveChanges_WhenEntityNotExists_ShouldInsert()
        {
            // Arrange
            var model = CreateTestModel();
            var key = PackingOrderModel.Key("PO001");
            var dto = PackingOrderDto.FromModel(model);
            var itemDtos = model.ListItem
                .Select(x => PackingOrderItemDto.FromModel(x, model.PackingOrderId))
                .ToList();

            _packingOrderDalMock
                .Setup(x => x.GetData(It.Is<IPackingOrderKey>(k => k.PackingOrderId == "PO001")))
                .Returns((PackingOrderDto)null);
            _packingOrderItemDalMock
                .Setup(x => x.ListData(It.Is<IPackingOrderKey>(k => k.PackingOrderId == "PO001")))
                .Returns(Enumerable.Empty<PackingOrderItemDto>());

            // Act
            _sut.SaveChanges(model);

            // Assert
            _packingOrderDalMock.Verify(x => x.Insert(It.IsAny<PackingOrderDto>()), Times.Once);
            _packingOrderDalMock.Verify(x => x.Update(It.IsAny<PackingOrderDto>()), Times.Never);
            _packingOrderItemDalMock.Verify(x => x.Delete(model), Times.Once);
            _packingOrderItemDalMock.Verify(x => x.Insert(It.IsAny<IEnumerable<PackingOrderItemDto>>()), Times.Once);
        }

        [Fact]
        public void UT03_DeleteEntity_ShouldDeleteHeaderAndItems()
        {
            // Arrange
            var key = PackingOrderModel.Key("PO001");

            // Act
            _sut.DeleteEntity(key);

            // Assert
            _packingOrderDalMock.Verify(x => x.Delete(key), Times.Once);
            _packingOrderItemDalMock.Verify(x => x.Delete(key), Times.Once);
        }

        [Fact]
        public void UT04_LoadEntity_WhenEntityExists_ShouldReturnSome()
        {
            // Arrange
            var key = PackingOrderModel.Key("PO001");
            var model = CreateTestModel();
            var dto = PackingOrderDto.FromModel(model);
            var itemDtos = model.ListItem
                .Select(x => PackingOrderItemDto.FromModel(x, model.PackingOrderId))
                .ToList();

            _packingOrderDalMock
                .Setup(x => x.GetData(key))
                .Returns(dto);
            _packingOrderItemDalMock
                .Setup(x => x.ListData(key))
                .Returns(itemDtos);

            // Act
            var result = _sut.LoadEntity(key);

            // Assert
            result.HasValue.Should().BeTrue();
            result.Match(
                onSome: m => m.Should().BeEquivalentTo(model),
                onNone: () => Assert.Fail("Should have value"));

            _packingOrderDalMock.Verify(x => x.GetData(key), Times.Once);
            _packingOrderItemDalMock.Verify(x => x.ListData(key), Times.Once);
        }

        [Fact]
        public void UT05_LoadEntity_WhenHeaderNotExists_ShouldReturnNone()
        {
            // Arrange
            var key = PackingOrderModel.Key("PO001");

            _packingOrderDalMock
                .Setup(x => x.GetData(key))
                .Returns((PackingOrderDto)null);
            _packingOrderItemDalMock
                .Setup(x => x.ListData(key))
                .Returns(Enumerable.Empty<PackingOrderItemDto>());

            // Act
            var result = _sut.LoadEntity(key);

            // Assert
            result.HasValue.Should().BeFalse();
            result.Match(
                onSome: m => Assert.Fail("Should not have value"),
                onNone: () => { /* Success */ });
        }

        [Fact]
        public void UT06_LoadEntity_WhenItemsEmpty_ShouldReturnModelWithEmptyItems()
        {
            // Arrange
            var key = PackingOrderModel.Key("PO001");
            var model = CreateTestModel();
            var dto = PackingOrderDto.FromModel(model);

            _packingOrderDalMock
                .Setup(x => x.GetData(key))
                .Returns(dto);
            _packingOrderItemDalMock
                .Setup(x => x.ListData(key))
                .Returns(Enumerable.Empty<PackingOrderItemDto>());

            // Act
            var result = _sut.LoadEntity(key);

            // Assert
            result.HasValue.Should().BeTrue();
            result.Match(
                onSome: m =>
                {
                    m.PackingOrderId.Should().Be(model.PackingOrderId);
                    m.ListItem.Should().BeEmpty();
                },
                onNone: () => Assert.Fail("Should have value"));
        }
    }
}