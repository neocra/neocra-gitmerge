using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Neocra.GitMerge.Tests
{
    public class AutomaticTests : BaseTests
    {
        public AutomaticTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
        }
        
        [Theory(DisplayName = "Files used to test use case merge")]
        [MemberData(nameof(GetData), "automatic")]
        [MemberData(nameof(GetData), "automatic-ignore")]
        public async Task Should_merge_automatic_directory(string directory, string ancestor, string current, string other, string expected, string extension)
        {
            var ancestorContent = await File.ReadAllTextAsync(ancestor);
            var currentContent = await File.ReadAllTextAsync(current);
            var otherContent = await File.ReadAllTextAsync(other);
            var result =
                File.Exists(expected) ? await File.ReadAllTextAsync(expected) : "File result not found";

            
            await this.Merge(
                ancestorContent,
                currentContent,
                otherContent,
                result,
                extension);
        }
        
        public static IEnumerable<object[]> GetData(string directoryName)
        {
            foreach (var directory in Directory.GetDirectories(directoryName))
            {
                var ancestor = Path.Combine(directory, "ancestor");

                if (File.Exists(ancestor))
                {
                    yield return new[]
                    {
                        directory,
                        ancestor,
                        Path.Combine(directory, "current"),
                        Path.Combine(directory, "other"),
                        Path.Combine(directory, "result"),
                        directory.Split(".").Last()
                    };
                }
            }
        }
    }

    public class XmlTests : BaseTests
    {
        public XmlTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, "xml")
        {
        }

        [Fact]
        public async Task Should_preserve_white_space_When_merge_two_xml_file()
        {
            await this.Merge(
                "<root>\n</root>",
                "<root>\n</root>",
                "<root>\n    <node1></node1>\n</root>",
                "<root>\n    <node1></node1>\n</root>");
        }
        
        [Fact]
        public async Task Should_preserve_text_When_merge_two_xml_file()
        {
            await this.Merge(
                "<root>A<node1 />B<node1 />C</root>",
                "<root>A<node1 />B<node1 />C</root>",
                "<root>A<node1 />B<node1 />C</root>",
                "<root>A<node1 />B<node1 />C</root>");
        }
        
        [Fact]
        public async Task Should_preserve_order_When_merge_two_xml_file()
        {
            await this.Merge(
                "<root><node1 att=\"val1\" /><node1 att=\"val1\" /></root>",
                "<root><node1 att=\"val1\" /><node1 att=\"val1\" /></root>",
                "<root><node1 att=\"val1\" /><node1 att=\"val2\" /></root>",
                "<root><node1 att=\"val1\" /><node1 att=\"val2\" /></root>");
        }
        
        [Fact]
        public async Task Should_remove_the_second_node1_When_merge_two_xml_file()
        {
            await this.Merge(
                "<root><node2 /><node1 att=\"val1\" /><node1 att=\"val2\" /></root>",
                "<root><node2 /><node1 att=\"val1\" /><node1 att=\"val2\" /></root>",
                "<root><node2 /><node1 att=\"val1\" /></root>",
                "<root><node2 /><node1 att=\"val1\" /></root>");
        }
        
        [Fact]
        public async Task Should_remove_the_second_node1_When_merge_two_xml_file_and_sub_node()
        {
            await this.Merge(
                "<root><sub1 /><sub><node2 /><node1 att=\"val1\" /><node1 att=\"val2\" /></sub></root>",
                "<root><sub1 /><sub><node2 /><node1 att=\"val1\" /><node1 att=\"val2\" /></sub></root>",
                "<root><sub1 /><sub><node2 /><node1 att=\"val1\" /></sub></root>",
                "<root><sub1 /><sub><node2 /><node1 att=\"val1\" /></sub></root>");
        }
        
        [Fact]
        public async Task Should_merge_two_xml_file_When_are_same()
        {
            await this.Merge(
                @"<root></root>",
                @"<root></root>",
                @"<root></root>",
                @"<root></root>");
        }
        
        [Fact]
        public async Task Should_merge_two_xml_file_When_other_add_node()
        {
            await this.Merge(
                @"<root></root>",
                @"<root></root>",
                @"<root><node1></node1></root>",
                @"<root><node1></node1></root>");
        }
        
        [Fact]
        public async Task Should_merge_two_xml_file_When_other_remove_node()
        {
            await this.Merge(
                @"<root><node1></node1><node2></node2></root>",
                @"<root><node1></node1><node2></node2></root>",
                @"<root><node2></node2></root>",
                @"<root><node2></node2></root>");
        }
        
        [Fact]
        public async Task Should_merge_two_xml_file_When_element_is_changed()
        {
            await this.Merge(
                @"<root><node1></node1></root>",
                @"<root><node1></node1></root>",
                @"<root><node2></node2></root>",
                @"<root><node2></node2></root>");
        }
        
        [Fact]
        public async Task Should_merge_two_xml_file_When_element_is_add_in_current()
        {
            await this.Merge(
                @"<root></root>",
                @"<root><node1></node1></root>",
                @"<root></root>",
                @"<root><node1></node1></root>");
        }
        
        [Fact]
        public async Task Should_merge_two_xml_file_When_element_is_add_in_other_with_another_element()
        {
            await this.Merge(
                @"<root></root>",
                @"<root><node1></node1></root>",
                @"<root><node2></node2></root>",
                @"<root><node2></node2><node1></node1></root>");
        }
        
        [Fact]
        public async Task Should_merge_two_xml_file_When_element_is_add_in_other_on_nested()
        {
            await this.Merge(
                @"<root><node1></node1></root>",
                @"<root><node1><subnode1></subnode1></node1></root>",
                @"<root><node1><subnode2></subnode2></node1></root>",
                @"<root><node1><subnode2></subnode2><subnode1></subnode1></node1></root>");
        }

        [Fact]
        public async Task Should_change_attribute_value_When_merge_node_with_attribute()
        {
            await this.Merge(
                @"<root><node1 att1=""val1""></node1></root>",
                @"<root><node1 att1=""val1""></node1></root>",
                @"<root><node1 att1=""val1"" att2=""val1""></node1></root>",
                @"<root><node1 att1=""val1"" att2=""val1""></node1></root>");
        }

        [Fact]
        public async Task Should_get_order_of_attribute_value_When_merge_node_with_attribute()
        {
            await this.Merge(
                @"<root><node1 att1=""val1"" att2=""val1""></node1></root>",
                @"<root><node1 att1=""val1"" att2=""val1""></node1></root>",
                @"<root><node1 att1=""val2"" att2=""val1""></node1></root>",
                @"<root><node1 att1=""val2"" att2=""val1""></node1></root>");
        }

        [Fact]
        public async Task Should_remove_two_node_When_merge_with_multi_node()
        {
            await this.Merge(
                @"<root><node att=""1"" /><node att=""2"" /><node att=""3"" /><node att=""4"" /><node att=""5"" /></root>",
                @"<root><node att=""1"" /><node att=""2"" /><node att=""3"" /><node att=""4"" /><node att=""5"" /></root>",
                @"<root><node att=""1"" /><node att=""3"" /><node att=""5"" /></root>",
            @"<root><node att=""1"" /><node att=""3"" /><node att=""5"" /></root>");
        }
        
        [Fact]
        public async Task Should_remove_nodes_and_update_attribute_on_another_When_merge_with_multi_node()
        {
            await this.Merge(
                @"<root>\n<node att=""1"" v=""val1"" />\n<node att=""2"" v=""val2"" />\n<node att=""3"" v=""val3"" />\n</root>",
                @"<root>\n<node att=""1"" v=""val1"" />\n<node att=""2"" v=""val2"" />\n<node att=""3"" v=""val4"" />\n</root>",
                @"<root>\n<node att=""3"" v=""val3"" />\n</root>",
                @"<root>\n<node att=""3"" v=""val4"" />\n</root>");
        }
        
        [Fact]
        public async Task Should_remove_four_node_When_have_two_node_name_different_and_merge_with_multi_node()
        {
            await this.Merge(
                @"<root><node att=""1"" /><node att=""2"" /><node att=""3"" /><node att=""4"" /><node att=""5"" /><node2 att=""1"" /><node2 att=""2"" /><node2 att=""3"" /><node2 att=""4"" /><node2 att=""5"" /></root>",
                @"<root><node att=""1"" /><node att=""2"" /><node att=""3"" /><node att=""4"" /><node att=""5"" /><node2 att=""1"" /><node2 att=""2"" /><node2 att=""3"" /><node2 att=""4"" /><node2 att=""5"" /></root>",
                @"<root><node att=""1"" /><node att=""3"" /><node att=""5"" /><node2 att=""1"" /><node2 att=""3"" /><node2 att=""5"" /></root>",
                @"<root><node att=""1"" /><node att=""3"" /><node att=""5"" /><node2 att=""1"" /><node2 att=""3"" /><node2 att=""5"" /></root>");
        }
        
        [Fact]
        public async Task Should_remove_one_node_and_add_another_When_merge_with_multi_node()
        {
            await this.Merge(
                @"<root><node att=""1"" /><node att=""2"" /><node att=""3"" /><node att=""4"" /><node att=""5"" /></root>",
                @"<root><node att=""1"" /><node att=""2"" /><node att=""3"" /><node att=""4"" /><node att=""5"" /></root>",
                @"<root><node att=""1"" /><node att=""3"" /><node att=""6"" /><node att=""5"" /></root>",
                @"<root><node att=""1"" /><node att=""3"" /><node att=""6"" /><node att=""5"" /></root>");
        }
        
        [Fact]
        public async Task Should_conflict_When_merge_files_with_a_modification_on_same_attribute()
        {
            await this.MergeConflict(
                @"<root><node att=""1"" /></root>",
                @"<root><node att=""2"" /></root>",
                @"<root><node att=""3"" /></root>");
        }
        
        [Fact]
        public async Task Should_do_not_conflict_When_merge_files_with_a_delete_on_each_files()
        {
            await this.Merge(
                @"<root><node att=""1"" /><node att=""2"" /></root>",
                @"<root><node att=""2"" /></root>",
                @"<root><node att=""2"" /></root>",
            @"<root><node att=""2"" /></root>");
        }
    }
}