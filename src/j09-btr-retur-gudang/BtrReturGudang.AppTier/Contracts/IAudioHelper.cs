namespace BtrReturGudang.AppTier.Contracts
{
    /// <summary>
    /// Contract for workstation audio cues.
    /// Traceable to: TD-009, GAP-005, ARCHITECTURE §5.1, P2-S06.
    /// </summary>
    public interface IAudioHelper
    {
        bool EnableSoundFeedback { get; set; }
        void PlaySuccess();
        void PlayWarning();
    }
}
