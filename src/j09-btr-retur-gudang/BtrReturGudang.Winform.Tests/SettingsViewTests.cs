using System;
using System.Threading;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.Infrastructure.Helpers;
using BtrReturGudang.Winform.Views;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class MockRegistryHelper : IRegistryHelper
    {
        public string WarehouseCode { get; set; } = "GDG-TEST";
        public string ServerTargetId { get; set; } = "SRV-TEST";
        public string LastOperatorName { get; set; } = "Operator Test";
        public bool EnableSoundFeedback { get; set; } = true;
    }

    public class MockAudioHelper : IAudioHelper
    {
        public bool EnableSoundFeedback { get; set; } = true;
        public int SuccessBeepCount { get; private set; } = 0;
        public int WarningBeepCount { get; private set; } = 0;

        public void PlaySuccess()
        {
            SuccessBeepCount++;
        }

        public void PlayWarning()
        {
            WarningBeepCount++;
        }
    }

    public class SettingsViewTests
    {
        private static void ExecuteInStaThread(Action action)
        {
            Exception exception = null;
            var thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                throw new AggregateException("STA thread execution failed", exception);
            }
        }

        [Fact]
        public void SettingsView_InitializesAndLoadsWorkstationConfig()
        {
            ExecuteInStaThread(() =>
            {
                var mockReg = new MockRegistryHelper();
                var mockAudio = new MockAudioHelper();

                using (var view = new SettingsView(mockReg, mockAudio))
                {
                    Assert.NotNull(view.WarehouseCodeBox);
                    Assert.NotNull(view.ServerTargetIdBox);
                    Assert.NotNull(view.PcNameBox);
                    Assert.NotNull(view.OperatorLabel);
                    Assert.NotNull(view.ConnStringBox);
                    Assert.NotNull(view.EnableAudioCheckBox);
                    Assert.NotNull(view.SaveAllButton);

                    // Assert loaded values from registry
                    Assert.Equal("GDG-TEST", view.WarehouseCodeBox.Text);
                    Assert.Equal("SRV-TEST", view.ServerTargetIdBox.Text);
                    Assert.Equal("Operator Test", view.OperatorLabel.Text);
                    Assert.True(view.EnableAudioCheckBox.Checked);
                    Assert.Equal(Environment.MachineName, view.PcNameBox.Text);
                }
            });
        }

        [Fact]
        public void SettingsView_ToggleAudioFeedback_UpdatesRegistryAndAudioHelper()
        {
            ExecuteInStaThread(() =>
            {
                var mockReg = new MockRegistryHelper();
                var mockAudio = new MockAudioHelper();

                using (var view = new SettingsView(mockReg, mockAudio))
                {
                    // Toggle off
                    view.EnableAudioCheckBox.Checked = false;
                    Assert.False(mockReg.EnableSoundFeedback);
                    Assert.False(mockAudio.EnableSoundFeedback);

                    // Toggle on
                    view.EnableAudioCheckBox.Checked = true;
                    Assert.True(mockReg.EnableSoundFeedback);
                    Assert.True(mockAudio.EnableSoundFeedback);
                }
            });
        }

        [Fact]
        public void SettingsView_TestAudioButtons_TriggersAudioCues()
        {
            ExecuteInStaThread(() =>
            {
                var mockReg = new MockRegistryHelper();
                var mockAudio = new MockAudioHelper();

                using (var view = new SettingsView(mockReg, mockAudio))
                {
                    view.TestSuccessBeepButton.PerformClick();
                    Assert.Equal(1, mockAudio.SuccessBeepCount);

                    view.TestWarningBeepButton.PerformClick();
                    Assert.Equal(1, mockAudio.WarningBeepCount);
                }
            });
        }

        [Fact]
        public void SettingsView_SaveAll_PersistsWorkstationConfig()
        {
            ExecuteInStaThread(() =>
            {
                var mockReg = new MockRegistryHelper();
                var mockAudio = new MockAudioHelper();

                using (var view = new SettingsView(mockReg, mockAudio))
                {
                    view.WarehouseCodeBox.Text = "GDG-SURABAYA";
                    view.ServerTargetIdBox.Text = "SRV-SBY-01";
                    view.EnableAudioCheckBox.Checked = false;
                    view.SuppressMessageBox = true;

                    view.SaveAllButton.PerformClick();

                    Assert.Equal("GDG-SURABAYA", mockReg.WarehouseCode);
                    Assert.Equal("SRV-SBY-01", mockReg.ServerTargetId);
                    Assert.False(mockReg.EnableSoundFeedback);
                }
            });
        }
    }
}
