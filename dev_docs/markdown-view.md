# MarkdownView: showing markdown to be read, and copied out of

Why: Cloyd shows messages with LiveMarkdown, which copies only the displayed text. `MarkdownView` shows them with the model the editor has, in which each
char that is shown knows the markdown it stands for, so that what is selected is copied as markdown. Agreed with Magnus, 2026-11-25. Cloyd itself is changed by other sessions:
see [using the controls in an application](using-the-controls-in-an-application.md).

## Decisions

- **One control for each message**, a drop-in for LiveMarkdown's `MarkdownRenderer`. Selection across messages comes later, once this has proved itself.
- **`RenderedMarkdownView` is what reading and editing share:** the layout, the drawing, the selecting. `MarkdownView` and `RenderedEditorView` are it. Their models, `MarkdownToRead` and `RenderedEditor`, are both a `SelectableMarkdown`.
- **A dialect decides what is markup** (`MarkdownDialect`). Editing turns off what people type without meaning to. Reading is as GitHub reads it. In plain text nothing is markup.
- **Copying** puts markdown on the clipboard as text, HTML for programs that take rich text, and a format of our own, so that pasting in our editor takes the markdown as it is and not the HTML converted back. What is copied leaves out the containers that all of it is inside, except where something means nothing outside them: the title of an alert, the box of a task.
- **Images are loaded only by what the application gives the view to load them with.** A reply of an AI can hold an image whose URL carries what it knows to somebody else, so loading them unasked is unsafe.
- **Code is coloured off the UI thread** and drawn in the colours of code that starts the same until then, so that they stay while it's written. The colours change no sizes.
- **Streaming is setting the markdown again.** Blocks that look the same keep their layouts, and the selection stays when the text before its end is the same.
- **A text that fits keeps its layout at other widths.** Avalonia's `TextRunCache` would make width changes faster still, for long histories. It's a preview API, and saves about 15 ms at 100 messages, so it isn't used.
- **Scrolling draws nothing anew** but what comes within 1,200 pixels of being seen.

## Not done

- Horizontal scrolling, so code and wide tables wrap.
- Selection across messages.
- Rich text on the clipboard of other systems than Windows.
