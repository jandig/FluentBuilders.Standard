using System;
using System.Collections.Generic;

namespace FluentBuilders.Core
{
    public class CollectionBuilder<T, TBuilder> : Builder<IEnumerable<T>>
        where T : class
        where TBuilder : Builder<T>
    {
        private readonly Func<TBuilder> factoryResolver;
        private readonly List<TBuilder> builders;

        /// <summary>
        /// True if the collection builder was explicitly set to be empty, otherwise False.
        /// </summary>
        public bool ExplicitlyEmpty { get; set; }

        public CollectionBuilder(IBuilder parentFactory)
        {
            factoryResolver = parentFactory.BuildUsing<TBuilder>;
            builders = new List<TBuilder>();
        }

        /// <summary>
        /// List of builders that will construct this collection.
        /// </summary>
        public List<TBuilder> Builders => builders;

        /// <summary>
        /// Explicitly state that this collection should remain empty.
        /// </summary>
        /// <returns></returns>
        public void None() => ExplicitlyEmpty = true;

        /// <summary>
        /// Add one entity that will be created by a builder to this child collection.
        /// </summary>
        /// <returns></returns>
        public TBuilder AddOne() => AddMany(1);

        /// <summary>
        /// Add specified instance to this child collection.
        /// </summary>
        /// <param name="toAdd">Instance to add.</param>
        public void AddOne(T toAdd) => AddMany(new[] { toAdd });

        /// <summary>
        /// Add one entity that will be created by a builder to this child collection
        /// </summary>
        /// <param name="opts">Alterations that should be done to the builder for the entity</param>
        public void AddOne(Action<TBuilder> opts = null!) => AddMany(1, opts!);

        /// <summary>
        /// Add specified instances to this child collection.
        /// </summary>
        /// <param name="addItems">Instances to add.</param>
        public void AddMany(IEnumerable<T> addItems)
        {
            foreach (var a in addItems)
                builders.Add((TBuilder)factoryResolver().WithInstance(a));
        }

        /// <summary>
        /// Adds multiple entities that will each be created by a builder to this child collection
        /// </summary>
        /// <param name="count">Number of entities to create</param>
        /// <param name="opts">Alterations that should be done to the builder of each item.</param>
        /// <returns></returns>
        public TBuilder AddMany(int count, Action<TBuilder> opts = null!)
        {
            var fac = factoryResolver();
            for (var i = 0; i < count; i++)
            {
                if (opts != null)
                    opts(fac);
                builders.Add(fac);
            }
            return fac;
        }

        /// <summary>
        /// Processes this collection builder, creating all instances.
        /// </summary>
        /// <param name="setupAction">Optional action to apply to each builder.</param>
        /// <param name="customization">Optional action to apply to each created entity.</param>
        /// <returns>All instances created by this collection builder.</returns>
        public IEnumerable<T> CreateAll(Action<TBuilder> setupAction = null!, Action<T> customization = null!)
        {
            for (var i = 0; i < builders.Count; i++)
            {
                var builder = builders[i];
                setupAction?.Invoke(builder);
                var result = builder.Create(i);
                customization?.Invoke(result);
                yield return result;
            }
        }

        protected override IEnumerable<T> Build(int seed) => CreateAll();
    }
}
