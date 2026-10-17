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
            var origOperatorEmail = helper.LastOperatorEmail;
            var origSound = helper.EnableSoundFeedback;

            try
            {
                helper.WarehouseCode = "GDG-TEST-01";
                helper.ServerTargetId = "SRV-TEST-99";
                helper.LastOperatorName = "Operator Testing";
                helper.LastOperatorEmail = "operator.testing@btr.co.id";
                helper.EnableSoundFeedback = false;

                Assert.Equal("GDG-TEST-01", helper.WarehouseCode);
                Assert.Equal("SRV-TEST-99", helper.ServerTargetId);
                Assert.Equal("Operator Testing", helper.LastOperatorName);
                Assert.Equal("operator.testing@btr.co.id", helper.LastOperatorEmail);
                Assert.False(helper.EnableSoundFeedback);
                Assert.False(AudioHelper.EnableSoundFeedback);

                // Static reads match
                Assert.Equal("GDG-TEST-01", RegistryHelper.ReadString(RegistryHelper.KeyWarehouseCode));
                Assert.Equal("SRV-TEST-99", RegistryHelper.ReadString(RegistryHelper.KeyServerTargetId));
                Assert.Equal("Operator Testing", RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorName));
                Assert.Equal("operator.testing@btr.co.id", RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorEmail));
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
                helper.LastOperatorEmail = origOperatorEmail;
                helper.EnableSoundFeedback = origSound;
            }
        }

        [Fact]
        public void RegistryHelper_LastOperatorEmail_RoundTripsUnderKeyPath()
        {
            var helper = new RegistryHelper();
            var original = helper.LastOperatorEmail;

            try
            {
                // P1-S02 / TD-012: the Google email is persisted and trimmed on read/write.
                helper.LastOperatorEmail = "  gudang.operator@btr.co.id  ";

                Assert.Equal("gudang.operator@btr.co.id", helper.LastOperatorEmail);
                Assert.Equal("gudang.operator@btr.co.id", RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorEmail));

                // Unset yields an empty string (never null).
                helper.LastOperatorEmail = null;
                Assert.Equal(string.Empty, helper.LastOperatorEmail);
                Assert.Equal(string.Empty, RegistryHelper.ReadString(RegistryHelper.KeyLastOperatorEmail));
            }
            finally
            {
                helper.LastOperatorEmail = original;
            }
        }

        [Fact]
        public void RegistryHelper_ImplementsIRegistryHelper()
        {
            IRegistryHelper contract = new RegistryHelper();
            Assert.NotNull(contract);
            Assert.NotNull(contract.WarehouseCode);
        }

        [Fact]
        public void RegistryHelper_DefaultsToJOGAndCloudApi_WhenEmptyOrWhitespace()
        {
            var helper = new RegistryHelper();
            var origServerId = helper.ServerTargetId;
            var origApiUrl = helper.ApiBaseUrl;

            try
            {
                helper.ServerTargetId = "";
                helper.ApiBaseUrl = "";

                Assert.Equal(RegistryHelper.DefaultServerTargetId, helper.ServerTargetId);
                Assert.Equal("JOG", helper.ServerTargetId);
                Assert.Equal(RegistryHelper.DefaultApiBaseUrl, helper.ApiBaseUrl);

                helper.ServerTargetId = "   ";
                helper.ApiBaseUrl = "   ";

                Assert.Equal("JOG", helper.ServerTargetId);
                Assert.Equal(RegistryHelper.DefaultApiBaseUrl, helper.ApiBaseUrl);
            }
            finally
            {
                helper.ServerTargetId = origServerId;
                helper.ApiBaseUrl = origApiUrl;
            }
        }
    }
}
