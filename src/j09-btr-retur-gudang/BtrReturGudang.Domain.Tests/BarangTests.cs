using System.Linq;
using BtrReturGudang.Domain.Entities;
using Xunit;

namespace BtrReturGudang.Domain.Tests
{
    public class BarangTests
    {
        [Fact]
        public void Constructor_AssignsPropertiesCorrectly()
        {
            var barang = new Barang("BRG-001", "Indomie Goreng", "Dus", "Pcs", konversi: 24, isActive: true);

            Assert.Equal("BRG-001", barang.BrgId);
            Assert.Equal("Indomie Goreng", barang.BrgName);
            Assert.Equal("Dus", barang.SatBesar);
            Assert.Equal("Pcs", barang.SatKecil);
            Assert.Equal(24, barang.Konversi);
            Assert.True(barang.IsActive);
        }

        [Fact]
        public void Constructor_WhenKonversiIsZeroOrNegative_DefaultsToOne()
        {
            var barangZero = new Barang("BRG-001", "Indomie Goreng", "Dus", "Pcs", konversi: 0);
            var barangNegative = new Barang("BRG-002", "Minyak Goreng", "Dus", "Pcs", konversi: -5);

            Assert.Equal(1, barangZero.Konversi);
            Assert.Equal(1, barangNegative.Konversi);
        }

        [Fact]
        public void GetConversionForUnit_ReturnsKonversi_WhenUnitMatchesSatBesarCaseInsensitive()
        {
            var barang = new Barang("BRG-001", "Indomie Goreng", satBesar: "Dus", satKecil: "Pcs", konversi: 24);

            Assert.Equal(24, barang.GetConversionForUnit("DUS"));
            Assert.Equal(24, barang.GetConversionForUnit("dus"));
            Assert.Equal(24, barang.GetConversionForUnit("Dus"));
        }

        [Fact]
        public void GetConversionForUnit_ReturnsOne_WhenUnitMatchesSatKecilCaseInsensitive()
        {
            var barang = new Barang("BRG-001", "Indomie Goreng", satBesar: "Dus", satKecil: "Pcs", konversi: 24);

            Assert.Equal(1, barang.GetConversionForUnit("PCS"));
            Assert.Equal(1, barang.GetConversionForUnit("pcs"));
            Assert.Equal(1, barang.GetConversionForUnit("Pcs"));
        }

        [Fact]
        public void GetConversionForUnit_ReturnsOne_WhenUnitIsUnknownOrNullOrWhitespace()
        {
            var barang = new Barang("BRG-001", "Indomie Goreng", satBesar: "Dus", satKecil: "Pcs", konversi: 24);

            Assert.Equal(1, barang.GetConversionForUnit(null));
            Assert.Equal(1, barang.GetConversionForUnit(""));
            Assert.Equal(1, barang.GetConversionForUnit("   "));
            Assert.Equal(1, barang.GetConversionForUnit("Karton"));
        }

        [Fact]
        public void GetConversionForUnit_ReturnsOne_WhenKonversiIsOneOrNegative()
        {
            var barangKonversiOne = new Barang("BRG-001", "Indomie Goreng", satBesar: "Dus", satKecil: "Pcs", konversi: 1);
            var barangKonversiNegative = new Barang("BRG-002", "Minyak Goreng", satBesar: "Dus", satKecil: "Pcs", konversi: -10);

            Assert.Equal(1, barangKonversiOne.GetConversionForUnit("Dus"));
            Assert.Equal(1, barangKonversiNegative.GetConversionForUnit("Dus"));
        }

        [Fact]
        public void GetAvailableUnits_ReturnsBoth_WhenSatBesarAndSatKecilDistinct()
        {
            var barang = new Barang("BRG-001", "Indomie Goreng", satBesar: "Dus", satKecil: "Pcs");

            var units = barang.GetAvailableUnits().ToList();

            Assert.Equal(2, units.Count);
            Assert.Equal("Dus", units[0]);
            Assert.Equal("Pcs", units[1]);
        }

        [Fact]
        public void GetAvailableUnits_ReturnsSingleUnit_WhenSatBesarEqualsSatKecilCaseInsensitive()
        {
            var barang = new Barang("BRG-002", "Minyak Goreng", satBesar: "PCS", satKecil: "pcs");

            var units = barang.GetAvailableUnits().ToList();

            Assert.Single(units);
            Assert.Equal("PCS", units[0]);
        }

        [Fact]
        public void GetAvailableUnits_ReturnsOnlySatBesar_WhenSatKecilIsNullOrEmpty()
        {
            var barangNull = new Barang("BRG-003", "Beras 5kg", satBesar: "Sak", satKecil: null);
            var barangEmpty = new Barang("BRG-004", "Beras 10kg", satBesar: "Sak", satKecil: "   ");

            var unitsNull = barangNull.GetAvailableUnits().ToList();
            var unitsEmpty = barangEmpty.GetAvailableUnits().ToList();

            Assert.Single(unitsNull);
            Assert.Equal("Sak", unitsNull[0]);

            Assert.Single(unitsEmpty);
            Assert.Equal("Sak", unitsEmpty[0]);
        }

        [Fact]
        public void GetAvailableUnits_ReturnsOnlySatKecil_WhenSatBesarIsNullOrEmpty()
        {
            var barangNull = new Barang("BRG-005", "Permen", satBesar: null, satKecil: "Btl");
            var barangEmpty = new Barang("BRG-006", "Permen Stick", satBesar: "", satKecil: "Btl");

            var unitsNull = barangNull.GetAvailableUnits().ToList();
            var unitsEmpty = barangEmpty.GetAvailableUnits().ToList();

            Assert.Single(unitsNull);
            Assert.Equal("Btl", unitsNull[0]);

            Assert.Single(unitsEmpty);
            Assert.Equal("Btl", unitsEmpty[0]);
        }

        [Fact]
        public void GetAvailableUnits_ReturnsEmptySequence_WhenBothAreNullOrWhitespace()
        {
            var barangNull = new Barang("BRG-007", "Sample Product", satBesar: null, satKecil: null);
            var barangWhitespace = new Barang("BRG-008", "Sample Product 2", satBesar: " ", satKecil: "\t\r\n");

            Assert.Empty(barangNull.GetAvailableUnits());
            Assert.Empty(barangWhitespace.GetAvailableUnits());
        }
    }
}
