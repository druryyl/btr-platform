using System.Threading.Tasks;
using BtrReturGudang.AppTier.DTOs;

namespace BtrReturGudang.AppTier.Contracts
{
    /// <summary>
    /// Service contract for Cloud API relay interactions (j06-pkl-btrade-api).
    /// Implemented by BtradeApiClient in BtrReturGudang.Infrastructure.
    /// Traceable to: ARCHITECTURE §5.1, §6, P2-S05.
    /// </summary>
    public interface IBtradeApiClient
    {
        Task<ApiUploadResultDto> UploadReturnOrderAsync(ReturnOrderDto dto);
        Task<ApiUploadResultDto> UploadBarcodeRegistrationAsync(BarcodeRegistrationDto dto);
        Task<MasterSyncDto> DownloadMasterDataAsync(string serverId);
        Task<SessionResolveResultDto> ResolveSessionAsync(string email);
    }
}
