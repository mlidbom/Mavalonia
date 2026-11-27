namespace Mavalonia.Markdown.Rendering.Containers;

///<summary>What is shown before each item of a list: a bullet, or a number.</summary>
public enum ListKind
{
   ///<summary>A bullet, written <c>-</c>, <c>*</c> or <c>+</c>.</summary>
   Bulleted,

   ///<summary>A number, written such as <c>1.</c> or <c>1)</c>.</summary>
   Numbered
}
