using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Shared.Helper.Test
{
    public static class EqualityHelper
    {
        public static void PropertyValuesAreEqual(object actual, object expected, string[] ignoreList)
        {
            var properties = expected.GetType().GetProperties();
            foreach (var property in properties.Where(x => !ignoreList.Contains(x.Name)))
            {
                var expectedValue = property.GetValue(expected, null);
                var actualValue = property.GetValue(actual, null);
                var list = actualValue as IList;
                if (list != null)
                    AssertListsAreEqual(property, list, (IList) expectedValue);
                else if (!Equals(expectedValue, actualValue))
                    if (property.DeclaringType != null)
                    {
                        Assert.Fail("Property {0}.{1} does not match. Expected: {2} but was: {3}",
                            property.DeclaringType.Name, property.Name, expectedValue, actualValue);
                    }
                    else
                    {
                        throw new ArgumentNullException(nameof(property));
                    }
            }
        }

        private static void AssertListsAreEqual(PropertyInfo property, IList actualList, IList expectedList)
        {
            if (property == null)
                throw new ArgumentNullException(nameof(property));
            if (actualList.Count != expectedList.Count)
                Assert.Fail(
                    "Property {0}.{1} does not match. Expected IList containing {2} elements but was IList containing {3} elements",
                    property.PropertyType.Name, property.Name, expectedList.Count, actualList.Count);

            for (var i = 0; i < actualList.Count; i++)
                if (!Equals(actualList[i], expectedList[i]))
                    Assert.Fail(
                        "Property {0}.{1} does not match. Expected IList with element {1} equals to {2} but was IList with element {1} equals to {3}",
                        property.PropertyType.Name, property.Name, expectedList[i], actualList[i]);
        }

        public static void PropertyValuesAreEqual(object actual, object expected)
        {
            PropertyValuesAreEqual(actual, expected, new string[0]);
        }

        public static void AssertListsAreEqual(IList actualList, IList expectedList, string[] ignoreList)
        {
            foreach (var actualObject in actualList)
            {
                PropertyValuesAreEqual(actualObject,
                    expectedList[actualList.IndexOf(actualObject)], ignoreList);
            }
        }
    }
}