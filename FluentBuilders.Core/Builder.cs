using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FluentBuilders.Core
{
    public abstract class Builder<TSubject> : IBuilder<TSubject> where TSubject : class
    {
        public List<Action> Setups { get; }
        protected Dictionary<string, IBuilder> PropertyBuilders { get; set; }

        protected readonly List<Action<TSubject>> Customizations;
        public TSubject? Instance { get; set; }
        public BuilderFactoryConvention BuilderFactoryConvention { get; set; }

        protected Builder()
        {
            Setups = new List<Action>();
            PropertyBuilders = new Dictionary<string, IBuilder>();
            Customizations = new List<Action<TSubject>>();
            BuilderFactoryConvention = new BuilderFactoryConvention();
            Instance = null;
        }

        public Builder<TSubject> WithInstance(TSubject instance)
        {
            Instance = instance;
            return this;
        }

        protected void SetProperty<T>(Expression<Func<TSubject, object>> prop, T instance) =>
            SetPropertyBuilder(prop, new ObjectContainer<T>(instance));

        protected void SetProperty<T>(string key, T instance) => SetPropertyBuilder(key, new ObjectContainer<T>(instance));

        protected void SetProperty<TNestedBuilder>(Expression<Func<TSubject, object>> prop,
            Action<TNestedBuilder> opts = null!) where TNestedBuilder : IBuilder
        {
            var builder = BuildUsing<TNestedBuilder>();
            if (opts != null)
            {
                opts(builder);
                builder.Setup(opts); //opts(builder);
            }
            SetPropertyBuilder(prop, builder);
        }

        protected void SetProperty<TNestedBuilder>(string key, Action<TNestedBuilder> opts = null!) where TNestedBuilder : IBuilder
        {
            var builder = BuildUsing<TNestedBuilder>();
            if (opts != null)
            {
                opts(builder);
                builder.Setup(opts);//opts(builder);
            }
            SetPropertyBuilder(key, builder);
        }

        protected void SetCollection<TChild, TChildBuilder>(
            Expression<Func<TSubject, object>> prop,
            Action<CollectionBuilder<TChild, TChildBuilder>> opts)
            where TChild : class
            where TChildBuilder : Builder<TChild>
        {
            var colBuilder = new CollectionBuilder<TChild, TChildBuilder>(this);
            opts(colBuilder);
            SetPropertyBuilder(prop, colBuilder);
        }

        protected CollectionBuilder<TChild, TChildBuilder> GetCollection<TChild, TChildBuilder>(Expression<Func<TSubject, object>> prop)
            where TChild : class
            where TChildBuilder : Builder<TChild>
        {
            CollectionBuilder<TChild, TChildBuilder> col = null;
            var key = GetPropertyName(prop);
            if (PropertyBuilders.ContainsKey(key))
            {
                col = PropertyBuilders[key] as CollectionBuilder<TChild, TChildBuilder>;
            }

            return col ?? new CollectionBuilder<TChild, TChildBuilder>(this);
        }

        private void SetPropertyBuilder<TNestedBuilder>(Expression<Func<TSubject, object>> prop, TNestedBuilder builder)
            where TNestedBuilder : IBuilder => SetPropertyBuilder(GetPropertyName(prop), builder);

        private void SetPropertyBuilder<TNestedBuilder>(string key, TNestedBuilder builder) where TNestedBuilder : IBuilder
        {
            if (PropertyBuilders.ContainsKey(key))
                PropertyBuilders.Remove(key);
            PropertyBuilders.Add(key, builder);
        }

        protected bool HasProperty<T>(Expression<Func<TSubject, T>> prop)
        {
            var member = (MemberExpression)prop.Body;
            var key = member.Member.Name;
            return HasProperty(key);
        }

        [Obsolete("Replace with HasProperty, HasOptInFor will be removed in version 1.0.")]
        protected bool HasOptInFor<T>(Expression<Func<TSubject, T>> prop) => HasProperty(prop);

        protected bool HasProperty(string key)
        {
            if (PropertyBuilders.ContainsKey(key))
                return true;
            return false;
        }

        [Obsolete("Replace with HasProperty, HasOptInFor will be removed in version 1.0.")]
        protected bool HasOptInFor(string key) => HasProperty(key);

        protected TBuilder GetPropertyBuilder<TBuilder>(Expression<Func<TSubject, object>> prop, Func<TBuilder> orUse)
        {
            var key = GetPropertyName(prop);
            if (PropertyBuilders.ContainsKey(key))
                return (TBuilder)PropertyBuilders[key];

            return orUse();
        }

        protected T GetProperty<T>(string key, Func<T> orUse)
        {
            if (!PropertyBuilders.ContainsKey(key))
                return orUse();
            return (T)PropertyBuilders[key].Create();
        }

        [Obsolete("Replace with GetProperty, OptInFor will be removed in version 1.0.")]
        protected T OptInFor<T>(string key, Func<T> valueIfNoOptIn) => GetProperty(key, valueIfNoOptIn);

        protected T GetProperty<T>(string key, T orUse) => GetProperty(key, () => orUse);

        [Obsolete("Replace with GetProperty, OptInFor will be removed in version 1.0.")]
        protected T OptInFor<T>(string key, T valueIfNoOptIn) => GetProperty(key, valueIfNoOptIn);

        protected T GetProperty<T>(Expression<Func<TSubject, T>> prop, Func<T> orUse) => GetProperty(GetPropertyName(prop), orUse);

        [Obsolete("Replace with GetProperty, OptInFor will be removed in version 1.0.")]
        protected T OptInFor<T>(Expression<Func<TSubject, T>> prop, Func<T> valueIfNoOptIn) => GetProperty(prop, valueIfNoOptIn);

        protected T GetProperty<T>(Expression<Func<TSubject, T>> prop, T orUse) => GetProperty(prop, () => orUse);

        [Obsolete("Replace with GetProperty, OptInFor will be removed in version 1.0.")]
        protected T OptInFor<T>(Expression<Func<TSubject, T>> prop, T valueIfNoOptIn) => GetProperty(prop, valueIfNoOptIn);

        public Builder<TSubject> Customize(Action<TSubject> action)
        {
            Customizations.Add(action);
            return this;
        }

        public virtual TSubject Create(int seed = 0)
        {
            TSubject subject;
            if (Instance != null)
                subject = Instance;
            else
            {
                foreach (var setup in Setups)
                {
                    setup();
                }
                subject = Build(seed);
            }

            foreach (var customization in Customizations)
                customization(subject);

            return subject;
        }

        object IBuilder.Create(int seed) => Create(seed);

        public static implicit operator TSubject(Builder<TSubject> builder) => builder.Create();

        public virtual IEnumerable<TSubject> CreateMany(int i)
        {
            var subjects = new List<TSubject>();
            for (var x = 0; x < i; x++)
            {
                subjects.Add(Create(x));
            }

            return subjects;
        }

        protected abstract TSubject Build(int seed);

        public T BuildUsing<T>() where T : IBuilder => BuilderFactoryConvention.Create<T>();

        private string GetPropertyName<T>(Expression<Func<TSubject, T>> expr)
        {
            switch (expr.Body)
            {
                case MemberExpression member:
                    return member.Member.Name;
                case UnaryExpression unary when unary.Operand is MemberExpression unaryMember:
                    return unaryMember.Member.Name;
                default:
                    throw new ArgumentException(
                        $"The property expression should point out a member of the class {typeof(TSubject).Name}" +
                        $", however the expression does not seem to do so.");
            }
        }
    }
}
