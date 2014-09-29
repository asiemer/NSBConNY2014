using System;
using System.Diagnostics;
using NServiceBus;
using VideoStore.Common;
using VideoStore.Messages.Events;
using VideoStore.Messages.RequestResponse;

namespace VideoStore.Sales
{
    public class OrderAcceptedHandler : IHandleMessages<OrderAccepted>
    {
        public IBus Bus { get; set; }


        public void Handle(OrderAccepted message)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }

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

            Console.Out.WriteLine("Order # {0} has been accepted, Let's provision the download -- Sending ProvisionDownloadRequest to the VideoStore.Operations endpoint", message.OrderNumber);
        }
    }
}
