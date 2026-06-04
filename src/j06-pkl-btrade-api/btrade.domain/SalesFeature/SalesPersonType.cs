using btrade.domain.SalesFeature;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace btrade.domain.SalesFeature;

public record SalesPersonType(string SalesPersonId, string SalesPersonCode,
    string SalesPersonName, string Email, string ServerId) : ISalesPersonKey;

public interface ISalesPersonKey : IServerId
{
    string SalesPersonId { get; }
}