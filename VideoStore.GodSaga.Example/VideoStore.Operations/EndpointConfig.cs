namespace VideoStore.Operations
{
    using System;
    using NServiceBus;

	public class EndpointConfig : IConfigureThisEndpoint, AsA_Server, UsingTransport<Msmq>, IWantCustomInitialization
    {
        public void Init()
        {
            Configure.With()
                .DefaultBuilder()
                .UseNHibernateTimeoutPersister();
        }
    }
	
    public class MyClass : IWantToRunWhenBusStartsAndStops
    {
        public void Start()
        {
            Console.Out.WriteLine("The VideoStore.Operations endpoint is now started and ready to accept messages");
        }

        public void Stop()
        {

        }
    }
}
