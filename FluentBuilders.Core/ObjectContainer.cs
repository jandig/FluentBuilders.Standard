﻿using System;
using System.Collections.Generic;

namespace FluentBuilders.Core
{
    public class ObjectContainer<T> : IBuilder<T>
    {
        public T Value { get; set; }

        public ObjectContainer(T obj)
        {
            Value = obj;
        }
        
        public T1 BuildUsing<T1>() where T1 : IBuilder
        {
            throw new NotImplementedException();
        }

        public T Create(int seed = 0)
        {
            return Value;
        }

        object IBuilder.Create(int seed)
        {
            return Create(seed);
        }

        public BuilderFactoryConvention BuilderFactoryConvention { get; set; }
        public List<Action> Setups { get; private set; }
    }
}
