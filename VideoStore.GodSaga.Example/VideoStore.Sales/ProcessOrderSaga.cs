using System.Collections.Generic;
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
        IHandleMessages<ClientBecamePreferred>,
    IHandleTimeouts<ProcessOrderSaga.BuyersRemorseIsOver>
                                    
    {
        private readonly IDictionary<string, string> videoIdToUrlMap = new Dictionary<string, string>
            {
                {"intro1", "http://youtu.be/6lYF83wKerA"},
                {"intro2", "http://youtu.be/icze_WCyEdQ"},
                {"gems", "http://skillsmatter.com/podcast/design-architecture/hidden-nservicebus-gems"},
                {"integ", "http://skillsmatter.com/podcast/home/reliable-integrations-nservicebus/js-1877"},
                {"shiny", "http://skillsmatter.com/podcast/open-source-dot-net/nservicebus-3"},
                {"day", "http://dnrtv.com/dnrtvplayer/player.aspx?ShowNum=0199"},
                {"need", "http://vimeo.com/37728422"},
            };

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
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }

            Console.Out.WriteLine("Provision the videos and make the Urls available to the Content management for download ...[{0}] video(s) to provision", String.Join(", ", message.VideoIds));

            Bus.Reply(new ProvisionDownloadResponse
            {
                OrderNumber = message.OrderNumber,
                VideoIds = message.VideoIds,
                ClientId = message.ClientId
            });

            Data.DownloadRequestedOn = DateTime.Now;
            Console.Out.WriteLine("Download request received for #{0}", Data.OrderNumber);
        }

        public void Handle(OrderAccepted message)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }

            Console.Out.WriteLine("Order # {0} has been accepted, Let's provision the download -- Sending ProvisionDownloadRequest to the VideoStore.Operations endpoint", message.OrderNumber);

            //send out a request (a event will be published when the response comes back)
            Bus.Send<ProvisionDownloadRequest>(r =>
            {
                r.ClientId = message.ClientId;
                r.OrderNumber = message.OrderNumber;
                r.VideoIds = message.VideoIds;
            });
            
            Console.Out.WriteLine("Customer: {0} is now a preferred customer publishing for service concerns", message.ClientId);
            // publish this event as an asynchronous event
            Bus.Publish<ClientBecamePreferred>(m =>
            {
                m.ClientId = message.ClientId;
                m.PreferredStatusExpiresOn = DateTime.Now.AddMonths(2);
            });

            Data.OrderAcceptedOn = DateTime.Now;
            Console.Out.WriteLine("Order #{0} accepted", Data.OrderNumber);
        }

        public void Handle(ProvisionDownloadResponse message)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }

            Console.Out.WriteLine("Download for Order # {0} has been provisioned, Publishing Download ready event", message.OrderNumber);

            Bus.Publish<DownloadIsReady>(e =>
            {
                e.OrderNumber = message.OrderNumber;
                e.ClientId = message.ClientId;
                e.VideoUrls = new Dictionary<string, string>();

                foreach (var videoId in message.VideoIds)
                {
                    e.VideoUrls.Add(videoId, videoIdToUrlMap[videoId]);
                }
            });

            Console.Out.WriteLine("Downloads for Order #{0} is ready, publishing it.", message.OrderNumber);
            Data.DownloadRespondedOn = DateTime.Now;
            Console.Out.WriteLine("Download provisioned for #{0}", Data.OrderNumber);
        }

        public void Handle(ClientBecamePreferred message)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }
            Console.WriteLine("Handler WhenCustomerIsPreferredSendWelcomeEmail invoked for CustomerId: {0}", message.ClientId);

            // Don't write code to do the smtp send here, instead do a Bus.Send. If this handler fails, then 
            // the message to send email will not be sent.
            //Bus.Send<SendWelcomeEmail>(m => { m.ClientId = message.ClientId; });

            Console.WriteLine("Handler WhenCustomerIsPreferredSendLimitedTimeOffer invoked for CustomerId: {0}", message.ClientId);
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