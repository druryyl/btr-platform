using System;
using System.Threading.Tasks;
using BtrReturGudang.AppTier.Contracts;

namespace BtrReturGudang.Infrastructure.Helpers
{
    /// <summary>
    /// Native audio cues generator using Windows console multimedia beeps.
    /// Provides non-blocking sound feedback for scan outcomes without external media dependencies.
    /// Traceable to: TD-009, GAP-005, ARCHITECTURE §5.1, P2-S06.
    /// </summary>
    public class AudioHelper : IAudioHelper
    {
        private static volatile bool _enableSoundFeedback = true;

        /// <summary>
        /// Global toggle for sound feedback.
        /// </summary>
        public static bool EnableSoundFeedback
        {
            get => _enableSoundFeedback;
            set => _enableSoundFeedback = value;
        }

        bool IAudioHelper.EnableSoundFeedback
        {
            get => EnableSoundFeedback;
            set => EnableSoundFeedback = value;
        }

        /// <summary>
        /// Plays high-pitch short click (2000 Hz, 80ms) for successful scans.
        /// Non-blocking; executes asynchronously via Task.Run.
        /// </summary>
        public static void PlaySuccess()
        {
            if (!_enableSoundFeedback) return;

            Task.Run(() =>
            {
                try
                {
                    Console.Beep(2000, 80);
                }
                catch
                {
                    // Gracefully suppress on environments without audio support
                }
            });
        }

        /// <summary>
        /// Plays lower buzz tone (400 Hz, 200ms) for unrecognized / warning scans.
        /// Non-blocking; executes asynchronously via Task.Run.
        /// </summary>
        public static void PlayWarning()
        {
            if (!_enableSoundFeedback) return;

            Task.Run(() =>
            {
                try
                {
                    Console.Beep(400, 200);
                }
                catch
                {
                    // Gracefully suppress on environments without audio support
                }
            });
        }

        void IAudioHelper.PlaySuccess() => PlaySuccess();

        void IAudioHelper.PlayWarning() => PlayWarning();
    }
}
