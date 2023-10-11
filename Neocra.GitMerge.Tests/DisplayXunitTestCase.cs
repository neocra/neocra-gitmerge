using Humanizer;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Neocra.GitMerge.Tests;

public class DisplayXunitTestCase : XunitTestCase
{
    public DisplayXunitTestCase(IMessageSink diagnosticMessageSink, TestMethodDisplay methodDisplayOrDefault, TestMethodDisplayOptions methodDisplayOptionsOrDefault, ITestMethod testMethod)
        :base(diagnosticMessageSink, methodDisplayOrDefault, methodDisplayOptionsOrDefault, testMethod)
    {
    }

    public DisplayXunitTestCase()
    {
            
    }

    protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName)
    {
        return base.GetDisplayName(factAttribute, displayName).Humanize();
    }
}