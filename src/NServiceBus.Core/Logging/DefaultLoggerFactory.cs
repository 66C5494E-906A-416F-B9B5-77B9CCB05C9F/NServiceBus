namespace NServiceBus.Logging
{
    using System;

    class DefaultLoggerFactory : ILoggerFactory
    {
        object locker = new object();

        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            return new DefaultLogger(name, Environment.CurrentDirectory, locker);
        }
    }
}