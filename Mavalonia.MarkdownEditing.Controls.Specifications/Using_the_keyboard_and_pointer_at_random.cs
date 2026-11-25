using Avalonia;
using Avalonia.Input;
using Compze.Must;
using Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

namespace Mavalonia.MarkdownEditing.Controls.Specifications;

///<summary>Edits in ways that nobody has thought of. The ways are the same each time the specification runs.</summary>
public class Using_the_keyboard_and_pointer_at_random
{
   const string Markdown = "# a‸\n\n> b **c**\n>\n> - d\n>   - e\n\n| A | B |\n|---|---|\n| 1 |  |\n\n```\nx\n\ny\n```\n\n1. f\n2. g\n\n---\n\nh";

   static readonly PhysicalKey[] Keys =
   [
      PhysicalKey.Enter, PhysicalKey.Backspace, PhysicalKey.Delete, PhysicalKey.Tab, PhysicalKey.ArrowUp, PhysicalKey.ArrowDown, PhysicalKey.ArrowLeft, PhysicalKey.ArrowRight, PhysicalKey.Home, PhysicalKey.End, PhysicalKey.B, PhysicalKey.E, PhysicalKey.Z
   ];

   static readonly RawInputModifiers[] Modifiers = [RawInputModifiers.None, RawInputModifiers.None, RawInputModifiers.Shift, RawInputModifiers.Control];
   static readonly string[] Typed = ["a", " ", "**", "`", "- ", "> ", "|", "word "];

   [XFOnUIThread] public void never_fails()
   {
      for(var seed = 0; seed < 8; seed++)
      {
         var random = new Random(seed);
         var editor = new EditorInAWindow(Markdown);
         for(var time = 0; time < 60; time++) DoSomethingAtRandom(editor, random);
         editor.Editor.Document.Text.Must().NotBeNull();
      }
   }

   static void DoSomethingAtRandom(EditorInAWindow editor, Random random)
   {
      switch(random.Next(6))
      {
         case 0 or 1: editor.Type(AnyOf(Typed)); break;
         case 2 or 3: editor.Press(AnyOf(Keys), AnyOf(Modifiers)); break;
         case 4: editor.Window.Click(new Point(random.Next(800), random.Next(600)), AnyOf(Modifiers)); break;
         case 5: editor.Editor.ShowsSource = !editor.Editor.ShowsSource; break;
      }

      T AnyOf<T>(T[] all) => all[random.Next(all.Length)];
   }
}
