using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Mavalonia.MarkdownEditing.Controls.Menus;

///<summary>A menu of a control whose items are made each time that it opens, for what is selected and pointed at then. It opens at the pointer, or, when it's opened with the keyboard, below the caret.</summary>
abstract class MenuMadeWhenItOpens : MenuFlyout
{
   readonly Control _control;

   private protected MenuMadeWhenItOpens(Control control)
   {
      _control = control;
      _control.AddHandler(Control.ContextRequestedEvent, RememberWhereItIsRequested, RoutingStrategies.Tunnel); // Before it opens.

      Placement = PlacementMode.AnchorAndGravity;
      PlacementAnchor = PopupAnchor.TopLeft;
      PlacementGravity = PopupGravity.BottomRight;
      PlacementConstraintAdjustment = PopupPositionerConstraintAdjustment.SlideX | PopupPositionerConstraintAdjustment.SlideY;
   }

   private protected abstract IEnumerable<Control> MakeItems();

   ///<summary>Just below the caret, from the corner of the control.</summary>
   private protected abstract Point PlaceBelowTheCaret { get; }

   ///<summary>Where the pointer requested the menu, from the corner of the control. Null when the keyboard did.</summary>
   private protected Point? RequestedAt { get; private set; }

   void RememberWhereItIsRequested(object? sender, ContextRequestedEventArgs request) => RequestedAt = request.TryGetPosition(_control, out var point) ? point : null;

   protected override Control CreatePresenter()
   {
      MakeItemsAnew(); // What is added to a menu that has had no items yet isn't seen by a presenter that was made before.
      return base.CreatePresenter();
   }

   protected override void OnOpening(CancelEventArgs args)
   {
      MakeItemsAnew();
      (HorizontalOffset, VerticalOffset) = RequestedAt is null ? PlaceBelowTheCaret : default;
      base.OnOpening(args);
   }

   void MakeItemsAnew()
   {
      Items.Clear();
      foreach(var item in MakeItems()) Items.Add(item);
   }
}
