using BtrReturGudang.Domain.ValueObjects;
using Xunit;

namespace BtrReturGudang.Domain.Tests
{
    public class BarcodeCommandTests
    {
        [Theory]
        [InlineData("CMD:BAGUS", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("CMD:QTY_BAGUS", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("$$BAGUS", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("BTR:QTY-BAGUS", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("BTR:QTY_BAGUS", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("CMD:RUSAK", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("CMD:QTY_RUSAK", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("$$RUSAK", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("BTR:QTY-RUSAK", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("BTR:QTY_RUSAK", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("CMD:F9", BarcodeCommandType.SaveDraft)]
        [InlineData("CMD:SIMPAN", BarcodeCommandType.SaveDraft)]
        [InlineData("CMD:SIMPAN_DRAFT", BarcodeCommandType.SaveDraft)]
        [InlineData("$$F9", BarcodeCommandType.SaveDraft)]
        [InlineData("CMD:ESC", BarcodeCommandType.ResetForm)]
        [InlineData("CMD:RESET", BarcodeCommandType.ResetForm)]
        [InlineData("CMD:RESET_FORM", BarcodeCommandType.ResetForm)]
        [InlineData("$$ESC", BarcodeCommandType.ResetForm)]
        [InlineData("CMD:ENTER", BarcodeCommandType.CommitItem)]
        [InlineData("CMD:TAMBAH", BarcodeCommandType.CommitItem)]
        [InlineData("$$ENTER", BarcodeCommandType.CommitItem)]
        [InlineData("CMD:F10", BarcodeCommandType.SaveAndUpload)]
        [InlineData("CMD:UPLOAD", BarcodeCommandType.SaveAndUpload)]
        [InlineData("$$F10", BarcodeCommandType.SaveAndUpload)]
        public void TryParse_AllValidCommandAliases_ReturnExpectedCommandType(string rawInput, BarcodeCommandType expectedType)
        {
            var success = BarcodeCommand.TryParse(rawInput, out BarcodeCommandType commandType);

            Assert.True(success);
            Assert.Equal(expectedType, commandType);

            // Also verify BarcodeCommand value object overload
            var successVo = BarcodeCommand.TryParse(rawInput, out BarcodeCommand vo);
            Assert.True(successVo);
            Assert.Equal(expectedType, vo.CommandType);
            Assert.True(vo.IsCommand);
        }

        [Theory]
        [InlineData("cmd:bagus", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("Cmd:Qty_Bagus", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("$$bagus", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("btr:qty-bagus", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("Cmd:Rusak", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("cmd:qty_rusak", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("CMD:f9", BarcodeCommandType.SaveDraft)]
        [InlineData("cmd:simpan", BarcodeCommandType.SaveDraft)]
        [InlineData("cmd:esc", BarcodeCommandType.ResetForm)]
        [InlineData("cmd:enter", BarcodeCommandType.CommitItem)]
        [InlineData("cmd:f10", BarcodeCommandType.SaveAndUpload)]
        [InlineData("cmd:upload", BarcodeCommandType.SaveAndUpload)]
        public void TryParse_IsCaseInsensitive(string rawInput, BarcodeCommandType expectedType)
        {
            var success = BarcodeCommand.TryParse(rawInput, out BarcodeCommandType commandType);

            Assert.True(success);
            Assert.Equal(expectedType, commandType);
        }

        [Theory]
        [InlineData("  CMD:BAGUS  ", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("CMD:RUSAK\r\n", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("\tCMD:F9\t\r\n", BarcodeCommandType.SaveDraft)]
        [InlineData("  $$ESC\r", BarcodeCommandType.ResetForm)]
        [InlineData("\nCMD:ENTER\n", BarcodeCommandType.CommitItem)]
        [InlineData(" \r\n\t CMD:F10 \t\r\n ", BarcodeCommandType.SaveAndUpload)]
        public void TryParse_IsResilientToWhitespaceCrLfAndTab(string rawInput, BarcodeCommandType expectedType)
        {
            var success = BarcodeCommand.TryParse(rawInput, out BarcodeCommandType commandType);

            Assert.True(success);
            Assert.Equal(expectedType, commandType);
        }

        [Theory]
        [InlineData("*CMD:BAGUS*", BarcodeCommandType.SwitchToQtyBagus)]
        [InlineData("*CMD:RUSAK*", BarcodeCommandType.SwitchToQtyRusak)]
        [InlineData("*CMD:F9*", BarcodeCommandType.SaveDraft)]
        [InlineData("*$$ESC*", BarcodeCommandType.ResetForm)]
        public void TryParse_Code39AsteriskWrapping_ParsesCorrectly(string rawInput, BarcodeCommandType expectedType)
        {
            var success = BarcodeCommand.TryParse(rawInput, out BarcodeCommandType commandType);

            Assert.True(success);
            Assert.Equal(expectedType, commandType);
        }

        [Theory]
        [InlineData("8998008152130")]
        [InlineData("1018013")]
        [InlineData("BR0182")]
        [InlineData("0012345678901")]
        [InlineData("UNKNOWN_COMMAND")]
        [InlineData("CMD:INVALID")]
        public void TryParse_ProductBarcodesAndUnknownInputs_ReturnFalseAndNone(string rawInput)
        {
            var success = BarcodeCommand.TryParse(rawInput, out BarcodeCommandType commandType);

            Assert.False(success);
            Assert.Equal(BarcodeCommandType.None, commandType);

            var successVo = BarcodeCommand.TryParse(rawInput, out BarcodeCommand vo);
            Assert.False(successVo);
            Assert.Equal(BarcodeCommandType.None, vo.CommandType);
            Assert.False(vo.IsCommand);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\r\n\t")]
        public void TryParse_NullOrEmptyOrWhitespace_ReturnFalseAndNone(string rawInput)
        {
            var success = BarcodeCommand.TryParse(rawInput, out BarcodeCommandType commandType);

            Assert.False(success);
            Assert.Equal(BarcodeCommandType.None, commandType);
        }

        [Fact]
        public void Parse_ValidCommand_ReturnsPopulatedValueObject()
        {
            var vo = BarcodeCommand.Parse("CMD:BAGUS\r\n");

            Assert.Equal(BarcodeCommandType.SwitchToQtyBagus, vo.CommandType);
            Assert.Equal("CMD:BAGUS\r\n", vo.RawValue);
            Assert.Equal("CMD:BAGUS", vo.NormalizedValue);
            Assert.True(vo.IsCommand);
            Assert.Contains("SwitchToQtyBagus", vo.ToString());
        }

        [Fact]
        public void Parse_NonCommand_ReturnsNoneValueObject()
        {
            var vo = BarcodeCommand.Parse("8998008152130");

            Assert.Equal(BarcodeCommandType.None, vo.CommandType);
            Assert.Equal("8998008152130", vo.RawValue);
            Assert.Equal("8998008152130", vo.NormalizedValue);
            Assert.False(vo.IsCommand);
            Assert.Equal("8998008152130", vo.ToString());
        }

        [Fact]
        public void ValueObject_EqualityAndHashCode_WorkCorrectly()
        {
            var cmd1 = new BarcodeCommand("CMD:BAGUS");
            var cmd2 = new BarcodeCommand("cmd:bagus\r\n");
            var cmd3 = new BarcodeCommand("CMD:RUSAK");

            Assert.Equal(cmd1, cmd2);
            Assert.True(cmd1 == cmd2);
            Assert.False(cmd1 != cmd2);
            Assert.Equal(cmd1.GetHashCode(), cmd2.GetHashCode());

            Assert.NotEqual(cmd1, cmd3);
            Assert.True(cmd1 != cmd3);
            Assert.False(cmd1 == cmd3);
        }

        [Fact]
        public void None_ReturnsDefaultNonCommandInstance()
        {
            var none = BarcodeCommand.None;

            Assert.Equal(BarcodeCommandType.None, none.CommandType);
            Assert.False(none.IsCommand);
            Assert.Equal(string.Empty, none.RawValue);
            Assert.Equal(string.Empty, none.NormalizedValue);
        }
    }
}
