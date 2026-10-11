using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.Infrastructure.Helpers;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class RegistryHelperTests
    {
        [Fact]
        public void RegistryHelper_CanReadAndWriteWorkstationConfiguration()
        {
            var helper = new RegistryHelper();

            // Save original values
            var origWarehouse = helper.WarehouseCode;
            var origServerId = helper.ServerTargetId;
            var origOperator = helper.LastOperatorName;
            var origSound = helper.EnableSoundFeedback;

            try
            {
                helper.WarehouseCode = "GDG-TEST-01";
                helper.ServerTargetId = "SRV-TEST-99";
                helper.LastOperatorName = "Operator Testing";
                helper.EnableSoundFeedback = false;

                Assert.Equal("GDG-TEST-01", helper.WarehouseCode);
                Assert.Equal("SRV-TEST-99", helper.ServerTargetId);
                Assert.Equal("Operator Testing", helper.LastOperatorName);
                Assert.False(helper.EnableSoundFeedback);
                Assert.False(AudioHelper.EnableSoundFeedback);

                // Static reads match
                Assert.Equal("GDG-TEST-01", RegistryHelper.ReadString(RegistryHelper.KeyWarehouseCode));
                Assert.Equal("SRV-TEST-99", RegistryHelper.ReadString(RegistryHelper.KeyServerTargetId));
                Assert.Equal("Operator Testing", RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorName));
                Assert.False(RegistryHelper.ReadBool(RegistryHelper.KeyEnableSoundFeedback));

                // Re-enable sound
                helper.EnableSoundFeedback = true;
                Assert.True(helper.EnableSoundFeedback);
                Assert.True(AudioHelper.EnableSoundFeedback);
            }
            finally
            {
                // Restore originals
                helper.WarehouseCode = origWarehouse;
                helper.ServerTargetId = origServerId;
                helper.LastOperatorName = origOperator;
                helper.EnableSoundFeedback = origSound;
            }
        }

        [Fact]
        public void RegistryHelper_ImplementsIRegistryHelper()
        {
            IRegistryHelper contract = new RegistryHelper();
            Assert.NotNull(contract);
            Assert.NotNull(contract.WarehouseCode);
        }
    }
}
