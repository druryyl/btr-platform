using System;
using System.Diagnostics;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Contracts;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Policies;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Services;
using btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.Workers;
using btr.domain.SalesContext.SalesOmzetHealthWeeklyAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.SalesContext.SalesOmzetHealthWeeklyAgg.UseCases
{
    public interface IGenerateSalesOmzetHealthWeeklyWorker : INunaServiceVoid<GenerateSalesOmzetHealthWeeklyRequest> { }

    public class GenerateSalesOmzetHealthWeeklyWorker : IGenerateSalesOmzetHealthWeeklyWorker
    {
        private readonly IIsoWeekCalendar _isoWeekCalendar;
        private readonly ISalesOmzetHealthMetricsDal _metricsDal;
        private readonly ISalesOmzetHealthPolicy _healthPolicy;
        private readonly ISalesOmzetHealthWeeklyDal _weeklyDal;
        private readonly ISalesOmzetHealthWeeklyWriter _writer;

        public GenerateSalesOmzetHealthWeeklyWorker(
            IIsoWeekCalendar isoWeekCalendar,
            ISalesOmzetHealthMetricsDal metricsDal,
            ISalesOmzetHealthPolicy healthPolicy,
            ISalesOmzetHealthWeeklyDal weeklyDal,
            ISalesOmzetHealthWeeklyWriter writer)
        {
            _isoWeekCalendar = isoWeekCalendar;
            _metricsDal = metricsDal;
            _healthPolicy = healthPolicy;
            _weeklyDal = weeklyDal;
            _writer = writer;
        }

        public void Execute(GenerateSalesOmzetHealthWeeklyRequest request)
        {
            if (request is null)
                throw new ArgumentNullException(nameof(request));

            var sw = Stopwatch.StartNew();
            var (periodStart, periodEnd) = _isoWeekCalendar.GetWeekBounds(request.YearNumber, request.WeekNumber);
            var weekPeriode = new Periode(periodStart, periodEnd);

            SalesOmzetHealthWeeklyModel model;
            using (var trans = TransHelper.NewScope())
            {
                var metrics = _metricsDal.GetMetrics(weekPeriode);
                var score = _healthPolicy.ComputeScore(metrics, periodStart, periodEnd);
                var level = _healthPolicy.ResolveLevel(score);

                var existing = _weeklyDal.GetByYearWeek(request.YearNumber, request.WeekNumber);
                var now = DateTime.Now;

                if (existing is null)
                {
                    model = new SalesOmzetHealthWeeklyModel
                    {
                        YearNumber = request.YearNumber,
                        WeekNumber = request.WeekNumber,
                        CreatedAt = now
                    };
                }
                else
                {
                    model = existing;
                }

                model.PeriodStartDate = periodStart;
                model.PeriodEndDate = periodEnd;
                model.HealthLevel = level;
                model.HealthScore = score;
                model.MissingOrdersCount = metrics.MissingOrders;
                model.MissingFaktursCount = metrics.MissingDirectFakturs;
                model.UnlinkedFaktursCount = metrics.UnlinkedFakturs;
                model.StaleDataCount = metrics.StaleFakturEstimate;
                sw.Stop();
                model.CalculationDurationMs = (int)sw.ElapsedMilliseconds;
                model.LastCalculatedAt = now;
                model.UpdatedAt = now;

                _writer.Save(ref model);

                trans.Complete();

                request.Result = new GenerateSalesOmzetHealthWeeklyResult
                {
                    HealthWeeklyId = model.HealthWeeklyId,
                    HealthScore = model.HealthScore,
                    HealthLevel = model.HealthLevel.ToString(),
                    MissingOrdersCount = model.MissingOrdersCount,
                    MissingFaktursCount = model.MissingFaktursCount,
                    UnlinkedFaktursCount = model.UnlinkedFaktursCount,
                    StaleDataCount = model.StaleDataCount,
                    CalculationDurationMs = model.CalculationDurationMs
                };
            }
        }
    }
}
