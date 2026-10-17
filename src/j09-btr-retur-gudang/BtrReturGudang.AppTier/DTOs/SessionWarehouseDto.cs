namespace BtrReturGudang.AppTier.DTOs
{
    /// <summary>
    /// Warehouse mapping entry returned by the Cloud operator session resolution
    /// (<c>POST api/session/resolve</c>): the tenant-local location and its Cloud ServerId.
    /// Traceable to: ARCHITECTURE §5.1, §7, TD-012.
    /// </summary>
    public class SessionWarehouseDto
    {
        public string LocationId { get; set; }
        public string ServerId { get; set; }
    }
}
