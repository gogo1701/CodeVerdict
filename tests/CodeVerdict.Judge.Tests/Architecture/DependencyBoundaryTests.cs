using System.Reflection;
using Xunit;

namespace CodeVerdict.Judge.Tests.Architecture;

public sealed class DependencyBoundaryTests
{
    [Fact]
    public void Judge_does_not_reference_web_or_persistence_assemblies()
    {
        var references = Assembly.Load("CodeVerdict.Judge").GetReferencedAssemblies();

        Assert.DoesNotContain(references, reference =>
            reference.Name!.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
            || reference.Name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
            || reference.Name.StartsWith("CodeVerdict.Web", StringComparison.Ordinal)
            || reference.Name.StartsWith("CodeVerdict.Infrastructure", StringComparison.Ordinal));
    }
}
