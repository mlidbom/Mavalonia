# Backlog

What Magnus has decided to do later, with what was worked out about it so far. An item leaves this file when it's
built, or when he drops it.

## A formatting bar above selected text in `MarkdownEditor`

Deferred by Magnus, 2026-11-27. A bar like ChatGPT's, which shows over text a person selects in the rendered view and
formats it with a click. This is the design as it stood when deferred. Nothing of it is built.

- **What's on it, left to right:**
  - **Block dropdown:** Text, Heading 1, Heading 2, Heading 3, Bulleted list, Numbered list. It shows what the selected
    blocks are, or is blank when they differ.
  - **B, I, `</>` (code), ~~S~~:** each shows pressed when it's on.
  - **Link:** opens a small address box in the bar itself. Enter makes the selection a link. On a link, the box shows
    its address, with a button that removes the link.
- **When it shows:**
  - **Mouse:** when the button is released over a selection.
  - **Keyboard:** when Shift+arrows stop for a moment, about 300 ms, so that it doesn't flicker on every step.
- **When it hides:** when the selection is emptied, on typing, on Escape, when the editor loses the focus, and in the
  source view.
- **Where:** centred over the first line of the selection, inside the editor's edges, and below the selection when
  there's no room above. It floats over the text and moves nothing. It never takes the focus, so keys keep going to the
  editor.
- **Reuse:** each item calls the same `MarkdownEditor.ToggleFormat` and `MarkdownEditor.ToggleList` as the menu, and
  shows the same on and off state (`HasFormat`, `IsInAList`).
- **For applications:** `MarkdownEditor.ShowsTheFormattingBar`, true by default. False turns it off, as
  `ContextFlyout = null` turns off the menu.
- **New edits it needs:**
  - Changing a block between text and a heading of a level. No such edit exists yet; `TogglingAList` does it for lists.
  - Writing a link around the selection, and removing one.
- **Calls made in the design, open to change:** strikethrough is on the bar, though ChatGPT's has fewer formats; level 3
  is the deepest heading in the dropdown.
- **What it costs:** the link is about a third of the work. Its address box needs focus handling of its own, since
  nothing else on the bar takes the focus. The options were to build it all at once, or the bar first and the link as a
  step of its own.
