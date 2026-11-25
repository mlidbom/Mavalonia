# MarkdownView: showing markdown to be read, and copied out of

Why: Cloyd shows messages with LiveMarkdown, which copies only the displayed text. `MarkdownView` shows them with the model the editor has, in which each
char that is shown knows the markdown it stands for, so that what is selected is copied as markdown. Agreed with Magnus, 2026-11-25. Cloyd itself is changed by other sessions.

## Decisions

- **One control for each message**, a drop-in for LiveMarkdown's `MarkdownRenderer`. Selection across messages comes later, once this has proved itself.
- **A dialect decides what is markup** (`MarkdownDialect`). Editing turns off what people type without meaning to. Reading is as GitHub reads it: also underlined headings, indented code, bare URLs as links, task lists, alerts, `<br>`. In plain text nothing is markup.
- **Copying** puts markdown on the clipboard as text, HTML for programs that take rich text, and a format of our own, so that pasting in our editor takes the markdown as it is and not the HTML converted back.
- **Images are loaded only by what the application gives the view to load them with.** A reply of an AI can hold an image whose URL carries what it knows to somebody else, so loading them unasked is unsafe.
- **Code is coloured off the UI thread** and drawn uncoloured until then. The colours change no sizes.
- **Streaming is setting the markdown again.** Blocks that look the same keep their layouts, and the selection stays when the text before its end is the same.
- **Not done:** horizontal scrolling, so code and wide tables wrap.

## Increments

1. Dialects, and what reading adds to what is rendered.
2. Selecting without editing.
3. The control: selecting, copying, links.
4. Code colours and the copy button.
5. Images.
6. Highlights, for searching.
7. Performance: a layout that is used again at other widths, nothing redrawn by scrolling.
8. Accessibility, the desktop host, documentation for those who integrate it.
