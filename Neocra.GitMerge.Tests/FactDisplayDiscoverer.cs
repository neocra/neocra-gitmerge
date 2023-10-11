using Xunit.Abstractions;
using Xunit.Sdk;

namespace Neocra.GitMerge.Tests;

public sealed class FactDisplayDiscoverer : FactDiscoverer
{
    public FactDisplayDiscoverer(IMessageSink diagnosticMessageSink) : base(diagnosticMessageSink)
    {
    }

    protected override IXunitTestCase CreateTestCase(ITestFrameworkDiscoveryOptions discoveryOptions, ITestMethod testMethod,
        IAttributeInfo factAttribute)
    {
        return new DisplayXunitTestCase(this.DiagnosticMessageSink, discoveryOptions.MethodDisplayOrDefault(), discoveryOptions.MethodDisplayOptionsOrDefault(), testMethod);

    }
}