using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common
{
    public class PagedCollection<TItem> : IPagedCollection<TItem>, ICollection<TItem>, IEnumerable<TItem>, IEnumerable
    {
        private ICollection<TItem> _Items;

        private PagedCollection()
        {
        }

        public PagedCollection(uint pageIndex, int pageSize, int totalRecords, IEnumerable<TItem> items)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalRecords = totalRecords;

            if (items != null)
                _Items = items.ToList();
        }

        public PagedCollection(ICollection<TItem> itemCollection, uint pageIndex, int pageSize, int totalRecords)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalRecords = totalRecords;

            if (itemCollection != null)
                _Items = itemCollection;
        }

        #region IEnumerable<TItem> Members

        public IEnumerator<TItem> GetEnumerator()
        {
            return InternalItems.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalItems.GetEnumerator();
        }

        #endregion

        #region IPagedCollection<TItem> Members

        public uint PageIndex { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; protected set; }

        public IEnumerable<TItem> Items
        {
            get { return InternalItems; }
            set { InternalItems = value.ToList(); }
        }

        public IPagedCollection<TCast> Cast<TCast>()
        {
            var casted = new PagedCollection<TCast>();

            casted.PageIndex = PageIndex;
            casted.PageSize = PageSize;
            casted.TotalRecords = TotalRecords;

            if (_Items != null)
                casted._Items = _Items.Cast<TCast>().ToList();

            return casted;
        }

        protected ICollection<TItem> InternalItems
        {
            get
            {
                if (_Items == null)
                    _Items = new List<TItem>();
                return _Items;
            }
            set
            {
                if (value != null)
                    _Items = value.ToList();
                else
                    _Items = null;
            }
        }

        #endregion

        #region ICollection<TItem> Members

        public void Add(TItem item)
        {
            InternalItems.Add(item);
        }

        public void Clear()
        {
            InternalItems.Clear();
        }

        public bool Contains(TItem item)
        {
            return Items.Contains(item);
        }

        public void CopyTo(TItem[] array, int arrayIndex)
        {
            InternalItems.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return InternalItems.Count; }
        }

        public bool IsReadOnly
        {
            get { return InternalItems.IsReadOnly; }
        }

        public bool Remove(TItem item)
        {
            return InternalItems.Remove(item);
        }

        #endregion
    }
}