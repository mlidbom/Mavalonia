# Changelog

All notable changes to the package Mavalonia are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/). Publishing a version needs a section of its own
here: see [releasing](dev_docs/releasing.md).

## [Unreleased]

### Added
- `MarkdownEditor.ToggleList(ListKind)`, and the keys for it in the rendered view, `ToggleBulletedListGesture`
  (Ctrl+Shift+8) and `ToggleNumberedListGesture` (Ctrl+Shift+7): they make each paragraph and heading that is selected,
  or that the caret is in, an item of a bulleted or a numbered list, or take them out of their lists when each is an item
  of such a list already. An item of a list of the other kind turns its whole list into one of the kind, and a
  paragraph after a list of the kind becomes its next item. The keys are those of the digits, whatever Shift and the
  digit type on the keyboard's layout, such as `(` for Shift+8 on a Swedish one.
- The menu of `MarkdownEditor` has bold, italic, code and strikethrough, and bulleted list and numbered list. Each has a
  check mark when what is selected, or what is typed next, has it, and, in the rendered view, the keys that do the same.

### Changed
- In `MarkdownEditor`, the caret moved with the arrow keys stops on both sides of the backticks where code in a line of
  text starts or ends: inside the code, what is typed is code; outside it, it isn't. Inside code, the caret is drawn
  wider, and at an edge over the code's background; outside, it's drawn as before, beside it. Before, it was always
  inside the code at its end, so nothing typed there could follow the code, and always outside it at its start.
  Selecting with Shift and the arrow keys, and moving the caret in `MarkdownView`, pass the backticks as before.

### Fixed
- Toggling code off with nothing selected, at the end of code in a line of text, and typing a space put the space in
  the code, where its background shows; the text after it went outside. The space now goes outside the code too.

## [0.10.1] - 2026-11-26

### Fixed
- The selection and highlights show over code in a line of text, such as `` `code` ``. Its background was drawn over
  them, so an opaque `CodeBackground` hid them. The background is now as high as the line, as the selection is.

## [0.10.0] - 2026-11-26

### Changed
- `MarkdownView` asks for no more width than what it shows needs, as a `TextBlock` does. Aligned to a side rather
  than stretched, such as in the bubble of a message, it's no wider than its widest line; its lines end where they do
  stretched. `MarkdownEditor` still takes all the width it's given.

### Removed
- `LinkClickedEventArgs.GetPosition`, which nothing used: an application shows what belongs at a click at the pointer.

## [0.9.0] - 2026-11-26

### Added
- `MarkdownView.Links`, text that the application shows as links where the markdown has none, such as the names of
  things in its code: ranges of `PlainText`, each with where it leads. They look as the markdown's links do, in the
  colour of links and underlined, in code too, and a click on one raises `LinkClicked`. The markdown, and what is copied
  of it, stay as they are. Setting them again, as the application learns of more, lays nothing out anew.
- `LinkClickedEventArgs.GetPosition`, where the link was clicked, such as to show a menu there.

## [0.8.0] - 2026-11-26

### Added
- `MarkdownEditor`, a WYSIWYG markdown editor whose document is the markdown text. It switches quickly between showing
  the markdown's source and showing it rendered (`ShowsSource`). Enter starts a paragraph and Shift+Enter breaks the
  line. Ctrl+B, Ctrl+I and Ctrl+E format what is selected. It pastes as text and as a quote, and `Quote` quotes what the
  application gives it. The `Pasted` event tells the application what each paste wrote.
- `MarkdownView`, which shows markdown to be read, as GitHub does: tables, task lists, alerts, bare URLs, strikethrough,
  and code coloured by its language. What is selected in it is copied as markdown. Setting `Markdown` again keeps what is
  selected, and lays out only what is new, for a message that is still being written. `Highlights` and `BoundsOf` let an
  application paint and scroll to what a search found. `LinkClicked` tells of a click on a link. Images are not loaded
  from any URL unless the application sets `LoadImage`.
- `RenderedMarkdown`, the text that markdown is shown as, on any thread: for searching it without drawing it.
