using System;

namespace PlaywrightDemo.Infrastructure.Fixtures
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequiredFixturesAttribute : Attribute
    {
        public Type[] Fixtures { get; }

        public RequiredFixturesAttribute(params Type[] fixtures)
        {
            Fixtures = fixtures;
        }
    }
}