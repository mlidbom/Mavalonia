# Changelog

All notable changes to the package Mavalonia are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/). Publishing a version needs a section of its own
here: see [releasing](dev_docs/releasing.md).

## [Unreleased]

## [0.8.0] - 2026-11-26

The first release.

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
