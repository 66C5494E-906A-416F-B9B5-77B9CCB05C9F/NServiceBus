namespace NServiceBus
{
    using Config;
    using Hosting.Azure;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Serialization;
    using Transports;

    /// <summary>
    /// Configures windows azure storage queues as the underlying transport.
    /// </summary>
    public class WindowsAzureStorageQueueTransportConfigurer : IConfigureTransport<NServiceBus.WindowsAzureStorage>, IWantTheEndpointConfig
    {
        public void Configure(Configure config)
        {
            Address.SetParser<AzureAddress>();

            if (RoleEnvironment.IsAvailable && !IsHostedIn.ChildHostProcess())
            {
                config.AzureConfigurationSource();
            }

            if (!config.Configurer.HasComponent<IMessageSerializer>())
            {
                config.JsonSerializer();
            }

            config
                .AzureMessageQueue()
                .AzureSubcriptionStorage();

        }

        public IConfigureThisEndpoint Config { get; set; }
    }
}