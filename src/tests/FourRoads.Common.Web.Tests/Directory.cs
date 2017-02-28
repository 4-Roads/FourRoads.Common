using System;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Web.Tests.Entities
{
    public class Directory : ICacheable
    {
        public Directory()
        {
            CreatedDate = DateTime.Now;
            ApplicationId = Guid.NewGuid();
            IsEnabled = true;
        }

        public static string CachePrefix = "Directory:";

        public int Id { get; set; }

        public Guid ApplicationId { get; set; }

        public string ApplicationKey { get; set; }

        public string Name { get; set; }

        public Guid ContainerId { get; set; }

        public int CreatedByUserId { get; set; }

        public DateTime CreatedDate { get; set; }

        public bool IsEnabled { get; set; }

        public bool IsIndexed { get; set; }

        public string Description { get; set; }

        #region Implementation of ICacheable

        public string CacheID => $"{CachePrefix}{Id}";

        public int CacheRefreshInterval => 6000;

        public string[] CacheTags => new string[] {};

        public CacheScopeOption CacheScope => CacheScopeOption.All;

        #endregion
    }
}