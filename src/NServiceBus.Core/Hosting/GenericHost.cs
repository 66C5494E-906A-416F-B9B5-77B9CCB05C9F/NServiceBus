namespace NServiceBus.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Configuration;
    using Helpers;
    using Installation;
    using Profiles;
    using Roles;
    using Settings;
    using Utils;
    using Wcf;

    /// <summary>
    ///     A generic host that can be used to provide hosting services in different environments
    /// </summary>
    public class GenericHost : IHost
    {
        /// <summary>
        ///     Accepts the type which will specify the users custom configuration.
        ///     This type should implement <see cref="IConfigureThisEndpoint" />.
        /// </summary>
        /// <param name="scannableAssembliesFullName">Assemblies full name that were scanned.</param>
        public GenericHost(IConfigureThisEndpoint specifier, string[] args, List<Type> defaultProfiles,
            string endpointName, IEnumerable<string> scannableAssembliesFullName = null)
        {
            this.specifier = specifier;

            if (String.IsNullOrEmpty(endpointName))
            {
                endpointName = specifier.GetType().Namespace ?? specifier.GetType().Assembly.GetName().Name;
            }

            Configure.GetEndpointNameAction = () => endpointName;
            Configure.DefineEndpointVersionRetriever = () => FileVersionRetriever.GetFileVersion(specifier.GetType());

            if (scannableAssembliesFullName == null)
            {
                var assemblyScanner = new AssemblyScanner();
                assemblyScanner.MustReferenceAtLeastOneAssembly.Add(typeof(IHandleMessages<>).Assembly);
                assembliesToScan = assemblyScanner
                    .GetScannableAssemblies()
                    .Assemblies;
            }
            else
            {
                assembliesToScan = scannableAssembliesFullName
                    .Select(Assembly.Load)
                    .ToList();
            }

            ProfileActivator.ProfileManager = new ProfileManager(assembliesToScan, specifier, args, defaultProfiles);

            configManager = new ConfigManager(assembliesToScan, specifier);
            wcfManager = new WcfManager();
            roleManager = new RoleManager(assembliesToScan);
        }

        /// <summary>
        ///     Creates and starts the bus as per the configuration
        /// </summary>
        public void Start()
        {
            PerformConfiguration();

            bus = Configure.Instance.CreateBus();
            if (bus != null && !SettingsHolder.Get<bool>("Endpoint.SendOnly"))
            {
                bus.Start();
            }

            configManager.Startup();
            wcfManager.Startup();
        }

        /// <summary>
        ///     Finalize
        /// </summary>
        public void Stop()
        {
            configManager.Shutdown();
            wcfManager.Shutdown();

            if (bus != null)
            {
                bus.Shutdown();
                bus.Dispose();

                bus = null;
            }
        }

        /// <summary>
        ///     When installing as windows service (/install), run infrastructure installers
        /// </summary>
        public void Install<TEnvironment>(string username) where TEnvironment : IEnvironment
        {
            PerformConfiguration();
            //HACK: to ensure the installer runner performs its installation
            Configure.Instance.Initialize();
        }

        void PerformConfiguration()
        {
            var initialization = specifier as IWantCustomInitialization;
            if (initialization != null)
            {
                try
                {
                    initialization.Init();
                }
                catch (NullReferenceException ex)
                {
                    throw new NullReferenceException(
                        "NServiceBus has detected a null reference in your initialization code." +
                        " This could be due to trying to use NServiceBus.Configure before it was ready." +
                        " One possible solution is to inherit from IWantCustomInitialization in a different class" +
                        " than the one that inherits from IConfigureThisEndpoint, and put your code there.",
                        ex);
                }
            }

            if (!Configure.WithHasBeenCalled())
            {
                Configure.With(assembliesToScan);
            }

            if (!Configure.BuilderIsConfigured())
            {
                Configure.Instance.DefaultBuilder();
            }

            roleManager.ConfigureBusForEndpoint(specifier);

            configManager.ConfigureCustomInitAndStartup();
        }

        List<Assembly> assembliesToScan;
        ConfigManager configManager;
        RoleManager roleManager;
        IConfigureThisEndpoint specifier;
        WcfManager wcfManager;
        IStartableBus bus;
    }
}