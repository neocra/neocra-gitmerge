using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.GitMerge.Tests
{
    public class CsharpTests : BaseTests
    {
        public CsharpTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, "cs")
        {
        }

        [Fact]
        public async Task Should_do_nothing_When_have_same_csharp_file()
        {
            await this.Merge(
                "using System;",
                "using System;",
                "using System;",
                "using System;");
        }
        
        [Fact]
        public async Task Should_merge_using_When_have_csharp_file()
        {
            await this.Merge(
                "using System;",
                "using System;using System.Collections.Generic;",
                "using System;using System.Globalization;",
                "using System;using System.Globalization;using System.Collections.Generic;");
        }
        
        [Fact]
        public async Task Should_merge_using_When_have_a_delete_on_other()
        {
            await this.Merge(
                "using System;using System.Collections.Generic;",
                "using System;using System.Collections.Generic;",
                "using System;",
                "using System;");
        }
        
        [Fact]
        public async Task Should_merge_using_When_have_two_delete()
        {
            await this.Merge(
                "using System;\nusing System.Collections.Generic;\n",
                "using System;",
                "using System;",
                "using System;\n");
        }
        
        [Fact]
        public async Task Should_merge_class_When_have_insert_field()
        {
            await this.Merge(
                "public class Class1\n{\n\n}",
                "public class Class1\n{\n\n}",
                "public class Class1\n{\n private string field;\n}",
                "public class Class1\n{\n private string field;\n}");
        }
        
        [Fact]
        public async Task Should_merge_class_When_have_insert_field_from_current_and_other()
        {
            await this.Merge(
                "public class Class1\n{\n\n}",
                "public class Class1\n{\n    private string field2;\n}",
                "public class Class1\n{\n    private string field1;\n}",
                "public class Class1\n{\n    private string field1;\n    private string field2;\n}");
        }
        
        [Fact]
        public async Task Should_merge_class_When_have_insert_class_in_namespace()
        {
            await this.Merge(
                "namespace Name1\n{\n\n}",
                "namespace Name1\n{\n\n}",
                "namespace Name1\n{\n    public class Class1\n    {\n    }\n}",
                "namespace Name1\n{\n    public class Class1\n    {\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_method_When_have_insert_method_in_class()
        {
            await this.Merge(
                "public class Class1\n{\n\n}",
                "public class Class1\n{\n\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_have_a_statement_in_a_method_in_class()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    int i = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    int i = 0;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_insert_a_statement_in_a_method_with_statements()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n        int a = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int a = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int i = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int i = 0;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_multiple_declarator()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n        int a = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int a = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int a = 0,i = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int a = 0,i = 0;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_declarator_is_moved()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n        int a = 0,i = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int i = 0,a = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int a = 0,i = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        int i = 0,a = 0;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_change_type_of_variable_When_merge_statement()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n   int a = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n   int a = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var a = 0;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var a = 0;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_have_invocation_expression()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n   bool a = HttpMethods.IsPost(httpContext.Request.Method1);\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n   bool a = HttpMethods.IsPost(httpContext.Request.Method1);\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n   bool a = HttpMethods.IsPost(httpContext.Request.Method2);\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n   bool a = HttpMethods.IsPost(httpContext.Request.Method2);\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_have_return_statement()
        {
            await this.Merge(
                "public class Class1\n{\n    public int Method()\n    {\n    return 0;\n    }\n}",
                "public class Class1\n{\n    public int Method()\n    {\n   return 0;\n    }\n}",
                "public class Class1\n{\n    public int Method()\n    {\n    return 1;\n    }\n}",
                "public class Class1\n{\n    public int Method()\n    {\n    return 1;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_have_if_statement()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n        if(1 == 0){ var i = 1; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if(1 == 0){ var i = 1; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if(1 == 0){ var i = 2; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if(1 == 0){ var i = 2; }\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_have_Parenthesized_Expression()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n        if((1 == 0)){ var i = 1; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if((1 == 0)){ var i = 1; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if((1 == 0)){ var i = 2; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if((1 == 0)){ var i = 2; }\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_have_lambda_Parenthesized_Expression()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n         Func<int> i = () => 1;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n         Func<int> i = () => 2;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n         Func<int> i = () => 1;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n         Func<int> i = () => 2;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_have_Unary_Expression()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n        if(!(1 == 0)){ var i = 1; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if(!(1 == 0)){ var i = 1; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if(!(1 == 0)){ var i = 2; }\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        if(!(1 == 0)){ var i = 2; }\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_methods_When_have_Prefix_Unary()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n        var i = (string.Equals(\"toto\",\"toto\");\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        var i = (string.Equals(\"toto\",\"toto\");\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        var i = (string.Equals(\"tato\",\"toto\");\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n        var i = (string.Equals(\"tato\",\"toto\");\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_have_return_statement_task()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    return;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n   return;\n    }\n}",
                "public class Class1\n{\n    public Task Method()\n    {\n    return Task.CompletedTask;\n    }\n}",
                "public class Class1\n{\n    public Task Method()\n    {\n    return Task.CompletedTask;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_have_member_access()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    return Debugger.IsAttached;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n   return Debugger.IsAttached;\n    }\n}",
                "public class Class1\n{\n    public Task Method()\n    {\n    return Debugger.IsAttache;\n    }\n}",
                "public class Class1\n{\n    public Task Method()\n    {\n    return Debugger.IsAttache;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_statement_is_deleted()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    {return;}\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n   {return;}\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    {}\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    {}\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_move_member()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method1()\n    {\n    }\n    public void Method2()\n    {\n    }\n}",
                "public class Class1\n{\n    public void Method2()\n    {\n    }\n    public void Method1()\n    {\n    }\n}",
                "public class Class1\n{\n    public void Method1()\n    {\n    }\n    public void Method2()\n    {\n    }\n}",
                "public class Class1\n{\n    public void Method2()\n    {\n    }\n    public void Method1()\n    {\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_statement_is_moved()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    var i = 1;\n    var a = 2;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var a = 2;\n    var i = 1;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var i = 1;\n    var a = 2;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var a = 2;\n    var i = 1;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_statement_is_moved_and_add_between_statement()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    var i = 1;\n    var a = 2;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var a = 2;\n    var i = 1;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var i = 1;\n    var b = 3;\n    var a = 2;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var b = 3;\n    var a = 2;\n    var i = 1;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_expression_change()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    var i = true || false;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var i = true || false;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var i = true;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var i = true;\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_remove_two_statements()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    var i = 1;\n    var a = 2;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var i = 1;\n    var a = 2;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    }\n}");
        }
        
        [Fact]
        public async Task Should_merge_When_remove_two_statements_and_add_one()
        {
            await this.Merge(
                "public class Class1\n{\n    public void Method()\n    {\n    var i = 1;\n    var a = 2;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var i = 1;\n    var a = 2;\n    var b = 3;\n    }\n}",
                "public class Class1\n{\n    public void Method()\n    {\n    var b = 3;\n    }\n}");
        }
        
               
        [Fact]
        public async Task Should_merge_When_field_setter_change()
        {
            await this.Merge(
                "public class Class1{ private int _f;private int _g; public Class1() { _f = 12345679;_g = 12;}}",
                "public class Class1{ private int _f;private int _g; public Class1() { _f = 12;_g = 134567;}}",
                "public class Class1{ private int _f;private int _g; public Class1() { _f = 12345679;_g = 12;}}",
                "public class Class1{ private int _f;private int _g; public Class1() { _f = 12;_g = 134567;}}");
        }
    }
}