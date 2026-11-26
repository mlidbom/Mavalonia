using Avalonia;

namespace Mavalonia.Markdown.Controls;

static class PointEX
{
   ///<summary>How far the pointer may move while a button is pressed without that being more than a click.</summary>
   const double FarthestInAClick = 4;

   extension(Point @this)
   {
      ///<summary>Whether a button that is pressed at <paramref name="other"/> and released here, or the other way around, is clicked.</summary>
      internal bool IsWithinAClickOf(Point other) => Point.Distance(@this, other) < FarthestInAClick;
   }
}
