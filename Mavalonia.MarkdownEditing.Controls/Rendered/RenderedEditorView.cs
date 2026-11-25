using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Mavalonia.MarkdownEditing.Controls.Rendered.Input;
using Mavalonia.MarkdownEditing.Editing;

namespace Mavalonia.MarkdownEditing.Controls.Rendered;

///<summary>Shows a markdown document as it is rendered, and lets a person edit it there.</summary>
public partial class RenderedEditorView : RenderedMarkdownView
{
   readonly BlinkingCaret _blinkingCaret;
   readonly KeyboardInput _keyboard;
   readonly InputMethodClient _inputMethod;

   static RenderedEditorView()
   {
      AffectsRender<RenderedEditorView>(WatermarkProperty);
      TextInputMethodClientRequestedEvent.AddClassHandler<RenderedEditorView>((view, request) => request.Client = view._inputMethod);
   }

   public RenderedEditorView()
   {
      _blinkingCaret = new BlinkingCaret(InvalidateVisual);
      Clipboard = new ClipboardInput(this);
      _keyboard = new KeyboardInput(this);
      _inputMethod = new InputMethodClient(this);
   }

   ///<summary>Edits the document that is shown. Null while none is.</summary>
   public RenderedEditor? Editor
   {
      get;
      set
      {
         if(field is not null) field.Changed -= ShowTheChange;
         field = value;
         if(field is not null) field.Changed += ShowTheChange;
         InvalidateWhatIsShown();
      }
   }

   internal override SelectableMarkdown? Shown => Editor;

   internal IClipboardInput Clipboard { get; }

   void ShowTheChange(object? sender, EventArgs e)
   {
      InvalidateWhatIsShown();
      if(IsFocused) _blinkingCaret.Show();
      _inputMethod.CaretHasMoved();
   }

   private protected override void DrawOver(DrawingContext context)
   {
      if(Watermark is not null && Editor!.Document.TextLength == 0) DrawTheWatermark(context, Watermark);
      if(IsFocused && _blinkingCaret.IsSeen) context.FillRectangle(Appearance.Foreground, CaretRectangle);
   }

   void DrawTheWatermark(DrawingContext context, string watermark)
   {
      using var text = new TextLayout(watermark, new Typeface(FontFamily), FontSize, Appearance.FaintForeground);
      text.Draw(context, Layout.StartOfTheFirstLine);
   }

   protected override void OnGotFocus(FocusChangedEventArgs e)
   {
      base.OnGotFocus(e);
      _blinkingCaret.Show();
      InvalidateVisual();
   }

   protected override void OnLostFocus(FocusChangedEventArgs e)
   {
      base.OnLostFocus(e);
      _blinkingCaret.Hide();
      InvalidateVisual();
   }

   protected override AutomationPeer OnCreateAutomationPeer() => new RenderedEditorAutomationPeer(this);

   ///<summary>A plain click puts the caret in a link, to edit its label.</summary>
   string? TargetOfTheLinkToFollowAt(Point point, KeyModifiers modifiers) => modifiers == Hotkeys.CommandModifiers ? Layout.TargetAt(point) : null;

   internal override void PointAt(Point point, KeyModifiers modifiers) => Cursor = TargetOfTheLinkToFollowAt(point, modifiers) is null ? Cursors.OfText : Cursors.OfALink;

   internal override void ClickAt(Point point, KeyModifiers modifiers)
   {
      if(TargetOfTheLinkToFollowAt(point, modifiers) is {} target) RaiseEvent(new LinkClickedEventArgs(target));
   }

   protected override void OnTextInput(TextInputEventArgs e) => _keyboard.Type(e);
   protected override void OnKeyDown(KeyEventArgs e) => _keyboard.Press(e);

   protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnDetachedFromVisualTree(e);
      _blinkingCaret.Hide();
   }
}
