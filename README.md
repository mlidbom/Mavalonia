# Mavalonia

Controls for Avalonia®. The first is a WYSIWYG markdown editor that switches quickly between showing the markdown source and showing it rendered.

| Project | Responsibility |
| --- | --- |
| `Mavalonia.MarkdownEditing` | Everything about editing markdown that needs no UI. Never references Avalonia. |
| `Mavalonia.MarkdownEditing.Controls` | The Avalonia controls |
| `Mavalonia.DesktopHost` | The application in which to try the controls by hand. Each file in `SampleDocuments` is offered in its list. |
| `Mavalonia.MarkdownEditing.Specifications` | Run in parallel, without UI. Most specifications belong here. |
| `Mavalonia.MarkdownEditing.Controls.Specifications` | Use real controls in windows that exist only in memory, one specification at a time. |

## Working with it

- **Run the specifications:** `dotnet test --solution Mavalonia.slnx`
- **Run one project's:** `dotnet test --project Mavalonia.MarkdownEditing.Controls.Specifications`
- **Try the controls, reloading code changes:** `dotnet watch --project Mavalonia.DesktopHost`

## Specifications that use controls

- Put `[XFOnUIThread]` on them instead of `[XF]`: the constructors then run on the UI thread too.
- `HeadlessWindow` clicks, types and renders frames. A rendered frame saved with `SaveForInspectionAs` ends up in `artifacts/rendered-frames`.
- `Avalonia.Headless.XUnit` is not used: it [breaks on xunit.v3 4.x](https://github.com/AvaloniaUI/Avalonia/issues/22072), which Compze.xUnitBDD needs.

Avalonia is a registered trademark of AvaloniaUI OÜ. This project is not affiliated with it.
