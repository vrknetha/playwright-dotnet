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

        public static Type[] Pages(params Type[] types) => types;
        public static Type[] Api(params Type[] types) => types;
        public static Type[] All(params Type[] types) => types;
    }
}