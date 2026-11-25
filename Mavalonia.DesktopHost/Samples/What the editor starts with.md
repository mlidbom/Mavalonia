# Try the editor

All of this is edited as it's shown. Ctrl+M, or the Source button, switches to the markdown and back. The Preview tab shows it as it's read.

## Text

Text can be **bold** with Ctrl+B, *italic* with Ctrl+I, `code` with Ctrl+E and ~~struck through~~ with Ctrl+Shift+X, or ***more than one*** at once.
Shift+Enter breaks the line,
like this, and Enter starts a new paragraph.

Links can lead anywhere: [a web page](https://example.org), [a file](file:///C:/Windows/win.ini), [a place in code](src/Answers.cs:12) or [the application itself](cloyd://session/7). Clicking one tells the application, and this one shows that above. Ctrl+click is the usual way to open one.

## Blocks

Typed at the start of a line and followed by a space, these start a block:

- `-`, `*` or `+` starts a list
  - Tab moves an item in, and Shift+Tab out
- `1.` or `1)` starts a numbered list
- `#` up to `######` start a heading
- `>` starts a quote
- three backticks start code

Such as:

1. A numbered list
2. goes on with Enter, and Enter on an empty item ends it

And:

1) A list numbered
2) with parentheses

> A quote, which Enter on an empty line ends.
>
> - [ ] A task, which the preview shows as a box
> - [x] and one that is done

> [!TIP]
> An alert, which the preview shows as GitHub does.

```csharp
// The preview colours code by its language.
public static int Answer(string question) => question.Length > 0 ? 42 : 0;
```

| Tables | are made | by typing |
| --- | --- | --- |
| a row of cells | and then | Enter |
| Tab goes to | the next cell, | or a new row |

---

![A sample image](sample-image.png)

## Copying and pasting

Select in the tabs to the right, or on a web page, copy, and paste here: rich text becomes markdown. Ctrl+Shift+V pastes only the text, and Ctrl+Shift+Q pastes it as a quote. The Quote button quotes what is selected in the tab that is shown. Right-click for a menu.
