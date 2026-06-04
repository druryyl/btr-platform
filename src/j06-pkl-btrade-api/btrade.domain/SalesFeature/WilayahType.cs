using btrade.domain.SalesFeature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.domain.SalesFeature;

public record WilayahType(string WilayahId, string WilayahName, string ServerId) : IWilayahKey;

public interface IWilayahKey : IServerId
{
    string WilayahId { get; }
}
