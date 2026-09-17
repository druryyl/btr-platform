using btrade.domain.WarehouseFeature;
using Nuna.Lib.DataAccessHelper;

namespace btrade.application.Contract;

public interface IWarehouseMappingDal :
    IGetDataMayBe<WarehouseMappingType, IWarehouseMappingKey>
{
}
