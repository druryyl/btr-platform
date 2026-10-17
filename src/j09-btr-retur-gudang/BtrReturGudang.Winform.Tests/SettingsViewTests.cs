using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Infrastructure.Helpers;
using BtrReturGudang.Winform.Views;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class MockRegistryHelper : IRegistryHelper
    {
        public string WarehouseCode { get; set; } = "GDG-TEST";
        public string ServerTargetId { get; set; } = "SRV-TEST";
        public string ApiBaseUrl { get; set; } = RegistryHelper.DefaultApiBaseUrl;
        public string LastOperatorName { get; set; } = "Operator Test";
        public string LastOperatorEmail { get; set; } = "operator.test@btr.co.id";
        public bool EnableSoundFeedback { get; set; } = true;
    }

    public class MockBtradeApiClient : IBtradeApiClient
    {
        public SessionResolveResultDto SessionResolveResult { get; set; } =
            SessionResolveResultDto.Ok(200, null, "USR-001", "Operator Test", "ROLE-1", null);

        public List<ReturnOrderDto> UploadedOrders { get; } = new List<ReturnOrderDto>();
        public List<BarcodeRegistrationDto> UploadedRegistrations { get; } = new List<BarcodeRegistrationDto>();

        public Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto dto)
        {
            UploadedOrders.Add(dto);
            return Task.FromResult(ApiUploadResultDto.Ok(200));
        }

        public Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto)
        {
            UploadedRegistrations.Add(dto);
            return Task.FromResult(ApiUploadResultDto.Ok(200));
        }

        public Task<MasterSyncDto> DownloadMasterDataAsync(string serverId) =>
            Task.FromResult(new MasterSyncDto());

        public Task<SessionResolveResultDto> ResolveSessionAsync(string email) =>
            Task.FromResult(SessionResolveResult);
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

        [Fact]
        public void SettingsView_LoadsAndDisplaysConfiguredOperatorGoogleEmail()
        {
            ExecuteInStaThread(() =>
            {
                var mockReg = new MockRegistryHelper { LastOperatorEmail = "operator.gudang@btr.co.id" };
                var mockAudio = new MockAudioHelper();

                using (var view = new SettingsView(mockReg, mockAudio))
                {
                    Assert.NotNull(view.OperatorEmailLabel);
                    Assert.NotNull(view.VerifyOperatorButton);
                    Assert.NotNull(view.SessionStatusLabel);

                    // TD-012: the configured operator Google email is the Cloud session actor and is displayed.
                    Assert.Equal("operator.gudang@btr.co.id", view.OperatorEmailLabel.Text);
                }
            });
        }

        [Fact]
        public void SettingsView_VerifyOperator_ResolvedIdentity_ShowsUserNameAndRole()
        {
            ExecuteInStaThread(() =>
            {
                var mockReg = new MockRegistryHelper { LastOperatorEmail = "operator.gudang@btr.co.id" };
                var mockAudio = new MockAudioHelper();
                var mockApi = new MockBtradeApiClient
                {
                    SessionResolveResult = SessionResolveResultDto.Ok(
                        200, null, "USR-777", "Budi Gudang", "ROLE-GDG", null)
                };

                using (var view = new SettingsView(mockReg, mockAudio, mockApi))
                {
                    view.VerifyOperatorButton.PerformClick();

                    Assert.Contains("Budi Gudang", view.SessionStatusLabel.Text);
                    Assert.Contains("ROLE-GDG", view.SessionStatusLabel.Text);
                }
            });
        }

        [Fact]
        public void SettingsView_VerifyOperator_Refusal_ShowsServerMessageVerbatim()
        {
            ExecuteInStaThread(() =>
            {
                const string refusal =
                    "Google account is not registered or is not active. Contact your administrator.";
                var mockReg = new MockRegistryHelper { LastOperatorEmail = "tidak.terdaftar@btr.co.id" };
                var mockAudio = new MockAudioHelper();
                var mockApi = new MockBtradeApiClient
                {
                    SessionResolveResult = SessionResolveResultDto.Fail(400, refusal)
                };

                using (var view = new SettingsView(mockReg, mockAudio, mockApi))
                {
                    view.VerifyOperatorButton.PerformClick();

                    // The Cloud refusal message is surfaced verbatim instead of an opaque HTTP 409.
                    Assert.Equal(refusal, view.SessionStatusLabel.Text);
                    Assert.DoesNotContain("409", view.SessionStatusLabel.Text);
                }
            });
        }
    }
}
