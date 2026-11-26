using JetBrains.Annotations;

namespace Mavalonia.Markdown.Rendering;

///<summary>What counts as markup in a text.</summary>
public sealed class MarkdownDialect
{
   ///<summary>For text that a person is writing. What people type without meaning it as markup isn't any: a line under a text doesn't make that a heading, nor do spaces before it make it code.<br/>
   /// Where a person can write, a blank line is an empty paragraph.</summary>
   [PublicAPI]
   public static readonly MarkdownDialect ForEditing = new();

   ///<summary>For text that is only read. All that GitHub shows is shown, such as headings that are underlined, bare URLs as links, tasks as boxes, and alerts. Comments aren't.</summary>
   public static readonly MarkdownDialect ForReading = new();

   ///<summary>For text that isn't markdown. Nothing in it is markup.</summary>
   public static readonly MarkdownDialect PlainText = new();

   MarkdownDialect() {}
}
