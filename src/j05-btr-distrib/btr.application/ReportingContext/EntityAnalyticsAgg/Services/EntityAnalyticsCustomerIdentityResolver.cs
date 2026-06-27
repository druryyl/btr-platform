using System;
using System.Collections.Generic;
using System.Linq;
using btr.application.ReportingContext.EntityAnalyticsAgg.Models;
using btr.domain.SalesContext.CustomerAgg;

namespace btr.application.ReportingContext.EntityAnalyticsAgg.Services
{
    public static class EntityAnalyticsCustomerIdentityResolver
    {
        public static IReadOnlyDictionary<string, EntityAnalyticsCustomerIdentity> BuildLookup(
            IEnumerable<CustomerModel> customers)
        {
            var lookup = new Dictionary<string, EntityAnalyticsCustomerIdentity>(StringComparer.OrdinalIgnoreCase);
            foreach (var customer in customers ?? Enumerable.Empty<CustomerModel>())
            {
                if (customer == null)
                    continue;

                var customerCode = customer.CustomerCode?.Trim();
                if (string.IsNullOrWhiteSpace(customerCode))
                    continue;

                if (lookup.ContainsKey(customerCode))
                    continue;

                var customerId = string.IsNullOrWhiteSpace(customer.CustomerId)
                    ? customerCode
                    : customer.CustomerId.Trim();

                lookup[customerCode] = new EntityAnalyticsCustomerIdentity
                {
                    CustomerId = customerId,
                    CustomerCode = customerCode,
                    CustomerName = customer.CustomerName?.Trim() ?? customerCode
                };
            }

            return lookup;
        }

        public static EntityAnalyticsCustomerIdentity Resolve(
            string customerCode,
            IReadOnlyDictionary<string, EntityAnalyticsCustomerIdentity> lookup,
            string customerId = null,
            string customerName = null)
        {
            var normalizedCode = customerCode?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(customerId))
            {
                return new EntityAnalyticsCustomerIdentity
                {
                    CustomerId = customerId.Trim(),
                    CustomerCode = normalizedCode,
                    CustomerName = EntityAnalyticsDisplayNameResolver.PreferBusinessName(
                        customerName,
                        null,
                        normalizedCode)
                };
            }

            if (!string.IsNullOrWhiteSpace(normalizedCode)
                && lookup != null
                && lookup.TryGetValue(normalizedCode, out var identity))
            {
                return new EntityAnalyticsCustomerIdentity
                {
                    CustomerId = identity.CustomerId,
                    CustomerCode = identity.CustomerCode,
                    CustomerName = EntityAnalyticsDisplayNameResolver.PreferBusinessName(
                        customerName,
                        identity.CustomerName,
                        identity.CustomerCode)
                };
            }

            return new EntityAnalyticsCustomerIdentity
            {
                CustomerId = normalizedCode,
                CustomerCode = normalizedCode,
                CustomerName = EntityAnalyticsDisplayNameResolver.PreferBusinessName(
                    customerName,
                    null,
                    normalizedCode)
            };
        }
    }
}
