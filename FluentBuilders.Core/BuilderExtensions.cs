﻿using System;

namespace FluentBuilders.Core
{
    public static class BuilderExtensions
    {
        /// <summary>
        /// Alter the builder itself by providing an action to perform on the builder.
        /// </summary>
        /// <typeparam name="TBuilder">Type of builder</typeparam>
        /// <param name="builder">Builder to apply alterations to</param>
        /// <param name="settings">Action to perform on the builder.</param>
        /// <returns>The builder with applied alterations.</returns>
        public static TBuilder Setup<TBuilder>(this TBuilder builder, Action<TBuilder> settings) where TBuilder : IBuilder
        {
            builder.Setups.Add(() => settings(builder));
            //settings(builder);
            return builder;
        }

        /// <summary>
        /// Tells the builder that when creating the instance, it should also persist it.
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="persist"></param>
        /// <returns></returns>
        public static TBuilder Persisted<TBuilder>(this TBuilder builder, bool persist = true) where TBuilder : IPersistingBuilder
        {
            builder.Persist = persist;
            return builder;
        }
    }
}
