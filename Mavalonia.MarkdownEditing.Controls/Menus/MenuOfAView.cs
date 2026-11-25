using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Platform;

namespace Mavalonia.MarkdownEditing.Controls.Menus;

///<summary>The menu that a <see cref="MarkdownView"/> has when the application gives it no other.</summary>
class MenuOfAView : MenuMadeWhenItOpens
{
   readonly MarkdownView _view;

   internal MenuOfAView(MarkdownView view) : base(view) => _view = view;

   private protected override Point PlaceBelowTheCaret => _view.CaretRectangle.BottomLeft;

   string? TargetOfTheLink => RequestedAt is {} point ? _view.TargetOfTheLinkAt(point) : null;

   private protected override IEnumerable<Control> MakeItems()
   {
      var keys = _view.Hotkeys;

      yield return new MenuCommand("Copy", _view.CopySelection, keys.Copy.FirstOrDefault()) { IsEnabled = _view.IsAnythingSelected };
      if(TargetOfTheLink is {} target) yield return new MenuCommand("Copy link", () => TopLevel.GetTopLevel(_view)!.Clipboard!.SetTextAsync(target));
      yield return new Separator();
      yield return new MenuCommand("Select all", _view.SelectAll, keys.SelectAll.FirstOrDefault()) { IsEnabled = _view.PlainText.Value.Length > 0 };
   }
}
