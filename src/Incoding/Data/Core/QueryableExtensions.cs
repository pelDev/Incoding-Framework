namespace Incoding.Data
{
    #region << Using >>

    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Incoding.Extensions;
    using Incoding.Maybe;
    using NHibernate.Linq;

    #endregion

    public static class QueryableExtensions
    {
        public static IncPaginatedResult<TEntity> Paginated<TEntity>(this IQueryable<TEntity> source, OrderSpecification<TEntity> orderSpecification, Specification<TEntity> whereSpecification, FetchSpecification<TEntity> fetchSpecification, PaginatedSpecification paginatedSpecification) where TEntity : class, IEntity
        {
            int totalCount = source.Query(null, whereSpecification, null, null).Count();
            var paginatedItems = source.Query(orderSpecification, whereSpecification, fetchSpecification, paginatedSpecification).ToList();
            return new IncPaginatedResult<TEntity>(paginatedItems, totalCount);
        }

        public static async Task<IncPaginatedResult<TEntity>> PaginatedAsync<TEntity>(this IQueryable<TEntity> source, OrderSpecification<TEntity> orderSpecification, Specification<TEntity> whereSpecification, FetchSpecification<TEntity> fetchSpecification, PaginatedSpecification paginatedSpecification, CancellationToken ct = default) where TEntity : class, IEntity
        {
            int totalCount = await source.Query(null, whereSpecification, null, null).CountAsync(ct);
            var paginatedItems = await source.Query(orderSpecification, whereSpecification, fetchSpecification, paginatedSpecification).ToListAsync(ct);
            return new IncPaginatedResult<TEntity>(paginatedItems, totalCount);
        }

        public static IQueryable<TEntity> Query<TEntity>(this IQueryable<TEntity> source, OrderSpecification<TEntity> orderSpecification, Specification<TEntity> whereSpecification, FetchSpecification<TEntity> fetchSpecification, PaginatedSpecification paginatedSpecification) where TEntity : class, IEntity
        {
            if (whereSpecification.With(r => r.IsSatisfiedBy()) != null)
                source = source.Where(whereSpecification.IsSatisfiedBy());

            if (orderSpecification != null)
            {
                var order = new AdHocOrderSpecification<TEntity>();
                orderSpecification.SortedBy()(order);
                source = order.applies.Aggregate(source, (current, apply) => apply(current));
            }

            if (paginatedSpecification != null)
                source = source.Page(paginatedSpecification.CurrentPage, paginatedSpecification.PageSize);

            if (fetchSpecification != null)
            {
                AdHocFetchSpecificationBase<TEntity> fetch = null;
                string fullName = source.Provider.GetType().FullName;
                if (fullName.EqualsWithInvariant("NHibernate.Linq.DefaultQueryProvider"))
                    fetch = new AdHocFetchNhibernateSpecification<TEntity>();
                else if (fullName.EqualsWithInvariant("System.Data.Entity.Internal.Linq.DbQueryProvider"))
                    fetch = new AdHocFetchEFSpecification<TEntity>();
                else if (fullName.Contains("Raven.Client.Linq.RavenQueryProvider"))
                    fetch = new AdHocFetchRavenDbSpecification<TEntity>();

                if (fetch != null)
                {
                    fetchSpecification.FetchedBy()(fetch);
                    source = fetch.applies.Aggregate(source, (current, apply) => apply(current));
                }
            }

            return source;
        }
    }
}