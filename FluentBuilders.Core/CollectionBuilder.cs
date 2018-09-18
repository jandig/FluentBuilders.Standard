﻿using System;
using System.Collections.Generic;

namespace FluentBuilders.Core
{
    public class CollectionBuilder<T, TBuilder> : Builder<IEnumerable<T>>
        where T : class
        where TBuilder : Builder<T>
    {
        private readonly Func<TBuilder> _factoryResolver;
        private readonly List<TBuilder> _builders;

        /// <summary>
        /// True if the collection builder was explicitly set to be empty, otherwise False.
        /// </summary>
        public bool ExplicitlyEmpty { get; set; }

        public CollectionBuilder(IBuilder parentFactory)
        {
            _factoryResolver = parentFactory.BuildUsing<TBuilder>;
            _builders = new List<TBuilder>();
        }

        /// <summary>
        /// List of builders that will construct this collection.
        /// </summary>
        public List<TBuilder> Builders => _builders;

        /// <summary>
        /// Explicitly state that this collection should remain empty.
        /// </summary>
        /// <returns></returns>
        public void None()
        {
            ExplicitlyEmpty = true;
        }

        /// <summary>
        /// Add one entity that will be created by a builder to this child collection.
        /// </summary>
        /// <returns></returns>
        public TBuilder AddOne()
        {
            return AddMany(1);
        }

        /// <summary>
        /// Add specified instance to this child collection.
        /// </summary>
        /// <param name="toAdd">Instance to add.</param>
        public void AddOne(T toAdd)
        {
            AddMany(new[] { toAdd });
        }

        /// <summary>
        /// Add specified instances to this child collection.
        /// </summary>
        /// <param name="addItems">Instances to add.</param>
        public void AddMany(IEnumerable<T> addItems)
        {
            foreach (var a in addItems)
                _builders.Add((TBuilder) _factoryResolver().WithInstance(a));
        }

        /// <summary>
        /// Adds multiple entities that will each be created by a builder to this child collection
        /// </summary>
        /// <param name="count">Number of entities to create</param>
        /// <param name="opts">Alterations that should be done to the builder of each item.</param>
        /// <returns></returns>
        public TBuilder AddMany(int count, Action<TBuilder> opts = null)
        {
            var fac = _factoryResolver();
            for (var i = 0; i < count; i++)
            {
                opts?.Invoke(fac);
                _builders.Add(fac);
            }
            return fac;
        }

        /// <summary>
        /// Processes this collection builder, creating all instances.
        /// </summary>
        /// <param name="setupAction">Optional action to apply to each builder.</param>
        /// <param name="customization">Optional action to apply to each created entity.</param>
        /// <returns>All instances created by this collection builder.</returns>
        public IEnumerable<T> CreateAll(Action<TBuilder> setupAction = null, Action<T> customization = null)
        {
            for (var i = 0; i < _builders.Count; i++)
            {
                var builder = _builders[i];
                setupAction?.Invoke(builder);
                var result = builder.Create(i);
                customization?.Invoke(result);
                yield return result;
            }
        }

        protected override IEnumerable<T> Build(int seed)
        {
            return CreateAll();
        }
    }
}