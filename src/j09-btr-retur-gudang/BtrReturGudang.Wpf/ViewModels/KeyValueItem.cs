namespace BtrReturGudang.Wpf.ViewModels
{
    /// <summary>
    /// Lightweight Key-Value presentation wrapper for comboboxes (Drivers, Salespersons).
    /// </summary>
    public class KeyValueItem
    {
        public string Id { get; }
        public string Name { get; }
        public string Display { get; }

        public KeyValueItem(string id, string name, string display = null)
        {
            Id = id;
            Name = name;
            Display = display;
        }

        public override string ToString() => Display ?? Name ?? string.Empty;
    }
}
