using System;
using System.Collections.Generic;

namespace FluentBuilders.Core
{
    public class ObjectContainer<T> : IBuilder<T>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ObjectContainer(T obj) => Value = obj;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public T Value { get; set; }

        public T1 BuildUsing<T1>() where T1 : IBuilder => throw new NotImplementedException();

        public T Create(int seed = 0) => Value;

        object IBuilder.Create(int seed) => Create(seed);

        public BuilderFactoryConvention BuilderFactoryConvention { get; set; }
        public List<Action> Setups { get; private set; }
    }
}