namespace VideoStore.Operations
{
    using NServiceBus;

    class InitializePersistence : INeedInitialization
    {
        public void Init()
        {
            Configure.Instance
                .UseNHibernateSubscriptionPersister() 
                .UseNHibernateTimeoutPersister() 
                .UseNHibernateSagaPersister(); 
        }
    }
}
