using System;
using System.Diagnostics;
using NServiceBus;
using VideoStore.Common;
using VideoStore.Messages.Events;

namespace VideoStore.Sales
{
    public class ClientBecamePreferredHandler : IHandleMessages<ClientBecamePreferred>
    {
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
    }
}
