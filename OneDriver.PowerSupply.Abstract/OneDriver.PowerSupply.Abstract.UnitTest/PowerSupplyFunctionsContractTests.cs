using System.Reflection;
using OneDriver.PowerSupply.Abstract.Contracts;

namespace OneDriver.PowerSupply.Abstract.UnitTest;

public class PowerSupplyFunctionsContractTests
{
    [Fact]
    public void IPowerSupply_ShouldContainExpectedMethodSignatures()
    {
        var type = typeof(IPowerSupplyFunctions);
        var allMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);

        var expectedMethods = new[]
        {
            new {
                Name = "SetVolts",
                ReturnType = typeof(int),
                Parameters = new[] { typeof(int), typeof(double) }
            },
            new {
                Name = "SetAmps",
                ReturnType = typeof(int),
                Parameters = new[] { typeof(int), typeof(double) }
            },
            new {
                Name = "AllChannelsOn",
                ReturnType = typeof(int),
                Parameters = Type.EmptyTypes
            },
            new {
                Name = "AllChannelsOff",
                ReturnType = typeof(int),
                Parameters = Type.EmptyTypes
            }
        };

        foreach (var expected in expectedMethods)
        {
            var match = allMethods.FirstOrDefault(m =>
                m.Name == expected.Name &&
                m.ReturnType == expected.ReturnType &&
                m.GetParameters().Select(p => p.ParameterType).SequenceEqual(expected.Parameters));

            Assert.True(match != null, $"Method {expected.Name}({string.Join(", ", expected.Parameters.Select(p => p.Name))}) with return type {expected.ReturnType.Name} was not found.");
        }
    }
}