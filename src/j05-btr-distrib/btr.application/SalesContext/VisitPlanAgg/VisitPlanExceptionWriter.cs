using System;
using System.Linq;
using btr.application.SalesContext.VisitPlanAgg.Services;
using btr.application.SupportContext.TglJamAgg;
using btr.domain.SalesContext.VisitPlanAgg;
using btr.nuna.Application;
using btr.nuna.Domain;

namespace btr.application.SalesContext.VisitPlanAgg
{
    public class VisitPlanExceptionWriter : IVisitPlanExceptionWriter
    {
        private readonly IVisitPlanExceptionDal _exceptionDal;
        private readonly IVisitPlanDal _visitPlanDal;
        private readonly IEffectiveVisitPlanResolver _resolver;
        private readonly ITglJamDal _tglJamDal;

        public VisitPlanExceptionWriter(
            IVisitPlanExceptionDal exceptionDal,
            IVisitPlanDal visitPlanDal,
            IEffectiveVisitPlanResolver resolver,
            ITglJamDal tglJamDal)
        {
            _exceptionDal = exceptionDal;
            _visitPlanDal = visitPlanDal;
            _resolver = resolver;
            _tglJamDal = tglJamDal;
        }

        public VisitPlanExceptionModel Save(VisitPlanExceptionModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            ValidateFutureDate(model.VisitDate);
            ValidateModel(model);

            var existing = (_exceptionDal.ListData(new VisitPlanDateFilter(model.SalesPersonId, model.VisitDate))
                            ?? Enumerable.Empty<VisitPlanExceptionModel>())
                .ToList();

            if (existing.Any(x =>
                    string.Equals(x.ExceptionType, model.ExceptionType, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.CustomerId, model.CustomerId, StringComparison.OrdinalIgnoreCase) &&
                    x.VisitPlanExceptionId != model.VisitPlanExceptionId))
            {
                throw new InvalidOperationException(
                    "A conflicting exception already exists for this customer on the selected date.");
            }

            ValidateBusinessRules(model, existing);

            model.CreatedAt = _tglJamDal.Now;

            using (var trans = TransHelper.NewScope())
            {
                _exceptionDal.Insert(model);
                trans.Complete();
            }

            return model;
        }

        public void Delete(IVisitPlanExceptionKey key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            var existing = _exceptionDal.GetData(key);
            if (existing == null)
                return;

            ValidateFutureDate(existing.VisitDate);

            using (var trans = TransHelper.NewScope())
            {
                _exceptionDal.Delete(key);
                trans.Complete();
            }
        }

        private void ValidateFutureDate(DateTime visitDate)
        {
            if (visitDate.Date < _tglJamDal.Now.Date)
                throw new InvalidOperationException("Exceptions cannot be created or modified for past dates.");
        }

        private static void ValidateModel(VisitPlanExceptionModel model)
        {
            if (string.IsNullOrWhiteSpace(model.SalesPersonId))
                throw new ArgumentException("Sales person is required.", nameof(model));

            if (string.IsNullOrWhiteSpace(model.CustomerId))
                throw new ArgumentException("Customer is required.", nameof(model));

            if (string.IsNullOrWhiteSpace(model.ExceptionType))
                throw new ArgumentException("Exception type is required.", nameof(model));

            if (string.IsNullOrWhiteSpace(model.CreatedByUserId))
                throw new ArgumentException("Created by user is required.", nameof(model));

            if (string.Equals(model.ExceptionType, VisitPlanExceptionTypeEnum.Replace.ToString(),
                    StringComparison.OrdinalIgnoreCase) &&
                string.IsNullOrWhiteSpace(model.ReplacementCustomerId))
            {
                throw new ArgumentException("Replacement customer is required for Replace exceptions.", nameof(model));
            }
        }

        private void ValidateBusinessRules(VisitPlanExceptionModel model, System.Collections.Generic.List<VisitPlanExceptionModel> existing)
        {
            var basePlan = (_visitPlanDal.ListData(new VisitPlanDateFilter(model.SalesPersonId, model.VisitDate))
                            ?? Enumerable.Empty<VisitPlanModel>())
                .ToList();

            var effectiveBefore = _resolver.Resolve(basePlan, existing).ToList();

            if (string.Equals(model.ExceptionType, VisitPlanExceptionTypeEnum.Replace.ToString(),
                    StringComparison.OrdinalIgnoreCase))
            {
                var onPlan = effectiveBefore.Any(x => x.CustomerId == model.CustomerId);
                var priorAdd = existing.Any(x =>
                    string.Equals(x.ExceptionType, VisitPlanExceptionTypeEnum.Add.ToString(),
                        StringComparison.OrdinalIgnoreCase) &&
                    x.CustomerId == model.CustomerId);

                if (!onPlan && !priorAdd)
                {
                    throw new InvalidOperationException(
                        "Replace source customer must exist on the effective plan for the selected date.");
                }

                if (effectiveBefore.Any(x => x.CustomerId == model.ReplacementCustomerId))
                {
                    throw new InvalidOperationException(
                        "Replacement customer is already on the effective plan for the selected date.");
                }
            }

            if (string.Equals(model.ExceptionType, VisitPlanExceptionTypeEnum.Add.ToString(),
                    StringComparison.OrdinalIgnoreCase) &&
                effectiveBefore.Any(x => x.CustomerId == model.CustomerId))
            {
                throw new InvalidOperationException(
                    "Customer is already on the effective plan for the selected date.");
            }
        }
    }
}
