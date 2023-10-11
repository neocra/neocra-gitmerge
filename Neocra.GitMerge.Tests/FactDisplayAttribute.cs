using System;
using Xunit;
using Xunit.Sdk;

namespace Neocra.GitMerge.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
[XunitTestCaseDiscoverer(" Neocra.GitMerge.Tests.FactDisplayDiscoverer", "Neocra.GitMerge.Tests")]
public sealed class FactDisplayAttribute : FactAttribute
{

}