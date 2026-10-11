namespace BtrReturGudang.AppTier.Contracts
{
    /// <summary>
    /// Contract for reading and persisting workstation configuration under HKCU\Software\BTRApp\ReturGudang.
    /// Traceable to: ARCHITECTURE §5.1, §7, P2-S06.
    /// </summary>
    public interface IRegistryHelper
    {
        string WarehouseCode { get; set; }
        string ServerTargetId { get; set; }
        string LastOperatorName { get; set; }
        bool EnableSoundFeedback { get; set; }
    }
}
