# MarkdownView: showing markdown to be read, and copied out of

Why: Cloyd shows messages with LiveMarkdown, which copies only the displayed text. `MarkdownView` shows them with the model the editor has, in which each
char that is shown knows the markdown it stands for, so that what is selected is copied as markdown. Agreed with Magnus, 2026-11-25. Cloyd itself is changed by other sessions:
see [using the controls in an application](using-the-controls-in-an-application.md).

## Decisions

- **One control for each message**, a drop-in for LiveMarkdown's `MarkdownRenderer`. Selection across messages comes later, once this has proved itself.
- **`RenderedMarkdownView` is what reading and editing share:** the layout, the drawing, the selecting. `MarkdownView` and `RenderedEditorView` are it. Their models, `MarkdownToRead` and `RenderedEditor`, are both a `SelectableMarkdown`.
- **A dialect decides what is markup** (`MarkdownDialect`). Editing turns off what people type without meaning to. Reading is as GitHub reads it. In plain text nothing is markup.
- **Copying** puts markdown on the clipboard as text, HTML for programs that take rich text, and a format of our own, so that pasting in our editor takes the markdown as it is and not the HTML converted back. What is copied leaves out the containers that all of it is inside, except where something means nothing outside them: the title of an alert, the box of a task.
- **A small menu that the application replaces as a whole,** rather than one that it adds items to: simpler, for the cost of making the standard items again. The application knows what else belongs in it, such as quoting into its writing box.
- **Images are loaded only by what the application gives the view to load them with.** A reply of an AI can hold an image whose URL carries what it knows to somebody else, so loading them unasked is unsafe.
- **Code is coloured off the UI thread,** once it's drawn, and drawn in the colours of code that starts the same until then, so that they stay while it's written. What it was before it was written on isn't coloured.
- **Colours change no sizes:** code is laid out without them, and then drawn in them line by line. Avalonia wraps the stretch of each colour alone, so laid out with them, long lines would be wrapped elsewhere.
- **The application can show text as links where the markdown has none** (`Links`), such as names in code, which Cloyd opens in the IDE. The markdown stays as it is, so that what is copied or quoted holds no links of the application's. They look as the markdown's links do, in the colour of links and underlined, in code too, over its colours (Magnus, 2026-11-26: "The styling that the old symbollinker java code had was fine, go with that", and "Why would code not link symbols?"). Like the colours of code, they are drawn on the lines laid out without them, so setting them, as the application learns of more names, lays out nothing and moves no line's end. Where one overlaps a link of the markdown, that of the markdown is clicked.
- **A table that is too wide** has its narrow columns as wide as their text. The others share the rest.
- **What nobody writes doesn't fail:** what is inside one another too deep is shown as its source, and NULs as the character that replaces them.
- **Streaming is setting the markdown again.** Blocks that look the same keep their layouts, and the selection stays when the text before its end is the same.
- **A text that fits keeps its layout at other widths.** Avalonia's `TextRunCache` would make width changes faster still, for long histories. It's a preview API, and saves about 15 ms at 100 messages, so it isn't used.
- **Scrolling draws nothing anew** but what comes within 1,200 pixels of being seen.

## Not done

- Horizontal scrolling, so code and wide tables wrap.
- Selection across messages.
- Rich text on the clipboard of other systems than Windows.
- The alignment of the columns of tables.
