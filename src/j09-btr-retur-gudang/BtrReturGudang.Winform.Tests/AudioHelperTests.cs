using System.Threading;
using BtrReturGudang.AppTier.Contracts;
using BtrReturGudang.Infrastructure.Helpers;
using Xunit;

namespace BtrReturGudang.Winform.Tests
{
    public class AudioHelperTests
    {
        [Fact]
        public void AudioHelper_CanToggleEnableSoundFeedback()
        {
            var original = AudioHelper.EnableSoundFeedback;
            try
            {
                AudioHelper.EnableSoundFeedback = false;
                Assert.False(AudioHelper.EnableSoundFeedback);

                AudioHelper.EnableSoundFeedback = true;
                Assert.True(AudioHelper.EnableSoundFeedback);
            }
            finally
            {
                AudioHelper.EnableSoundFeedback = original;
            }
        }

        [Fact]
        public void AudioHelper_WhenDisabled_PlaySuccessAndWarningDoNotThrow()
        {
            var original = AudioHelper.EnableSoundFeedback;
            try
            {
                AudioHelper.EnableSoundFeedback = false;
                var exception = Record.Exception(() =>
                {
                    AudioHelper.PlaySuccess();
                    AudioHelper.PlayWarning();
                });

                Assert.Null(exception);
            }
            finally
            {
                AudioHelper.EnableSoundFeedback = original;
            }
        }

        [Fact]
        public void AudioHelper_WhenEnabled_PlaySuccessAndWarningExecuteGracefully()
        {
            var original = AudioHelper.EnableSoundFeedback;
            try
            {
                AudioHelper.EnableSoundFeedback = true;
                var exception = Record.Exception(() =>
                {
                    AudioHelper.PlaySuccess();
                    AudioHelper.PlayWarning();
                    Thread.Sleep(50);
                });

                Assert.Null(exception);
            }
            finally
            {
                AudioHelper.EnableSoundFeedback = original;
            }
        }

        [Fact]
        public void AudioHelper_ImplementsIAudioHelper()
        {
            IAudioHelper helper = new AudioHelper();
            helper.EnableSoundFeedback = true;
            Assert.True(helper.EnableSoundFeedback);

            var exception = Record.Exception(() =>
            {
                helper.PlaySuccess();
                helper.PlayWarning();
            });

            Assert.Null(exception);
        }
    }
}
