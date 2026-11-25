using System.Globalization;

namespace Mavalonia.MarkdownEditing.Rendering.Containers;

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

   ///<summary>The number shown before the item, or null when it has a bullet.</summary>
   public int? Number { get; }

   internal override string PrefixOfItsFirstLine => _marker.PadRight(_width);
   internal override string PrefixOfItsOtherLines => new(' ', _width);

   internal override int? EndOfItsPrefix(string markdown, int offset)
   {
      var end = offset + _width;
      return end <= markdown.Length && markdown.AsSpan(offset, _width).IsWhiteSpaceWithoutLineBreak() ? end : null;
   }

   internal override bool LooksLike(BlockContainer other) => other is ListItem item && item.Number is null == Number is null;

   ///<summary>The item, were it the first of a list.</summary>
   internal ListItem AsTheFirstOfItsList() => Number is null ? this : new ListItem("1" + _marker[^1], 1, _width);

   ///<summary>A new item to follow this one in its list.</summary>
   internal ListItem NewItemAfter()
   {
      if(Number is not {} number) return new ListItem(_marker, null, _width);

      var marker = (number + 1).ToString(CultureInfo.InvariantCulture) + _marker[^1];
      return new ListItem(marker, number + 1, Math.Max(_width, marker.Length + 1));
   }
}
