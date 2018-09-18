using FluentAssertions;
using NUnit.Framework;
using System;

namespace FluentBuilders.Tests
{
    [TestFixture]
    public class When_Using_Nested_Builder
    {
        [Test]
        public void Nested_Lambda_Is_Applied_On_Nested_Builder()
        {
            var builder = new ExampleClassBuilder()
                .WithReferenceProp(x => x.WithStringProp("I am child"));

            var res = builder.Create();

            res.ReferenceProp.StringProp.Should().Be("I am child");
        }

        [Test]
        public void Nested_Lambda_Is_Merged_With_Default_Lambdas()
        {
            var builder = new ExampleClassBuilder()
                .WithReferenceProp(x => x.WithDateTimeProp(DateTime.Now));

            var res = builder.Create();

            res.ReferenceProp.StringProp.Should().Be("I am default");
        }

        [Test]
        public void Nested_Lambda_With_Instance_Uses_The_Instance()
        {
            var something = "I am from instance";

            var someDate = DateTime.Now.AddDays(-1);

            var exampleReferencedClass =
                new ExampleReferencedClassBuilder().WithDateTimeProp(someDate).WithStringProp(something).Create();

            var builder = new ExampleClassBuilder()
                .WithReferenceProp(x => x.WithInstance(exampleReferencedClass));

            var res = builder.Create();

            res.ReferenceProp.StringProp.Should().Be(something);
            res.ReferenceProp.DateProp.Should().Be(someDate);
        }
    }
}
