using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using btr.application.SupportContext.TglJamAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.infrastructure.Helpers;
using btr.infrastructure.SalesContext.VisitPlanAgg;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace btr.test.SalesContext
{
    public class VisitPlanDalDeleteFutureTest
    {
        private static readonly DateTime Today = new DateTime(2026, 6, 9);

        [Fact]
        [Trait("Category", "Integration")]
        public void DeleteFuture_WithPastFromDate_DoesNotRemoveHistoricalRows()
        {
            var options = Options.Create(new DatabaseOptions
            {
                ServerName = "JUDE7",
                DbName = "devTest",
                IsTest = true
            });
            var sut = new VisitPlanDal(options, new StubTglJamDal(Today));
            var connString = ConnStringHelper.Get(options.Value);
            var salesPersonId = "Z";
            var pastDate = Today.AddDays(-2);
            var futureDate = Today.AddDays(1);

            EnsureVisitPlanTable(connString);

            var pastRowId = InsertRow(connString, salesPersonId, pastDate, "C001");
            var futureRowId = InsertRow(connString, salesPersonId, futureDate, "C002");

            try
            {
                sut.DeleteFuture(salesPersonId, pastDate, futureDate);

                RowExists(connString, pastRowId).Should().BeTrue("past materialized rows must remain immutable");
                RowExists(connString, futureRowId).Should().BeFalse("future rows in the delete window should be removed");
            }
            finally
            {
                DeleteRow(connString, pastRowId);
                DeleteRow(connString, futureRowId);
            }
        }

        private static void EnsureVisitPlanTable(string connString)
        {
            const string sql = @"
                IF OBJECT_ID('dbo.BTR_VisitPlan', 'U') IS NULL
                BEGIN
                    CREATE TABLE dbo.BTR_VisitPlan
                    (
                        VisitPlanId     VARCHAR(26) NOT NULL,
                        SalesPersonId   VARCHAR(5)  NOT NULL,
                        VisitDate       DATE        NOT NULL,
                        CustomerId      VARCHAR(6)  NOT NULL,
                        NoUrut          INT         NOT NULL,
                        HariRuteId      VARCHAR(3)  NOT NULL,
                        PlanSource      VARCHAR(10) NOT NULL,
                        MaterializedAt  DATETIME    NOT NULL,
                        CONSTRAINT PK_BTR_VisitPlan PRIMARY KEY (VisitPlanId),
                        CONSTRAINT UX_BTR_VisitPlan UNIQUE (SalesPersonId, VisitDate, CustomerId)
                    );
                END";

            using (var conn = new SqlConnection(connString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private static string InsertRow(string connString, string salesPersonId, DateTime visitDate, string customerId)
        {
            var visitPlanId = Guid.NewGuid().ToString("N").Substring(0, 26);
            const string sql = @"
                INSERT INTO BTR_VisitPlan (
                    VisitPlanId, SalesPersonId, VisitDate, CustomerId,
                    NoUrut, HariRuteId, PlanSource, MaterializedAt)
                VALUES (
                    @VisitPlanId, @SalesPersonId, @VisitDate, @CustomerId,
                    1, 'H12', 'Template', @MaterializedAt)";

            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@VisitPlanId", SqlDbType.VarChar).Value = visitPlanId;
                cmd.Parameters.Add("@SalesPersonId", SqlDbType.VarChar).Value = salesPersonId;
                cmd.Parameters.Add("@VisitDate", SqlDbType.Date).Value = visitDate.Date;
                cmd.Parameters.Add("@CustomerId", SqlDbType.VarChar).Value = customerId;
                cmd.Parameters.Add("@MaterializedAt", SqlDbType.DateTime).Value = DateTime.Now;
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            return visitPlanId;
        }

        private static bool RowExists(string connString, string visitPlanId)
        {
            const string sql = "SELECT COUNT(1) FROM BTR_VisitPlan WHERE VisitPlanId = @VisitPlanId";
            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@VisitPlanId", SqlDbType.VarChar).Value = visitPlanId;
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        private static void DeleteRow(string connString, string visitPlanId)
        {
            const string sql = "DELETE FROM BTR_VisitPlan WHERE VisitPlanId = @VisitPlanId";
            using (var conn = new SqlConnection(connString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.Add("@VisitPlanId", SqlDbType.VarChar).Value = visitPlanId;
                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private sealed class StubTglJamDal : ITglJamDal
        {
            public StubTglJamDal(DateTime now) => Now = now;
            public DateTime Now { get; }
        }
    }
}
