﻿namespace VideoStore.ContentManagement
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Messages.Events;
    using Messages.RequestResponse;
    using NServiceBus;
    using Common;

    //public class ProvisionDownloadResponseHandler : IHandleMessages<ProvisionDownloadResponse>
    //{
    //    public IBus Bus { get; set; }
    //    private readonly IDictionary<string, string> videoIdToUrlMap = new Dictionary<string, string>
    //        {
    //            {"intro1", "http://youtu.be/6lYF83wKerA"},
    //            {"intro2", "http://youtu.be/icze_WCyEdQ"},
    //            {"gems", "http://skillsmatter.com/podcast/design-architecture/hidden-nservicebus-gems"},
    //            {"integ", "http://skillsmatter.com/podcast/home/reliable-integrations-nservicebus/js-1877"},
    //            {"shiny", "http://skillsmatter.com/podcast/open-source-dot-net/nservicebus-3"},
    //            {"day", "http://dnrtv.com/dnrtvplayer/player.aspx?ShowNum=0199"},
    //            {"need", "http://vimeo.com/37728422"},
    //        };

    //    public void Handle(ProvisionDownloadResponse message)
    //    {
    //        if (DebugFlagMutator.Debug)
    //        {
    //            Debugger.Break();
    //        }
    //    }
    //}
}
