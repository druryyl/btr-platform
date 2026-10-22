namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// Contract for child view models capable of intercepting global shell commands (F9, F10, Esc).
    /// Traceable to: ARCHITECTURE §4 TD-011, P2-S04.
    /// </summary>
    public interface IOrderActionHandler
    {
        void HandleSaveDraft();
        void HandleSaveAndUpload();
        void HandleCancelOrClear();
    }
}
