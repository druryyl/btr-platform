using System;
using System.Collections.Generic;
using btr.application.ReportingContext.DashboardSnapshotAgg.Models;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Contracts
{
    public interface IPiutangOpenBalanceDal
    {
        IReadOnlyList<PiutangOpenBalanceDto> ListOpenBalances();
    }
}
