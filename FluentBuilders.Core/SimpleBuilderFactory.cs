using System;
using System.Linq;

namespace FluentBuilders.Core
{
    internal class SimpleBuilderFactory:IBuilderFactory
    {
        public T Create<T>() where T : IBuilder
        {
            var type = typeof(T);
            var constructors = type.GetConstructors();
            if (constructors.Any(x => x.GetParameters().Length == 0))
                return (T)Activator.CreateInstance(typeof(T));

            throw new InvalidOperationException($@"Cannot create a new builder of type {type.Name}, because it does not have a parameterless constructor.
You might need to create your own IBuilderFactory that can instantiate the builder with parameters in the constructor.
Put it to use by using BuilderFactoryConvention.UseFactory of the parent builder.");
        }
    }
}