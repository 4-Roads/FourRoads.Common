using System;

namespace FourRoads.Common
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class CacheKeyAttribute : Attribute
    {
        public readonly object DefaultValue;
        public readonly bool HasDefaultValue;
        public readonly bool Ignored;
        public readonly string KeyName;

        public CacheKeyAttribute(string keyName)
        {
            KeyName = keyName;
            Ignored = false;
            HasDefaultValue = false;
        }

        public CacheKeyAttribute(string keyName, object defaultValue)
        {
            KeyName = keyName;
            DefaultValue = defaultValue;
            HasDefaultValue = true;
            Ignored = false;
        }

        public CacheKeyAttribute(bool includeInKey)
        {
            Ignored = !includeInKey;
            HasDefaultValue = false;
        }
    }
}