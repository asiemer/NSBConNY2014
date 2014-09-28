using System;
using System.Collections.Generic;
using System.Diagnostics;
using NServiceBus;
using VideoStore.Common;
using VideoStore.Messages.Events;
using VideoStore.Messages.RequestResponse;

namespace VideoStore.Sales
{
    public class ProvisionDownloadResponseHandler : IHandleMessages<ProvisionDownloadResponse>
    {
        public IBus Bus { get; set; }

        public void Handle(ProvisionDownloadResponse message)
        {
            if (DebugFlagMutator.Debug)
            {
                Debugger.Break();
            }
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

            Console.Out.WriteLine("Download for Order # {0} has been provisioned, Publishing Download ready event", message.OrderNumber);
            Console.Out.WriteLine("Downloads for Order #{0} is ready, publishing it.", message.OrderNumber);
        }

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
    }
}
