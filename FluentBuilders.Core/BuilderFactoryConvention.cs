namespace FluentBuilders.Core
{
    public class BuilderFactoryConvention
    {
        private IBuilderFactory _factory;

        public BuilderFactoryConvention()
        {
            _factory = new SimpleBuilderFactory();
        }

        public void UseFactory(IBuilderFactory factory)
        {
            _factory = factory;
        }

        internal T Create<T>() where T : IBuilder
        {
            var builder = _factory.Create<T>();
            builder.BuilderFactoryConvention.UseFactory(_factory);
            return builder;
        }
    }
}
