using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mavalonia.MarkdownEditing.Specifications.CommonMark;

///<summary>One of the examples in the specification of CommonMark, which together use all of it.</summary>
record CommonMarkExample([property: JsonPropertyName("markdown")] string Markdown, [property: JsonPropertyName("example")] int Number)
{
   internal static readonly IReadOnlyList<CommonMarkExample> All =
      JsonSerializer.Deserialize<IReadOnlyList<CommonMarkExample>>(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "CommonMark", "CommonMark-0.31.2-examples.json")))!;

   internal bool DefinesLinks => Markdown.Contains("]:", StringComparison.Ordinal);
   internal bool HasEntities => Markdown.Contains('&', StringComparison.Ordinal);
}
