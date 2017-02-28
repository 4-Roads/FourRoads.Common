using System.Collections.Generic;
using System.Linq;
using FourRoads.Common.Interfaces;

namespace FourRoads.Common.Extensions
{
    public static class PagedCollectionExtensions
    {
        /// <summary>
        ///     Delegate pointer to a test that returns true if an entity should be
        ///     excluded from a returned set of results
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="item">The entity to be tested</param>
        /// <returns>Boolean, true if the entity should be exclude otherwise false</returns>
        public delegate bool ExcludeItems<in TEntity>(TEntity item)
            where TEntity : class;

        /// <summary>
        ///     Delegate pointer to a method that takes an IPaged query and returns
        ///     an IPagedCollection of type TEntity
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TQuery">The entity IPagedQueryV2 implemented type</typeparam>
        /// <param name="query">The query objecy</param>
        /// <returns>Returns an IPagedCollection that contains the results of the query</returns>
        public delegate IPagedCollection<TEntity> GetPagedItems<TEntity, in TQuery>(TQuery query)
            where TEntity : class
            where TQuery : class, IPagedQueryV2;

        /// <summary>
        ///     This method returns an enumerator that will get all items in all pages of a paged result set
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TQuery">The entity IPagedQueryV2 implemented type</typeparam>
        /// <param name="query">The query object</param>
        /// <param name="getPagedItems">
        ///     A delegate pointer to the method used to get items for a query
        ///     (Normally manager.GetItems)
        /// </param>
        /// ///
        /// <returns>An IEnumerable of type TEntity</returns>
        public static IEnumerable<TEntity> All<TEntity, TQuery>(this TQuery query, GetPagedItems<TEntity, TQuery> getPagedItems)
            where TEntity : class
            where TQuery : class, IPagedQueryV2
        {
            return query.All(getPagedItems, null);
        }

        /// <summary>
        ///     This method returns an enumerator that will get all items in all pages of a paged result set
        ///     where the exlude results in false or is not supplied
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TQuery">The entity IPagedQueryV2 implemented type</typeparam>
        /// <param name="query">The query object</param>
        /// <param name="getPagedItems">
        ///     A delegate pointer to the method used to get items for a query
        ///     (Normally manager.GetItems)
        /// </param>
        /// <param name="exlude">
        ///     A delegate pointer to a test that says whether an item should be included in
        ///     the enumeration or not
        /// </param>
        /// <returns>An IEnumerable of type TEntity</returns>
        public static IEnumerable<TEntity> All<TEntity, TQuery>(this TQuery query, GetPagedItems<TEntity, TQuery> getPagedItems, ExcludeItems<TEntity> exlude)
            where TEntity : class
            where TQuery : class, IPagedQueryV2
        {
            query.PageIndex = 0;
            query.PageSize = query.PageSize == 0 ? query.PageSize = 1000 : query.PageSize;
            var pagedItems = getPagedItems.Invoke(query);

            while (pagedItems != null)
            {
                foreach (var item in pagedItems.Items)
                {
                    if (exlude == null || !exlude(item))
                        yield return item;
                }

                if (pagedItems.HasMoreItems())
                {
                    query.PageIndex += 1;
                    pagedItems = getPagedItems(query);
                }
                else
                {
                    pagedItems = null;
                }
            }
        }

        /// <summary>
        ///     Returns true if the collection has one or more pages otherwise false
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <param name="items">The IPagedCollection to be tested</param>
        /// <returns>Returns true if there are more items, otherwise false</returns>
        public static bool HasMoreItems<TEntity>(this IPagedCollection<TEntity> items)
        {
            return items.PageIndex*items.PageSize + items.Items.Count() < items.TotalRecords;
        }

        /// <summary>
        ///     This method will return an IPagedCollection of Items without items
        ///     that should be excluded.  This method should only be used when it
        ///     is not feasable to modify the query to subset the collection
        /// </summary>
        /// <typeparam name="TEntity">The entity type</typeparam>
        /// <typeparam name="TQuery">The entity IPagedQueryV2 implemented type</typeparam>
        /// <param name="query">The query object</param>
        /// <param name="getPagedItems"></param>
        /// <param name="exlude">
        ///     A delegate pointer to a test that says whether an item should be included in
        ///     the enumeration or not
        /// </param>
        /// <returns>
        ///     An IPageCollection for the PageIndex set in the query with results that
        ///     does not contain results where the exclude delegate returned true
        /// </returns>
        public static IPagedCollection<TEntity> WithOut<TEntity, TQuery>(this TQuery query, GetPagedItems<TEntity, TQuery> getPagedItems, ExcludeItems<TEntity> exlude)
            where TEntity : class
            where TQuery : class, IPagedQueryV2
        {
            var page = (int) query.PageIndex;
            var size = query.PageSize;

            query.PageIndex = 0;
            var items = new List<TEntity>(query.All(getPagedItems));
            return new PagedCollection<TEntity>(items.Skip(page*size).Take(size).ToList(),
                query.PageIndex,
                query.PageSize,
                items.Count);
        }
    }
}