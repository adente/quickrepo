using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace Tehkne.QuickRepo.EF
{
    /// <summary>
    /// Interface class for BaseRepository
    /// </summary>
    /// <typeparam name="TContext">DbContext type</typeparam>
    public interface IBaseRepository<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Creates the context using the provided setings;
        /// Should be called with using statement
        /// </summary>
        /// <returns><typeparamref name="TContext"/></returns>
        TContext CreateContext();
    }

    /// <summary>
    /// Repository Base for no specific type (DbContext Wrapper)
    /// </summary>
    /// <typeparam name="TContext">DbContext type</typeparam>
    public abstract class BaseRepository<TContext> : IBaseRepository<TContext>
        where TContext : DbContext
    {
        protected bool AutoDetectChangesEnabled { get; private set; }
        protected bool LazyLoadingEnabled { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="autoDetectChanges">indicates whether to auto detect changes in DbContext</param>
        /// <param name="lazyLoading">indicates whether to use lazy loading in DbContext</param>
        protected BaseRepository(bool autoDetectChanges = false, bool lazyLoading = false)
        {
            AutoDetectChangesEnabled = autoDetectChanges;
            LazyLoadingEnabled = lazyLoading;
        }

        /// <inheritdoc />
        public TContext CreateContext()
        {
            var context = Activator.CreateInstance<TContext>();
            context.Configuration.AutoDetectChangesEnabled = AutoDetectChangesEnabled;
            context.Configuration.LazyLoadingEnabled = LazyLoadingEnabled;
            return context;
        }
    }

    /// <summary>
    /// Base repository for a specific entity with key
    /// </summary>
    /// <typeparam name="TContext">DbContext type</typeparam>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <typeparam name="TKey">Entity key type</typeparam>
    public abstract class BaseRepository<TContext, TEntity, TKey> : BaseRepository<TContext>
        where TEntity : class
        where TContext : DbContext
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="autoDetectChanges">indicates whether to auto detect changes in DbContext</param>
        /// <param name="lazyLoading">indicates whether to use lazy loading in DbContext</param>
        protected BaseRepository(bool autoDetectChanges = false, bool lazyLoading = false) : base(autoDetectChanges, lazyLoading)
        { }

        /// <summary>
        /// Finds the entity with the specified key
        /// </summary>
        /// <param name="key">The entity key</param>
        /// <param name="context">The DbContext instance</param>
        /// <returns>The specific entity</returns>
        public virtual TEntity Get(TKey key, TContext context)
        {
            return context.Set<TEntity>().Find(key);
        }

        /// <summary>
        /// Finds the entity with the specified key
        /// </summary>
        /// <param name="key">The entity key</param>
        /// <returns>The specific entity</returns>
        public virtual TEntity Get(TKey key)
        {
            using (var context = CreateContext())
            {
                return Get(key, context);
            }
        }

        /// <summary>
        /// Gets a query to all entities of that entity type
        /// </summary>
        /// <param name="context">The DbContext instance</param>
        /// <returns>IQueryable of entity type</returns>
        public virtual IQueryable<TEntity> GetAll(TContext context)
        {
            return context.Set<TEntity>();
        }

        /// <summary>
        /// Gets all instances of the specific entity type
        /// </summary>
        /// <param name="filter">Function to filter / order results</param>
        /// <returns>All instances of the specific entity type</returns>
        public virtual IList<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null)
        {
            using (var context = CreateContext())
            {
                var result = GetAll(context);

                if (filter != null)
                    result = filter(result);

                return result.ToList();
            }
        }
        
        /// <summary>
        /// Gets all instances of the specific entity type
        /// </summary>
        /// <typeparam name="T">Type of parameter to pass to filter function</typeparam>
        /// <param name="filter">Function to filter / order results</param>
        /// <param name="param">Aditional parameter to pass to filter function</param>
        /// <returns>All instances of the specific entity type</returns>
        public virtual IList<TEntity> GetAll<T>(Func<IQueryable<TEntity>, T, IQueryable<TEntity>> filter = null, T param = default(T))
        {
            using (var context = CreateContext())
            {
                var result = GetAll(context);

                if (filter != null)
                    result = filter(result, param);

                return result.ToList();
            }
        }

        /// <summary>
        /// Adds the given entity to the DbContext
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="context">The DbContext instance</param>
        /// <returns>The entity</returns>
        public virtual TEntity Add(TEntity entity, TContext context)
        {
            return SetState(entity, context, EntityState.Added);
        }

        /// <summary>
        /// Adds the given entity to the DbContext
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The entity</returns>
        public virtual TEntity Add(TEntity entity)
        {
            using (var context = CreateContext())
            {
                var addedEntity = Add(entity, context);
                context.SaveChanges();
                return addedEntity;
            }
        }

        /// <summary>
        /// Updates the given entity to the DbContext
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="context">The DbContext instance</param>
        /// <returns>The entity</returns>
        public virtual TEntity Update(TEntity entity, TContext context)
        {
            return SetState(entity, context, EntityState.Modified);
        }

        /// <summary>
        /// Updates the given entity to the DbContext
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The entity</returns>
        public virtual TEntity Update(TEntity entity)
        {
            using (var context = CreateContext())
            {
                var updatedEntity = Update(entity, context);
                context.SaveChanges();
                return updatedEntity;
            }
        }

        /// <summary>
        /// Deletes the given entity to the DbContext
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <param name="context">The DbContext instance</param>
        public virtual void Delete(TEntity entity, TContext context)
        {
            SetState(entity, context, EntityState.Deleted);
        }

        /// <summary>
        /// Deletes the given entity to the DbContext
        /// </summary>
        /// <param name="entity">The entity</param>
        public virtual void Delete(TEntity entity)
        {
            using (var context = CreateContext())
            {
                Delete(entity, context);
                context.SaveChanges();
            }
        }

        private static TEntity SetState(TEntity entity, TContext context, EntityState state)
        {
            var entry = context.Entry(entity);
            entry.State = state;
            return entry.Entity;
        }
    }
}
