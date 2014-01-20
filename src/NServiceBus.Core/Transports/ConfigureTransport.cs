namespace NServiceBus.Transports
{
    using System;
    using Features;
    using Unicast.Transport;

    public abstract class ConfigureTransport<T> : Feature, IConfigureTransport<T> where T : TransportDefinition
    {

        public void Configure(Configurator config)
        {
            var connectionStringRetriever = new TransportConnectionStringRetriever();
            connectionStringRetriever.Override(config.Bootstrapper["transport.definesConnectionString"] as Func<string>);

            var connectionString = connectionStringRetriever.GetConnectionStringOrNull(config.Bootstrapper["transport.connectionStringName"] as string);

            if (connectionString == null && RequiresConnectionString)
            {
                throw new InvalidOperationException(String.Format(Message, GetConfigFileIfExists(), typeof(T).Name, ExampleConnectionStringForErrorMessage));
            }

            config.SettingsHolder.Set("NServiceBus.Transport.ConnectionString", connectionString);

            var selectedTransportDefinition = Activator.CreateInstance<T>();
            config.SettingsHolder.Set("NServiceBus.Transport.SelectedTransport", selectedTransportDefinition);
            config.RegisterInstance<TransportDefinition>(selectedTransportDefinition, DependencyLifecycle.SingleInstance);
            InternalConfigure(config);
        }

        protected abstract void InternalConfigure(Configurator config);

        protected abstract string ExampleConnectionStringForErrorMessage { get; }

        protected virtual bool RequiresConnectionString
        {
            get { return true; }
        }


        static string GetConfigFileIfExists()
        {
            return AppDomain.CurrentDomain.SetupInformation.ConfigurationFile ?? "App.config";
        }

        const string Message =
            @"No default connection string found in your config file ({0}) for the {1} Transport.

To run NServiceBus with {1} Transport you need to specify the database connectionstring.
Here is an example of what is required:
  
  <connectionStrings>
    <add name=""NServiceBus/Transport"" connectionString=""{2}"" />
  </connectionStrings>";

    }
}