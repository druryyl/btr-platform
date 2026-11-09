using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using btr.application.InventoryContext.ReturJualAgg.Contracts;
using btr.application.InventoryContext.ReturnOrderAgg;
using btr.distrib.InventoryContext.ReturnOrderAgg;
using btr.domain.InventoryContext.ReturJualAgg;
using btr.domain.InventoryContext.ReturnOrderAgg;
using btr.nuna.Domain;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace btr.test.InventoryContext
{
    public class ListReturnOrderFormTest
    {
        [Fact]
        public void Constructor_InstantiatesSuccessfully_AndInitializesControls()
        {
            var stubMediator = new StubMediator();
            var stubReturJualDal = new StubReturJualDal();

            using (var form = new ListReturnOrderForm(stubMediator, stubReturJualDal))
            {
                form.Should().NotBeNull();
                form.Text.Should().Be("Monitoring Order Retur");

                var startDatePicker = form.Controls.Find("PeriodeStartDatePicker", true).FirstOrDefault() as DateTimePicker;
                var endDatePicker = form.Controls.Find("PeriodeEndDatePicker", true).FirstOrDefault() as DateTimePicker;
                var searchBox = form.Controls.Find("SearchTextBox", true).FirstOrDefault() as TextBox;
                var searchButton = form.Controls.Find("SearchButton", true).FirstOrDefault() as Button;
                var refreshButton = form.Controls.Find("RefreshButton", true).FirstOrDefault() as Button;
                var excelButton = form.Controls.Find("ExcelButton", true).FirstOrDefault() as Button;
                var orderGrid = form.Controls.Find("OrderGrid", true).FirstOrDefault() as DataGridView;

                startDatePicker.Should().NotBeNull();
                endDatePicker.Should().NotBeNull();
                searchBox.Should().NotBeNull();
                searchButton.Should().NotBeNull();
                refreshButton.Should().NotBeNull();
                excelButton.Should().NotBeNull();
                orderGrid.Should().NotBeNull();

                // Verification of date picker default range (7 days)
                (endDatePicker.Value.Date - startDatePicker.Value.Date).TotalDays.Should().BeInRange(6, 8);

                // Verification of OrderGrid column headers
                var colHeaders = orderGrid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).ToList();
                colHeaders.Should().Contain("No Order");
                colHeaders.Should().Contain("Tgl Order");
                colHeaders.Should().Contain("Customer");
                colHeaders.Should().Contain("Gudang");
                colHeaders.Should().Contain("Salesman");
                colHeaders.Should().Contain("Driver");
                colHeaders.Should().Contain("Status");
                colHeaders.Should().Contain("Item");
                colHeaders.Should().Contain("No Retur Bagus");
                colHeaders.Should().Contain("No Retur Rusak");
                colHeaders.Should().Contain("Catatan");
            }
        }

        [Fact]
        public async Task RefreshData_PopulatesOrderGrid_WithReturnedItems()
        {
            var sampleOrders = new List<ListReturnOrderDto>
            {
                new ListReturnOrderDto
                {
                    ReturnOrderId = "RO001",
                    ReturnOrderNo = "RO-2026-001",
                    ReturnOrderDate = DateTime.Today,
                    CustomerName = "Toko Makmur",
                    WarehouseName = "Gudang Pusat",
                    SalesPersonName = "Budi",
                    DriverName = "Joko",
                    Status = ReturnOrderStatus.Synced,
                    TotalItem = 2,
                    ReturJualBagusCode = "",
                    ReturJualRusakCode = "",
                    Note = "Sample 1"
                },
                new ListReturnOrderDto
                {
                    ReturnOrderId = "RO002",
                    ReturnOrderNo = "RO-2026-002",
                    ReturnOrderDate = DateTime.Today,
                    CustomerName = "Toko Sejahtera",
                    WarehouseName = "Gudang Retur",
                    SalesPersonName = "Santoso",
                    DriverName = "Rudi",
                    Status = ReturnOrderStatus.Imported,
                    TotalItem = 1,
                    ReturJualBagusCode = "RJ-2026-001",
                    ReturJualRusakCode = "",
                    Note = "Sample 2"
                }
            };

            var stubMediator = new StubMediator(sampleOrders);
            var stubReturJualDal = new StubReturJualDal();

            using (var form = new ListReturnOrderForm(stubMediator, stubReturJualDal))
            {
                await form.RefreshData();

                var orderGrid = form.Controls.Find("OrderGrid", true).First() as DataGridView;
                orderGrid.Should().NotBeNull();
                orderGrid.Rows.Count.Should().Be(2);

                var firstRowItem = orderGrid.Rows[0].DataBoundItem as ListReturnOrderDto;
                firstRowItem.Should().NotBeNull();
                firstRowItem.ReturnOrderNo.Should().Be("RO-2026-001");
                firstRowItem.Status.Should().Be(ReturnOrderStatus.Synced);

                var secondRowItem = orderGrid.Rows[1].DataBoundItem as ListReturnOrderDto;
                secondRowItem.Should().NotBeNull();
                secondRowItem.ReturnOrderNo.Should().Be("RO-2026-002");
                secondRowItem.Status.Should().Be(ReturnOrderStatus.Imported);
            }
        }

        [Fact]
        public void DependencyResolution_FromServiceProvider_ResolvesListReturnOrderForm()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IMediator>(new StubMediator());
            services.AddSingleton<IReturJualDal>(new StubReturJualDal());
            services.AddTransient<ListReturnOrderForm>();

            using (var sp = services.BuildServiceProvider())
            {
                using (var form = sp.GetService<ListReturnOrderForm>())
                {
                    form.Should().NotBeNull();
                }
            }
        }

        private sealed class StubMediator : IMediator
        {
            private readonly IEnumerable<ListReturnOrderDto> _returnOrders;

            public StubMediator(IEnumerable<ListReturnOrderDto> returnOrders = null)
            {
                _returnOrders = returnOrders ?? Enumerable.Empty<ListReturnOrderDto>();
            }

            public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
            {
                if (request is ListReturnOrderMonitoringQuery)
                {
                    return Task.FromResult((TResponse)_returnOrders);
                }
                return Task.FromResult(default(TResponse));
            }

            public Task<object> Send(object request, CancellationToken cancellationToken = default) => Task.FromResult<object>(null);
            public Task Publish(object notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
            public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default) where TNotification : INotification => Task.CompletedTask;
            public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public IAsyncEnumerable<object> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
            public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest => Task.CompletedTask;
        }

        private sealed class StubReturJualDal : IReturJualDal
        {
            public void Insert(ReturJualModel model) => throw new NotImplementedException();
            public void Update(ReturJualModel model) => throw new NotImplementedException();
            public void Delete(IReturJualKey key) => throw new NotImplementedException();
            public ReturJualModel GetData(IReturJualKey key) => null;
            public IEnumerable<ReturJualModel> ListData(Periode filter) => Enumerable.Empty<ReturJualModel>();
            public IEnumerable<ReturJualModel> ListByReturnOrderId(string returnOrderId) => Enumerable.Empty<ReturJualModel>();
        }
    }
}
