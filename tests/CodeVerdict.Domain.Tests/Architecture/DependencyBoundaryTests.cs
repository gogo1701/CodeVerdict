using System.Reflection;
using Xunit;

namespace CodeVerdict.Domain.Tests.Architecture;

public sealed class DependencyBoundaryTests
{
    [Fact]
    public void Domain_does_not_reference_framework_or_infrastructure_assemblies()
    {
        var references = Assembly.Load("CodeVerdict.Domain").GetReferencedAssemblies();

        Assert.DoesNotContain(references, reference =>
            reference.Name!.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal)
            || reference.Name.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal)
            || reference.Name.StartsWith("Docker", StringComparison.Ordinal));
    }
}
