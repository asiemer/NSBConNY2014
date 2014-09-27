using VideoStore.Messages.RequestResponse;

namespace VideoStore.Sales
{
    using System;
    using System.Diagnostics;
    using Common;
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Saga;

    public class ProcessOrderSaga : Saga<ProcessOrderSaga.OrderData>,
                                    IAmStartedByMessages<SubmitOrder>,
                                    IHandleMessages<CancelOrder>,
                                    IHandleMessages<OrderAccepted>,
                                    IHandleMessages<ProvisionDownloadResponse>,
                                    IHandleMessages<ProvisionDownloadRequest>,
                                    IHandleMessages<OrderCancelled>,
                                    IHandleMessages<DownloadIsReady>,
                                    IHandleTimeouts<ProcessOrderSaga.BuyersRemorseIsOver>
                                    
    {
        public void Handle(SubmitOrder message)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }

            Data.OrderNumber = message.OrderNumber;
            Data.VideoIds = message.VideoIds;
            Data.ClientId = message.ClientId;

            //shortened cooling down period to in theory GO FASTER
            RequestTimeout(TimeSpan.FromSeconds(2), new BuyersRemorseIsOver());
            Console.Out.WriteLine("Starting cool down period for order #{0}.", Data.OrderNumber);
        }

        public void Timeout(BuyersRemorseIsOver state)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }

            Bus.Publish<OrderAccepted>(e =>
                {
                    e.OrderNumber = Data.OrderNumber;
                    e.VideoIds = Data.VideoIds;
                    e.ClientId = Data.ClientId;
                });

            Console.Out.WriteLine("Cooling down period for order #{0} has elapsed.", Data.OrderNumber);
        }

        public void Handle(ProvisionDownloadRequest message)
        {
            Data.DownloadRequestedOn = DateTime.Now;
            Console.Out.WriteLine("Download request received for #{0}", Data.OrderNumber);
        }

        public void Handle(OrderAccepted message)
        {
            Data.OrderAcceptedOn = DateTime.Now;
            Console.Out.WriteLine("Order #{0} accepted", Data.OrderNumber);
        }

        public void Handle(ProvisionDownloadResponse message)
        {
            Data.DownloadRespondedOn = DateTime.Now;
            Console.Out.WriteLine("Download provisioned for #{0}", Data.OrderNumber);
        }

        public void Handle(OrderCancelled message)
        {
            Data.OrderCancelledOn = DateTime.Now;
            Console.Out.WriteLine("Order #{0} was cancelled", Data.OrderNumber);
        }

        public void Handle(DownloadIsReady message)
        {
            Data.DownloadWasReadyAt = DateTime.Now;
            Console.Out.WriteLine("Download ready for #{0}", Data.OrderNumber);
            MarkAsComplete();
        }

        public void Handle(CancelOrder message)
        {
            if (DebugFlagMutator.Debug)
            {
                   Debugger.Break();
            }

            MarkAsComplete();

            Bus.Publish(Bus.CreateInstance<OrderCancelled>(o =>
                {
                    o.OrderNumber = message.OrderNumber;
                    o.ClientId = message.ClientId;
                }));

            Console.Out.WriteLine("Order #{0} was cancelled.", message.OrderNumber);
        }

        public override void ConfigureHowToFindSaga()
        {
            ConfigureMapping<SubmitOrder>(m => m.OrderNumber)
                .ToSaga(s=>s.OrderNumber);
            ConfigureMapping<CancelOrder>(m => m.OrderNumber)
                .ToSaga(s=>s.OrderNumber);
            ConfigureMapping<ProvisionDownloadRequest>(m => m.OrderNumber)
                .ToSaga(s => s.OrderNumber);
            ConfigureMapping<ProvisionDownloadResponse>(m => m.OrderNumber)
                .ToSaga(s => s.OrderNumber);
            ConfigureMapping<OrderCancelled>(m => m.OrderNumber)
                .ToSaga(s => s.OrderNumber);
            ConfigureMapping<OrderAccepted>(m => m.OrderNumber)
                .ToSaga(s => s.OrderNumber);
            ConfigureMapping<DownloadIsReady>(m => m.OrderNumber)
                .ToSaga(s => s.OrderNumber);
        }

        public class OrderData : ContainSagaData
        {
            [Unique]
            public virtual int OrderNumber { get; set; }
            public virtual string[] VideoIds { get; set; }
            public virtual string ClientId { get; set; }
            public virtual DateTime? DownloadRequestedOn { get; set; }
            public virtual DateTime? OrderAcceptedOn { get; set; }
            public virtual DateTime? DownloadRespondedOn { get; set; }
            public virtual DateTime? OrderCancelledOn { get; set; }
            public virtual DateTime? DownloadWasReadyAt { get; set; }
        }

        public class BuyersRemorseIsOver
        {
        }
    }
}