using System;
using PlaywrightDemo.Pages.UI;
using PlaywrightDemo.Pages.API;

namespace PlaywrightDemo.Infrastructure.Fixtures
{
    public static class FixtureTypes
    {
        public static Type[] Pages(params Type[] pageTypes) => pageTypes;
        public static Type[] Api(params Type[] apiTypes) => apiTypes;
        public static Type[] All(params Type[] types) => types;
    }
}