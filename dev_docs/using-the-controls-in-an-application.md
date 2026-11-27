# Using the controls in an application

Written for the sessions that put them into Cloyd, and for any other application. What Cloyd has was read on 2026-11-25, in its commit 47f6efa9.

## Getting them

- Reference the package `Mavalonia`, on nuget.org. It holds both `Mavalonia.Markdown.Controls` and `Mavalonia.Markdown`.
- In this repository, reference the project `Mavalonia.Markdown.Controls`, and `Mavalonia.Markdown` too where its types are used: the controls' reference to it is private, so that the package holds it.
- They target `net11.0` and need at least: Avalonia 12.1.5, Avalonia.AvaloniaEdit 12.0.0, Markdig 1.6.0, AngleSharp 1.11.0, TextMateSharp.Grammars 2.0.5. Where an application pins a lower version centrally, raise it: Cloyd has Markdig 1.1.2.
- Any theme works. Where a brush isn't set, it's the one that the Fluent theme has, or else a fixed one.

## Showing messages: `MarkdownView`

```xml
<controls:MarkdownView Markdown="{Binding Text}" HeadingFontSizes="{...}" LinkClicked="OpenTheLink" />
```

It is as high as what it shows, and doesn't scroll itself: put it in what scrolls.

| What Cloyd does with LiveMarkdown | What to do with `MarkdownView` |
| --- | --- |
| `MarkdownRenderer { MarkdownBuilder = ... }` in `ChatMarkdown.Render` | `MarkdownView { Markdown = ... }` |
| A new renderer for each piece of a streamed message | Set `Markdown` of the same view again. What is selected then stays selected, and only what is new is laid out. |
| `LinkClick`, handled in `SessionChat` | `LinkClicked`, which bubbles. `Target` is what the markdown says: check it before opening it. |
| Resource keys for colours, in `ColoursOfTheChat` | `Foreground`, `FaintForeground`, `LinkForeground`, `CodeBackground`, `LineBrush`, `SelectionBrush` |
| Heading sizes 17.5, 14, 14 | `HeadingFontSizes = [17.5, 14]` |
| `CodeBlockColorTheme`, DarkPlus or LightPlus by the theme | Nothing: that's what it does. `ColoursCode = false` turns it off. |
| `ToPlainText` for text to search without drawing it | `RenderedMarkdown.Of(markdown, MarkdownDialect.ForReading).PlainText.Value`, on any thread |
| `ApplyTextSearch`, named highlights with priorities | `Highlights`, ranges of `PlainText` with a brush each, the uppermost last. Set them again when `Markdown` is. |
| `GetTextRangeBoundsInControl` of a match | `BoundsOf(range)` |
| Rewriting the markdown to link names, such as of code | `Links`, ranges of `PlainText` with a target each, which are drawn and clicked as the markdown's links are, in code too. The markdown, and what is copied of it, stay as they are. Find them on any thread in `RenderedMarkdown.Of(markdown, MarkdownDialect.ForReading)`: its `PlainText`, and each block's `Text.StretchesOfTheSameFormatting`, which tell of code and of the markdown's own links. Set them again when `Markdown` is, or when the names change: that lays out nothing. |
| `MarkdownTextBlock` for rows that aren't markdown, so that search paints them alike | `MarkdownView { Dialect = MarkdownDialect.PlainText, Markdown = ... }` |
| Images not loaded, since `ImageBasePath` isn't set | Nothing: images are shown as their descriptions, unless `LoadImage` is set. **Don't load images from any URL:** one in an AI's reply can carry what it knows to somebody else. The rich text that is copied has links to images instead of them, for the same reason. |
| Swallowing the request to bring a clicked message into view | Nothing: getting the focus scrolls nothing. Selecting by dragging or with the keyboard brings the end of the selection into view. |

Copying: Ctrl+C in the view, or `Copy()`. `SelectedMarkdown` is there for a "Quote" command, and `IsAnythingSelected` can be bound to enable it:

```csharp
editor.Quote(view.SelectedMarkdown);
```

Something is selected in one `MarkdownView` of a window at a time. A selection doesn't reach across views yet.

It shows what GitHub does: tables, task lists, alerts, bare URLs, strikethrough. HTML is shown as its source, except `<br>` and comments. Footnotes, math and diagrams are shown as they're written. Code and wide tables wrap: there's no horizontal scrolling.

**How many to show at a time:** 100 messages of 700 characters take 0.4 s to show and 15 MB; LiveMarkdown takes 1.5 s and 44 MB for the same. 15,000 would take half a minute and well over a gigabyte, so keep drawing the latest 100 and more on scrolling up, as Cloyd does. `Mavalonia.Markdown.Controls.Measurements` measures it.

## Links

`Target` is what the markdown says, whatever the protocol: `file:`, `vscode:`, one of the application's own, or a path such as `src/x.cs:12`. Nothing is left out, `javascript:` neither, so check it before opening it.

| | Links, with any protocol | Text that is a link by itself |
| --- | --- | --- |
| `MarkdownView` | `[label](target)`, `[label][reference]`, `<protocol:...>` | `http:`, `https:`, `ftp:`, `mailto:`, `tel:`, `www.` |
| `MarkdownEditor`, rendered | The same | Nothing |
| `MarkdownEditor`, source | The same, anywhere in the markdown of the link | `http:`, `https:`, `ftp:`, `www.`, email addresses |

## Menus

A right-click, or the menu key, opens a small menu: in `MarkdownView` copy, copy link where it's opened at one, and select all; in `MarkdownEditor` cut, copy, paste, paste as text, paste as quote and select all, then bold, italic, code and strikethrough, then bulleted list and numbered list. Each format and kind of list has a check mark when what is selected, or what is typed next, has it, and each item tells the keys that do the same. The right button puts the caret where it's pressed, unless it's pressed in what is selected.

Setting `ContextMenu` or `ContextFlyout`, which all Avalonia controls have, replaces the menu. `ContextFlyout = null` turns it off. The application's menu can't have our items besides its own, so it makes its own of the public actions. Such as one that quotes into Cloyd's writing box:

```csharp
string? target = null;
var copy = new MenuItem { Header = "Copy" };
copy.Click += async (_, _) => await view.Copy();
var quote = new MenuItem { Header = "Quote" };
quote.Click += (_, _) => editor.Quote(view.SelectedMarkdown);
var copyLink = new MenuItem { Header = "Copy link" };
copyLink.Click += async (_, _) => await TopLevel.GetTopLevel(view)!.Clipboard!.SetTextAsync(target!);

view.ContextMenu = new ContextMenu { ItemsSource = new[] { copy, quote, copyLink } };
view.AddHandler(InputElement.ContextRequestedEvent, (_, request) =>
{
   target = request.TryGetPosition(view, out var point) ? view.TargetOfTheLinkAt(point) : null;
   copyLink.IsVisible = target is not null;
   copy.IsEnabled = quote.IsEnabled = view.IsAnythingSelected;
}, RoutingStrategies.Tunnel);
```

`RoutingStrategies.Tunnel` is what has it run before the menu opens: a handler of the `ContextRequested` event isn't called at all when the menu was set before it was added.

A `MenuFlyout` without items in it when it first opens stays empty, also when its `Opening` handler adds some: an Avalonia bug, or so it seems, as of 12.1.5.

## Writing messages: `MarkdownEditor`

```xml
<controls:MarkdownEditor x:Name="Editor" Watermark="Write something" />
```

- **`Document`** is an AvaloniaEdit `TextDocument`. Its text is the markdown, and all that is edited, so drafts are swapped by setting it, and stretches of it tracked with `TextSegment`s.
- **`ShowsSource`** switches between the source and what it's rendered as. Bind it to whatever is to switch it: the editor takes no key for that.
- **Keys that the application chooses,** for each editor, null for none. In both views: `PasteTextGesture` (Ctrl+Shift+V), `PasteAsQuoteGesture` (Ctrl+Shift+Q), `ToggleBoldGesture` (Ctrl+B), `ToggleItalicGesture` (Ctrl+I), `ToggleCodeGesture` (Ctrl+E), `ToggleStrikethroughGesture` (Ctrl+Shift+X), `ToggleBulletedListGesture` (Ctrl+Shift+8), `ToggleNumberedListGesture` (Ctrl+Shift+7). In the rendered view: `StartNewBlockGesture` (Enter), `BreakLineGesture` (Shift+Enter). In the source view, the keys for formats do nothing when nothing is selected, nor those for lists in code: the application gets them then. The keys of the lists are the keys of the digits, whatever Shift and a digit type on the keyboard's layout.
- **Keys that are Avalonia's,** for the whole application: those to copy, cut, paste, undo, redo and select all are those of `PlatformHotkeyConfiguration`, as are the modifier keys for moving and deleting by word and for going to the start or end. Other keys, such as the arrows, are fixed. In the source view, the keys that aren't named here are AvaloniaEdit's own.
- **A key it doesn't use goes on to the application,** such as Ctrl+Enter to send. To have one that it does use, handle it in the tunnel phase.
- **What the keys and the menu do is public,** for buttons and menus of the application: `Cut()`, `Copy()`, `Paste()`, `PasteText()`, `PasteAsQuote()`, `SelectAll()`, `ToggleFormat(TextFormats)` and `ToggleList(ListKind)`. They do it in the view that is shown.
- **`LinkClicked`** is the same event as that of `MarkdownView`, so one handler on what both are in is enough. Its `Source` is the `MarkdownEditor` or the `MarkdownView`. A click raises it, in both views, and the editor itself opens nothing. The click puts the caret in the link too, so an application may want to open only what is clicked with Ctrl: see the event's `KeyModifiers`. Links are those that markdown has, and, in the source view, URLs too, wherever they are.
- **`Quote(markdown)`** adds a quote where the caret is and puts the caret after it. It works while the control isn't shown. It returns the stretch of `Document` that it wrote.
- **`Pasted`** tells of each paste, in either view, by a key, the menu or `Paste()`, `PasteText()` and `PasteAsQuote()`, with the stretch of `Document` that it wrote (`Written`): such as for Cloyd, which sends what was pasted apart from what was typed. In the rendered view, the stretch may hold markup around what was pasted that was written anew, such as where bold text is split by a pasted list. Nothing pasted, no event.
- **Pasting images** is the application's. The editor takes Ctrl+V also when there's only an image on the clipboard, and then inserts nothing: handle the key before it does, in the tunnel phase.
- **An exception while using the clipboard,** such as when another program holds it, surfaces on the dispatcher as any other does.
