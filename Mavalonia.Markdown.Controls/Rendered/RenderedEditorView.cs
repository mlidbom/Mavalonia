using Avalonia;
using Avalonia.Automation.Peers;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using JetBrains.Annotations;
using Mavalonia.Markdown.Controls.Clipboards;
using Mavalonia.Markdown.Controls.Rendered.Input;
using Mavalonia.Markdown.Editing;

namespace Mavalonia.Markdown.Controls.Rendered;

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

   [PublicAPI]
   public RenderedEditorView()
   {
      _blinkingCaret = new BlinkingCaret(InvalidateVisual);
      ClipboardInput = new ClipboardInput(this);
      _keyboard = new KeyboardInput(this);
      _inputMethod = new InputMethodClient(this);
   }

   ///<summary>Edits the document that is shown. Null while none is.</summary>
   [PublicAPI]
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

   internal IClipboardInput ClipboardInput { get; }

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

   ///<summary>A link is text to put the caret in, unless the keys are pressed that links are followed with in any editor.</summary>
   internal override void PointAt(Point point, KeyModifiers modifiers) => Cursor = modifiers == this.Hotkeys.CommandModifiers && TargetAt(point) is not null ? Cursors.OfALink : Cursors.OfText;

   protected override void OnTextInput(TextInputEventArgs e) => _keyboard.Type(e);
   protected override void OnKeyDown(KeyEventArgs e) => _keyboard.Press(e);

   protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
   {
      base.OnDetachedFromVisualTree(e);
      _blinkingCaret.Hide();
   }
}
