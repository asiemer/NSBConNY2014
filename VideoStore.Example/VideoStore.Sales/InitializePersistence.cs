namespace VideoStore.Sales
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
