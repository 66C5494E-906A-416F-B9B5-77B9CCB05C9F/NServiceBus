﻿namespace NServiceBus.Features
{
    using System;
    using System.Collections.Generic;
    using Settings;
    using Utils;

    /// <summary>
    ///     Used to control the various features supported by the framework.
    /// </summary>
    public abstract class Feature
    {
        Configurator configurator;

        internal void SetConfigurator(Configurator configurator)
        {
            this.configurator = configurator;                
        }

        protected Feature()
        {
            name = GetType().Name.Replace("Feature", String.Empty);
        }

        /// <summary>
        ///     Return <c>true</c> if this is a default <see cref="Feature" /> that needs to be turned on automatically.
        /// </summary>
        public virtual bool IsEnabledByDefault
        {
            get { return false; }
        }

        /// <summary>
        ///     Feature name.
        /// </summary>
        public string Name
        {
            get { return name; }
        }

        /// <summary>
        ///     True if this specific feature is enabled
        /// </summary>
        public bool Enabled
        {
            get { return IsEnabled(GetType()); }
        }

        /// <summary>
        ///     Returns the category for this feature if any
        /// </summary>
        public virtual FeatureCategory Category
        {
            get { return FeatureCategory.None; }
        }

        public string Version
        {
            get { return FileVersionRetriever.GetFileVersion(GetType()); }
        }

        public Configurator Configurator
        {
            get { return configurator; }
        }

        /// <summary>
        ///     Called when the feature should perform its initialization. This call will only happen if the feature is enabled.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        ///     Returns true if the feature should be enable. This method wont be called if the feature is explicitly disabled
        /// </summary>
        public virtual bool ShouldBeEnabled()
        {
            return true;
        }

        /// <summary>
        ///     Enables the give feature
        /// </summary>
        public static void Enable<T>() where T : Feature
        {
            SettingsHolder.Set(featureType.FullName, true);
        }

        /// <summary>
        ///     Enables the give feature unless explicitly disabled
        /// </summary>
        public static void EnableByDefault<T>() where T : Feature
        {
            EnableByDefault(typeof(T));
        }

        /// <summary>
        ///     Enables the give feature unless explicitly disabled
        /// </summary>
        public static void EnableByDefault(Type featureType)
        {
            SettingsHolder.SetDefault(featureType.FullName, true);
        }


        /// <summary>
        ///     Turns the given feature off
        /// </summary>
        public static void Disable<T>() where T : Feature
        {
            SettingsHolder.Set(featureType.FullName, false);

        }

        /// <summary>
        ///     Returns true if the given feature is enabled
        /// </summary>
        public static bool IsEnabled<T>() where T : Feature
        {
            return IsEnabled(typeof(T));
        }


        /// <summary>
        ///     Returns true if the given feature is enabled
        /// </summary>
        public static bool IsEnabled(Type feature)
        {
            return SettingsHolder.GetOrDefault<bool>(feature.FullName);
        }

        /// <summary>
        ///     Gets all features for the given category
        /// </summary>
        public static IEnumerable<Feature> ByCategory(FeatureCategory category)
        {
            var result = new List<Feature>();

            Configure.Instance.ForAllTypes<Feature>(t =>
            {
                var feature = (Feature) Activator.CreateInstance(t);

                if (feature.Category == category)
                {
                    result.Add(feature);
                }
            });

            return result;
        }

        public override string ToString()
        {
            return string.Format("{0} [{1}]", Name, Version);
        }

        protected bool Equals(Feature other)
        {
            return string.Equals(name, other.name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((Feature) obj);
        }

        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }


        public static bool operator ==(Feature feature1, Feature feature2)
        {
            if (ReferenceEquals(feature1, null))
            {
                return ReferenceEquals(feature2, null);
            }

            return feature1.Equals(feature2);
        }

        public static bool operator !=(Feature feature1, Feature feature2)
        {
            return !(feature1 == feature2);
        }


        string name;
    }

    public abstract class Feature<T> : Feature where T : FeatureCategory
    {
        public override FeatureCategory Category
        {
            get { return Activator.CreateInstance<T>(); }
        }
    }
}