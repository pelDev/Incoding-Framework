namespace Incoding.Data
{
    #region << Using >>

    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Incoding.Extensions;
    using JetBrains.Annotations;
    using NHibernate;
    using NHibernate.Linq;
    using NHibernate.Persister.Entity;

    #endregion

    public class NhibernateRepository : IRepository
    {
        #region Fields

        readonly ISession session;

        #endregion

        #region Constructors

        public NhibernateRepository(ISession session)
        {
            this.session = session;
        }

        [Obsolete("Not needed use Repository on IOC", true), ExcludeFromCodeCoverage, UsedImplicitly]
        public NhibernateRepository() { }

        #endregion

        #region IRepository Members

        public void ExecuteSql(string sql)
        {
            session.CreateSQLQuery(sql).ExecuteUpdate();
        }

        public async Task ExecuteSqlAsync(string sql, CancellationToken cancellationToken = default)
        {
            var query = session.CreateSQLQuery(sql);
            await query.ExecuteUpdateAsync(cancellationToken);
        }

        public TProvider GetProvider<TProvider>() where TProvider : class
        {
            return session as TProvider;
        }

        public void Save<TEntity>(TEntity entity) where TEntity : class, IEntity, new()
        {
            session.Save(entity);
        }

        public async Task SaveAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            await session.SaveAsync(entity, cancellationToken);
        }

        public void Saves<TEntity>(IEnumerable<TEntity> entities) where TEntity : class, IEntity, new()
        {
            foreach (var entity in entities)
                Save(entity);
        }

        public async Task SavesAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            foreach (var entity in entities)
                await SaveAsync(entity, cancellationToken);
        }

        public void Flush()
        {
            session.Flush();            
        }

        public async Task FlushAsync(CancellationToken cancellationToken = default)
        {
            await session.FlushAsync(cancellationToken);
        }

        public void SaveOrUpdate<TEntity>(TEntity entity) where TEntity : class, IEntity, new()
        {
            session.SaveOrUpdate(entity);
        }

        public async Task SaveOrUpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            await session.SaveOrUpdateAsync(entity, cancellationToken);
        }

        public void Delete<TEntity>(object id) where TEntity : class, IEntity, new()
        {
            Delete(session.Load<TEntity>(id));
        }

        public async Task DeleteAsync<TEntity>(object id, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            var entity = await session.LoadAsync<TEntity>(id, cancellationToken);
            await DeleteAsync(entity, cancellationToken);
        }

        public void DeleteByIds<TEntity>(IEnumerable<object> ids) where TEntity : class, IEntity, new()
        {
            var metadata = GetMetaData<TEntity>();
            string idColumnName = metadata.GetPropertyColumnNames("Id").FirstOrDefault();
            string tableName = metadata.TableName;
            string queryString = "DELETE FROM [{0}] WHERE {1} IN ({2})".F(tableName, idColumnName, ids.Select(o => o.GetType().IsAnyEquals(typeof(string), typeof(Guid)) ? "'{0}'".F(o.ToString()) : o.ToString()).AsString(","));
            session
                    .CreateSQLQuery(queryString)
                    .ExecuteUpdate();
        }

        public async Task DeleteByIdsAsync<TEntity>(IEnumerable<object> ids, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            var metadata = GetMetaData<TEntity>();
            string idColumnName = metadata.GetPropertyColumnNames("Id").FirstOrDefault();
            string tableName = metadata.TableName;
            string queryString = "DELETE FROM [{0}] WHERE {1} IN ({2})".F(tableName, idColumnName, ids.Select(o => o.GetType().IsAnyEquals(typeof(string), typeof(Guid)) ? "'{0}'".F(o.ToString()) : o.ToString()).AsString(","));
            await session
                    .CreateSQLQuery(queryString)
                    .ExecuteUpdateAsync(cancellationToken);
        }

        public void Delete<TEntity>(TEntity entity) where TEntity : class, IEntity, new()
        {
            session.Delete(entity);
        }

        public async Task DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            await session.DeleteAsync(entity);
        }

        public void DeleteAll<TEntity>() where TEntity : class, IEntity, new()
        {
            session.CreateSQLQuery("DELETE {0}".F(GetMetaData<TEntity>().TableName))
                   .ExecuteUpdate();
        }

        public async Task DeleteAllAsync<TEntity>(CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            await session.CreateSQLQuery("DELETE {0}".F(GetMetaData<TEntity>().TableName))
                   .ExecuteUpdateAsync(cancellationToken);
        }

        public TEntity GetById<TEntity>(object id) where TEntity : class, IEntity, new()
        {
            if (id == null)
                return null;

            return session.Get<TEntity>(id);
        }

        public async Task<TEntity> GetByIdAsync<TEntity>(object id, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            if (id == null)
                return null;

            return await session.GetAsync<TEntity>(id, cancellationToken);
        }

        public async Task<TEntity> LoadByIdAsync<TEntity>(object id, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            if (id == null)
                return null;

            return await session.LoadAsync<TEntity>(id);
        }

        public TEntity LoadById<TEntity>(object id) where TEntity : class, IEntity, new()
        {
            if (id == null)
                return null;

            return session.Load<TEntity>(id);
        }

        public IQueryable<TEntity> Query<TEntity>(OrderSpecification<TEntity> orderSpecification = null, Specification<TEntity> whereSpecification = null, FetchSpecification<TEntity> fetchSpecification = null, PaginatedSpecification paginatedSpecification = null) where TEntity : class, IEntity, new()
        {
            return session.Query<TEntity>().Query(orderSpecification, whereSpecification, fetchSpecification, paginatedSpecification);
        }

        public IncPaginatedResult<TEntity> Paginated<TEntity>(PaginatedSpecification paginatedSpecification, OrderSpecification<TEntity> orderSpecification = null, Specification<TEntity> whereSpecification = null, FetchSpecification<TEntity> fetchSpecification = null) where TEntity : class, IEntity, new()
        {
            return session.Query<TEntity>().Paginated(orderSpecification, whereSpecification, fetchSpecification, paginatedSpecification);
        }

        public async Task<IncPaginatedResult<TEntity>> PaginatedAsync<TEntity>(PaginatedSpecification paginatedSpecification, OrderSpecification<TEntity> orderSpecification = null, Specification<TEntity> whereSpecification = null, FetchSpecification<TEntity> fetchSpecification = null, CancellationToken cancellationToken = default) where TEntity : class, IEntity, new()
        {
            return await session.Query<TEntity>().PaginatedAsync(orderSpecification, whereSpecification, fetchSpecification, paginatedSpecification);
        }


        public void Clear()
        {
            session.Clear();
        }

        #endregion

        SingleTableEntityPersister GetMetaData<T>()
        {
            var metadata = session.SessionFactory.GetClassMetadata(typeof(T));
            return (SingleTableEntityPersister)metadata;
        }
    }
}