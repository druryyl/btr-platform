using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.AppTier.DTOs;
using BtrReturGudang.Infrastructure.Helpers;
using BtrReturGudang.Wpf.ViewModels;
using Xunit;

namespace BtrReturGudang.Wpf.Tests
{
    public class SettingsViewModelTests
    {
        #region Test Fakes

        private class FakeRegistryHelper : IRegistryHelper
        {
            public string WarehouseCode { get; set; } = "GDG-SBY";
            public string ServerTargetId { get; set; } = "SRV-SBY";
            public string ApiBaseUrl { get; set; } = "http://192.168.1.50:5000/";
            public string LastOperatorName { get; set; } = "Joko Susilo";
            public string LastOperatorEmail { get; set; } = "joko@btr.co.id";
            public bool EnableSoundFeedback { get; set; } = true;
        }

        private class FakeAudioHelper : IAudioHelper
        {
            public bool EnableSoundFeedback { get; set; } = true;
            public int SuccessCount { get; private set; }
            public int WarningCount { get; private set; }

            public void PlaySuccess() => SuccessCount++;
            public void PlayWarning() => WarningCount++;
        }

        private class FakeBtradeApiClient : IBtradeApiClient
        {
            public bool ShouldSucceed { get; set; } = true;
            public SessionResolveResultDto ResolveResult { get; set; } =
                SessionResolveResultDto.Ok(200, null, "USR-101", "Joko Susilo", "OPERATOR", null);

            public Task<SessionResolveResultDto> ResolveSessionAsync(string email) =>
                Task.FromResult(ShouldSucceed ? ResolveResult : SessionResolveResultDto.Fail(400, "Refusal message", "Akun Google tidak aktif"));

            public Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto dto) =>
                Task.FromResult(ApiUploadResultDto.Ok(200));

            public Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto) =>
                Task.FromResult(ApiUploadResultDto.Ok(200));

            public Task<MasterSyncDto> DownloadMasterDataAsync(string serverId) =>
                Task.FromResult(new MasterSyncDto());
        }

        #endregion

        [Fact]
        public void SettingsViewModel_LoadsConfigurationFromRegistry()
        {
            var reg = new FakeRegistryHelper();
            var audio = new FakeAudioHelper();
            var vm = new SettingsViewModel(reg, audio);

            Assert.Equal("GDG-SBY", vm.WarehouseCode);
            Assert.Equal("SRV-SBY", vm.ServerTargetId);
            Assert.Equal("http://192.168.1.50:5000/", vm.ApiBaseUrl);
            Assert.Equal("Joko Susilo", vm.OperatorName);
            Assert.Equal("joko@btr.co.id", vm.OperatorEmail);
            Assert.True(vm.EnableSoundFeedback);
        }

        [Fact]
        public void SettingsViewModel_SavesConfigurationToRegistry()
        {
            var reg = new FakeRegistryHelper();
            var audio = new FakeAudioHelper();
            var vm = new SettingsViewModel(reg, audio);

            vm.WarehouseCode = "GDG-BDG";
            vm.ServerTargetId = "SRV-BDG";
            vm.ApiBaseUrl = "http://api.btr.co.id/staging";
            vm.OperatorName = "Bambang";
            vm.OperatorEmail = "bambang@btr.co.id";
            vm.EnableSoundFeedback = false;

            vm.SaveAllCommand.Execute(null);

            Assert.Equal("GDG-BDG", reg.WarehouseCode);
            Assert.Equal("SRV-BDG", reg.ServerTargetId);
            Assert.Equal("http://api.btr.co.id/staging/", reg.ApiBaseUrl);
            Assert.Equal("Bambang", reg.LastOperatorName);
            Assert.Equal("bambang@btr.co.id", reg.LastOperatorEmail);
            Assert.False(reg.EnableSoundFeedback);
            Assert.Equal(1, audio.SuccessCount);
            Assert.Contains("berhasil disimpan", vm.StatusMessage);
        }

        [Fact]
        public async Task SettingsViewModel_VerifiesOperatorSessionSuccessfully()
        {
            var reg = new FakeRegistryHelper();
            var audio = new FakeAudioHelper();
            var api = new FakeBtradeApiClient { ShouldSucceed = true };

            var vm = new SettingsViewModel(reg, audio, api);
            await vm.VerifyOperatorAsync();

            Assert.Contains("✅ Operator terverifikasi: Joko Susilo", vm.OperatorSessionStatus);
            Assert.Equal(1, audio.SuccessCount);
        }

        [Fact]
        public async Task SettingsViewModel_HandlesOperatorSessionRefusal()
        {
            var reg = new FakeRegistryHelper();
            var audio = new FakeAudioHelper();
            var api = new FakeBtradeApiClient { ShouldSucceed = false };

            var vm = new SettingsViewModel(reg, audio, api);
            await vm.VerifyOperatorAsync();

            Assert.Contains("Akun Google tidak aktif", vm.OperatorSessionStatus);
            Assert.Equal(1, audio.WarningCount);
        }

        [Fact]
        public async Task SettingsViewModel_SwitchOperatorDialog_UpdatesOperator()
        {
            var reg = new FakeRegistryHelper();
            var audio = new FakeAudioHelper();
            var api = new FakeBtradeApiClient
            {
                ResolveResult = SessionResolveResultDto.Ok(200, null, "USR-202", "Siti Aminah", "SUPERVISOR", null)
            };

            var vm = new SettingsViewModel(reg, audio, api);
            vm.SwitchOperatorDialogFunc = () => Tuple.Create("Siti Aminah", "siti@btr.co.id");

            await vm.SwitchOperatorAsync();

            Assert.Equal("Siti Aminah", vm.OperatorName);
            Assert.Equal("siti@btr.co.id", vm.OperatorEmail);
            Assert.Contains("✅ Operator terverifikasi: Siti Aminah", vm.OperatorSessionStatus);
        }

        [Fact]
        public void SettingsViewModel_SoundToggle_UpdatesAudioHelper()
        {
            var reg = new FakeRegistryHelper();
            var audio = new FakeAudioHelper();
            var vm = new SettingsViewModel(reg, audio);

            vm.EnableSoundFeedback = false;
            Assert.False(reg.EnableSoundFeedback);
            Assert.False(audio.EnableSoundFeedback);

            vm.EnableSoundFeedback = true;
            Assert.True(reg.EnableSoundFeedback);
            Assert.True(audio.EnableSoundFeedback);
        }
    }
}
