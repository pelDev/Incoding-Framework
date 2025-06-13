namespace Incoding.Data
{
    using System.Collections.Generic;
    #region << Using >>

    using System.Linq;
    using System.Threading.Tasks;

    #endregion

    public interface IRepositoryAsync
    {
        #region << Methods >>

        TProvider GetProvider<TProvider>() where TProvider : class;

        Task ExecuteSql(string sql);


        /// <summary>
        ///     Persist the given entity instance
        /// </summary>
        /// <typeparam name="TEntity">Strong type entity</typeparam>
        /// <param name="entity">Entity instance</param>
        Task Save<TEntity>(TEntity entity) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Persist the given entity instance
        /// </summary>
        /// <typeparam name="TEntity">Strong type entity</typeparam>
        /// <param name="entities">Entities instance</param>
        Task Saves<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Flushed if a query is requested for some entity type and there are dirty local entity instances
        /// </summary>
        Task Flush();

        /// <summary>
        ///     <see cref="Saves{TEntity}" /> or update
        /// </summary>
        /// <typeparam name="TEntity">Type entity</typeparam>
        /// <param name="entity">Entity instance</param>
        Task SaveOrUpdate<TEntity>(TEntity entity) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Delete a entity from the datastore  by id
        /// </summary>
        /// <typeparam name="TEntity">Type entity</typeparam>
        /// <param name="id">Id</param>
        Task Delete<TEntity>(object id) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Delete a entity from the datastore  by ids ( don't support cascade )
        /// </summary>
        /// <typeparam name="TEntity">Type entity</typeparam>
        /// <param name="ids">Ids</param>
        Task DeleteByIds<TEntity>(IEnumerable<object> ids) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Delete a entity instance from the datastore
        /// </summary>
        /// <typeparam name="TEntity">Type entity</typeparam>
        /// <param name="entity">Persistence instance</param>
        Task Delete<TEntity>(TEntity entity) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Delete all entities
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        Task DeleteAll<TEntity>() where TEntity : class, IEntity, new();

        /// <summary>
        ///     Getting entity instance from persist
        /// </summary>
        /// <typeparam name="TEntity">Strong type entity</typeparam>
        /// <param name="id">Primary key</param>
        /// <returns> Instance entity </returns>
        Task<TEntity> GetById<TEntity>(object id) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Getting entity instance from persist or cache
        /// </summary>
        /// <typeparam name="TEntity">Strong type entity</typeparam>
        /// <param name="id">Primary key</param>
        /// <returns> Instance entity </returns>
        Task<TEntity> LoadById<TEntity>(object id) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Query entities with specifications
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Strong type entity
        /// </typeparam>
        /// <param name="orderSpecification">
        ///     Specification how sort entities
        /// </param>
        /// <param name="whereSpecification">
        ///     Specification how filter entities
        /// </param>
        /// <param name="fetchSpecification">
        ///     Specification why join with entities ( many to many , many to one , one to one )
        /// </param>
        /// <param name="paginatedSpecification">
        ///     Specification how much skip and take entities
        /// </param>
        /// <returns>
        ///     Queryable collections ( pending request )
        /// </returns>
        Task<IQueryable<TEntity>> Query<TEntity>(OrderSpecification<TEntity> orderSpecification = null, Specification<TEntity> whereSpecification = null, FetchSpecification<TEntity> fetchSpecification = null, PaginatedSpecification paginatedSpecification = null) where TEntity : class, IEntity, new();

        /// <summary>
        ///     Query page by page
        /// </summary>
        /// <typeparam name="TEntity">
        ///     Strong type entity
        /// </typeparam>
        /// <param name="paginatedSpecification">
        ///     Specification how much skip and take entities
        /// </param>
        /// <param name="orderSpecification">
        ///     Specification how sort entities
        /// </param>
        /// <param name="whereSpecification">
        ///     Specification how filter entities
        /// </param>
        /// <param name="fetchSpecification">
        ///     Specification why join with entities ( many to many , many to one , one to one )
        /// </param>
        /// <returns>
        ///     <see cref="IncPaginatedResult{TItem}" />
        /// </returns>
        Task<IncPaginatedResult<TEntity>> Paginated<TEntity>(PaginatedSpecification paginatedSpecification, OrderSpecification<TEntity> orderSpecification = null, Specification<TEntity> whereSpecification = null, FetchSpecification<TEntity> fetchSpecification = null) where TEntity : class, IEntity, new();

        /// <summary>
        /// Completely clear the session. Evict all loaded instances and cancel all pending
        /// </summary>
        void Clear();

        #endregion
    }

}
