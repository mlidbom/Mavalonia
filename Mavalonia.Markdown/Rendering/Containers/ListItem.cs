using System.Globalization;
using JetBrains.Annotations;

namespace Mavalonia.Markdown.Rendering.Containers;

///<summary>An item of a list.</summary>
public class ListItem : BlockContainer
{
   readonly string _marker;
   readonly int _width;

   ///<param name="marker">What the item is written with, such as <c>-</c> or <c>1.</c>.</param>
   ///<param name="number">The item's number, or null when it is bulleted.</param>
   ///<param name="width">How far the text of the item is indented by the item.</param>
   internal ListItem(string marker, int? number, int width)
   {
      _marker = marker;
      _width = width;
      Number = number;
   }

   ///<summary>The item's number in its list: that of the first item written, counted on from there. Null when the item has a bullet.</summary>
   [PublicAPI]
   public int? Number { get; }

   ///<summary>The first item of a new list of <paramref name="kind"/>.</summary>
   internal static ListItem FirstOf(ListKind kind) => kind == ListKind.Bulleted ? new ListItem("-", null, 2) : new ListItem("1.", 1, 3);

   internal ListKind Kind => Number is null ? ListKind.Bulleted : ListKind.Numbered;

   ///<summary>The number as it's shown before the item, such as <c>1.</c>, or <c>1)</c> when it's written so. Null when the item has a bullet.</summary>
   public string? ShownNumber => Number is {} number ? MarkerOf(number) : null;

   ///<summary>What is after the number. Null when the item has a bullet.</summary>
   char? Delimiter => Number is null ? null : _marker[^1];

   string MarkerOf(int number) => number.ToString(CultureInfo.InvariantCulture) + Delimiter;

   internal override string PrefixOfItsFirstLine => _marker.PadRight(_width);
   internal override string PrefixOfItsOtherLines => new(' ', _width);

   internal override int? EndOfItsPrefix(string markdown, int offset)
   {
      var end = offset + _width;
      return end <= markdown.Length && markdown.AsSpan(offset, _width).IsWhiteSpaceWithoutLineBreak() ? end : null;
   }

   internal override bool LooksLike(BlockContainer other) => other is ListItem item && item.Delimiter == Delimiter;

   ///<summary>The item, were it the first of a list.</summary>
   internal ListItem AsTheFirstOfItsList() => Number is null ? this : new ListItem(MarkerOf(1), 1, _width);

   ///<summary>A new item to follow this one in its list.</summary>
   internal ListItem NewItemAfter()
   {
      if(Number is not {} number) return new ListItem(_marker, null, _width);

      var marker = MarkerOf(number + 1);
      return new ListItem(marker, number + 1, Math.Max(_width, marker.Length + 1));
   }
}
