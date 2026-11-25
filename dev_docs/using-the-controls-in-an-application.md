# Using the controls in an application

Written for the sessions that put them into Cloyd, and for any other application. What Cloyd has was read on 2026-11-25, in its commit 47f6efa9.

## Getting them

- Reference the project `Mavalonia.MarkdownEditing.Controls`. It brings `Mavalonia.MarkdownEditing`. Nothing is published as a package.
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
| `MarkdownTextBlock` for rows that aren't markdown, so that search paints them alike | `MarkdownView { Dialect = MarkdownDialect.PlainText, Markdown = ... }` |
| Images not loaded, since `ImageBasePath` isn't set | Nothing: images are shown as their descriptions, unless `LoadImage` is set. **Don't load images from any URL:** one in an AI's reply can carry what it knows to somebody else. |
| Swallowing the request to bring a clicked message into view | Nothing: getting the focus scrolls nothing. Selecting by dragging or with the keyboard brings the end of the selection into view. |

Copying: Ctrl+C in the view, or `CopySelection()`, such as for a menu. `SelectedMarkdown` is there for a "Quote" command:

```csharp
editor.Quote(view.SelectedMarkdown);
```

Something is selected in one `MarkdownView` of a window at a time. A selection doesn't reach across views yet.

It shows what GitHub does: tables, task lists, alerts, bare URLs, strikethrough. HTML is shown as its source, except `<br>` and comments. Footnotes, math and diagrams are shown as they're written. Code and wide tables wrap: there's no horizontal scrolling.

**How many to show at a time:** 100 messages of 700 characters take 0.4 s to show and 15 MB; LiveMarkdown takes 1.5 s and 44 MB for the same. 15,000 would take half a minute and well over a gigabyte, so keep drawing the latest 100 and more on scrolling up, as Cloyd does. `Mavalonia.MarkdownEditing.Controls.Measurements` measures it.

## Writing messages: `MarkdownEditor`

```xml
<controls:MarkdownEditor x:Name="Editor" Watermark="Write something" />
```

- **`Document`** is an AvaloniaEdit `TextDocument`. Its text is the markdown, and all that is edited, so drafts are swapped by setting it, and stretches of it tracked with `TextSegment`s.
- **`ShowsSource`** switches between the source and what it's rendered as. Bind it to whatever is to switch it: the editor takes no key for that.
- **The application decides the keys:** `StartNewBlockGesture` (Enter), `BreakLineGesture` (Shift+Enter), `PasteTextGesture` (Ctrl+Shift+V), `PasteAsQuoteGesture` (Ctrl+Shift+Q). Null for none. A key it doesn't use goes on to the application, such as Ctrl+Enter to send.
- **`Quote(markdown)`** adds a quote where the caret is and puts the caret after it. It works while the control isn't shown.
- **Pasting images** is the application's: the editor leaves the clipboard alone when there's only an image on it.
- **An exception while using the clipboard,** such as when another program holds it, surfaces on the dispatcher as any other does.
