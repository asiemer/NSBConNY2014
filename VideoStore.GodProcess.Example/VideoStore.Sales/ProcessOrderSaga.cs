﻿using System;
using System.Diagnostics;
using VideoStore.Common;
using VideoStore.Messages.Commands;
using VideoStore.Messages.Events;
using NServiceBus;
using NServiceBus.Saga;

namespace VideoStore.Sales
{
    public class ProcessOrderSaga : Saga<ProcessOrderSaga.OrderData>,
                                    IAmStartedByMessages<SubmitOrder>,
                                    IHandleMessages<CancelOrder>,
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
        }

        public class OrderData : ContainSagaData
        {
            [Unique]
            public virtual int OrderNumber { get; set; }
            public virtual string[] VideoIds { get; set; }
            public virtual string ClientId { get; set; }
        }

        public class BuyersRemorseIsOver
        {
        }
    }
}