using System;
using System.Collections.Generic;
using System.Linq;
using btr.domain.SalesContext.SalesPersonAgg;

namespace btr.application.ReportingContext.DashboardSnapshotAgg.Services
{
    public static class DashboardSalesmanKeyResolver
    {
        public static string ResolveSalesPersonId(string salesPersonId, string salesPersonName)
        {
            if (!string.IsNullOrWhiteSpace(salesPersonId))
                return salesPersonId.Trim();

            return string.Empty;
        }

        public static SalesmanLookup BuildLookup(IEnumerable<SalesPersonModel> salespeople)
        {
            var masterById = new Dictionary<string, SalesPersonModel>(StringComparer.OrdinalIgnoreCase);
            var nameToId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var person in salespeople ?? Enumerable.Empty<SalesPersonModel>())
            {
                if (string.IsNullOrWhiteSpace(person.SalesPersonId))
                    continue;

                var id = person.SalesPersonId.Trim();
                masterById[id] = person;

                if (!string.IsNullOrWhiteSpace(person.SalesPersonName) &&
                    !nameToId.ContainsKey(person.SalesPersonName.Trim()))
                {
                    nameToId[person.SalesPersonName.Trim()] = id;
                }
            }

            return new SalesmanLookup(masterById, nameToId);
        }

        public static string ResolveId(
            string salesPersonId,
            string salesPersonName,
            SalesmanLookup lookup)
        {
            var id = ResolveSalesPersonId(salesPersonId, salesPersonName);
            if (id.Length > 0)
                return id;

            if (!string.IsNullOrWhiteSpace(salesPersonName) &&
                lookup.NameToId.TryGetValue(salesPersonName.Trim(), out var resolved))
            {
                return resolved;
            }

            return string.Empty;
        }

        public sealed class SalesmanLookup
        {
            public SalesmanLookup(
                Dictionary<string, SalesPersonModel> masterById,
                Dictionary<string, string> nameToId)
            {
                MasterById = masterById;
                NameToId = nameToId;
            }

            public Dictionary<string, SalesPersonModel> MasterById { get; }

            public Dictionary<string, string> NameToId { get; }
        }
    }
}
