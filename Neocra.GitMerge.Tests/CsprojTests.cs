using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.GitMerge.Tests
{
    public class CsprojTests : BaseTests
    {
        public CsprojTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, "csproj")
        {
        }

        [FactDisplay]
        public async Task Should_do_nothing_When_merge_root_node_with_attribute()
        {
            await this.Merge(
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>",
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>",
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>",
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n<Project ToolsVersion=\"4.0\" DefaultTargets=\"Build\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"></Project>");
        }

        [FactDisplay]
        public async Task Should_merge_When_use_xmlns()
        {
            await this.Merge(
                "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><node1 att=\"val1\"></node1></Project>",
                "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><node1 att=\"val1\"></node1></Project>",
                "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><node1 att=\"val2\"></node1></Project>",
                "<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><node1 att=\"val2\"></node1></Project>");
        }
        
        [FactDisplay]
        public async Task Should_merge_two_delete_line_When_use_xmlns()
        {
            await this.Merge(
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.4"" />
        <PackageReference Include=""Azure.Security.KeyVault.Secrets"" Version=""4.1.0"" />
        <PackageReference Include=""FluentValidation.AspNetCore"" Version=""1.0.38"" />
        <PackageReference Include=""MicroElements.Swashbuckle.FluentValidation"" Version=""5.2.0"" />
        <PackageReference Include=""Microsoft.ApplicationInsights.AspNetCore"" Version=""2.18.0"" />
        <PackageReference Include=""Microsoft.AspNetCore.Mvc.NewtonsoftJson"" Version=""5.0.8"" />
        <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer"" Version=""4.2.0"" />
        <PackageReference Include=""Microsoft.Azure.Services.AppAuthentication"" Version=""1.6.1"" />
        <PackageReference Include=""Microsoft.Extensions.Configuration.AzureKeyVault"" Version=""3.1.10"" />
        <PackageReference Include=""Microsoft.FeatureManagement.AspNetCore"" Version=""2.3.0"" />
        <PackageReference Include=""Microsoft.VisualStudio.Validation"" Version=""16.10.34"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.Annotations"" Version=""6.1.1"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.Newtonsoft"" Version=""6.1.1"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.SwaggerGen"" Version=""6.1.1"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.SwaggerUI"" Version=""6.1.5"" />
        <PackageReference Include=""System.Text.Json"" Version=""5.0.2"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.4"" />
        <PackageReference Include=""Azure.Security.KeyVault.Secrets"" Version=""4.1.0"" />
        <PackageReference Include=""FluentValidation.AspNetCore"" Version=""1.0.38"" />
        <PackageReference Include=""MicroElements.Swashbuckle.FluentValidation"" Version=""5.2.0"" />
        <PackageReference Include=""Microsoft.ApplicationInsights.AspNetCore"" Version=""2.18.0"" />
        <PackageReference Include=""Microsoft.AspNetCore.Mvc.NewtonsoftJson"" Version=""5.0.8"" />
        <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer"" Version=""4.2.0"" />
        <PackageReference Include=""Microsoft.Azure.Services.AppAuthentication"" Version=""1.6.1"" />
        <PackageReference Include=""Microsoft.Extensions.Configuration.AzureKeyVault"" Version=""3.1.10"" />
        <PackageReference Include=""Microsoft.FeatureManagement.AspNetCore"" Version=""2.3.0"" />
        <PackageReference Include=""Microsoft.VisualStudio.Validation"" Version=""16.10.34"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.Annotations"" Version=""6.1.1"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.Newtonsoft"" Version=""6.1.1"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.SwaggerGen"" Version=""6.1.5"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.SwaggerUI"" Version=""6.1.5"" />
        <PackageReference Include=""System.Text.Json"" Version=""5.0.2"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.4"" />
        <PackageReference Include=""Azure.Security.KeyVault.Secrets"" Version=""4.1.0"" />
        <PackageReference Include=""FluentValidation.AspNetCore"" Version=""1.0.38"" />
        <PackageReference Include=""MicroElements.Swashbuckle.FluentValidation"" Version=""5.2.0"" />
        <PackageReference Include=""Microsoft.ApplicationInsights.AspNetCore"" Version=""2.18.0"" />
        <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer"" Version=""4.2.0"" />
        <PackageReference Include=""Microsoft.Azure.Services.AppAuthentication"" Version=""1.6.1"" />
        <PackageReference Include=""Microsoft.Extensions.Configuration.AzureKeyVault"" Version=""3.1.10"" />
        <PackageReference Include=""Microsoft.FeatureManagement.AspNetCore"" Version=""2.3.0"" />
        <PackageReference Include=""Microsoft.VisualStudio.Validation"" Version=""16.10.34"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.Annotations"" Version=""6.1.1"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.SwaggerGen"" Version=""6.1.1"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.SwaggerUI"" Version=""6.1.5"" />
        <PackageReference Include=""System.Text.Json"" Version=""5.0.2"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.4"" />
        <PackageReference Include=""Azure.Security.KeyVault.Secrets"" Version=""4.1.0"" />
        <PackageReference Include=""FluentValidation.AspNetCore"" Version=""1.0.38"" />
        <PackageReference Include=""MicroElements.Swashbuckle.FluentValidation"" Version=""5.2.0"" />
        <PackageReference Include=""Microsoft.ApplicationInsights.AspNetCore"" Version=""2.18.0"" />
        <PackageReference Include=""Microsoft.AspNetCore.Mvc.Versioning.ApiExplorer"" Version=""4.2.0"" />
        <PackageReference Include=""Microsoft.Azure.Services.AppAuthentication"" Version=""1.6.1"" />
        <PackageReference Include=""Microsoft.Extensions.Configuration.AzureKeyVault"" Version=""3.1.10"" />
        <PackageReference Include=""Microsoft.FeatureManagement.AspNetCore"" Version=""2.3.0"" />
        <PackageReference Include=""Microsoft.VisualStudio.Validation"" Version=""16.10.34"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.Annotations"" Version=""6.1.1"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.SwaggerGen"" Version=""6.1.5"" />
        <PackageReference Include=""Swashbuckle.AspNetCore.SwaggerUI"" Version=""6.1.5"" />
        <PackageReference Include=""System.Text.Json"" Version=""5.0.2"" />
    </ItemGroup>
</Project>");
        }
        
        [FactDisplay]
        public async Task Should_fix_with_up_version_When_conflict_merge_on_version()
        {
            await this.Merge(
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.1"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.3"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.2"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.3"" />
    </ItemGroup>
</Project>");
        }
        
        [FactDisplay]
        public async Task Should_fix_with_a_version_When_conflict_merge_same_version()
        {
            await this.Merge(
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.1"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.3"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.3"" />
    </ItemGroup>
</Project>",
                @"<Project Sdk=""Microsoft.NET.Sdk.Web"">
    <ItemGroup>
        <PackageReference Include=""AspNetCore.HealthChecks.CosmosDb"" Version=""5.0.3"" />
    </ItemGroup>
</Project>");
        }
    }
}