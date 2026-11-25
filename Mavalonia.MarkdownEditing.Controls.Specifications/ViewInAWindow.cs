using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;
using Mavalonia.MarkdownEditing.Rendering.Texts;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

///<summary>A <see cref="MarkdownView"/> that a person is reading.</summary>
class ViewInAWindow
{
   internal ViewInAWindow(string markdown)
   {
      View = new MarkdownView { Markdown = markdown, Padding = new Thickness(10) };
      View.LinkClicked += (_, click) => ClickedLinks.Add(click.Target);
      Window = new HeadlessWindow(new ScrollViewer { Content = View });
   }

   internal MarkdownView View { get; }
   internal HeadlessWindow Window { get; }
   internal List<string> ClickedLinks { get; } = [];

   internal ScrollViewer ScrollViewer => (ScrollViewer)View.Parent!;

   ///<summary>The middle of the first place in the window where <paramref name="text"/> is shown.</summary>
   internal Point CenterOf(string text) => Window.PointOf(View, BoundsOf(text).Center);

   ///<summary>Just before <paramref name="text"/> where it's first shown.</summary>
   internal Point StartOf(string text) => Window.PointOf(View, new Point(BoundsOf(text).Left + 0.5, BoundsOf(text).Center.Y));

   ///<summary>Just after <paramref name="text"/> where it's first shown.</summary>
   internal Point EndOf(string text) => Window.PointOf(View, new Point(BoundsOf(text).Right - 0.5, BoundsOf(text).Center.Y));

   internal Rect BoundsOf(string text) => View.BoundsOf(new PlainTextRange(View.PlainText.Value.IndexOf(text, StringComparison.Ordinal), text.Length)).First();

   internal ViewInAWindow Select(string from, string to)
   {
      Window.Drag(StartOf(from), EndOf(to));
      return this;
   }

   internal ViewInAWindow Press(PhysicalKey key, RawInputModifiers modifiers = RawInputModifiers.None, int times = 1)
   {
      Window.Press(key, modifiers, times);
      return this;
   }
}
