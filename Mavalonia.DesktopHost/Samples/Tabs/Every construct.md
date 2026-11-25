# Heading 1
## Heading 2
### Heading 3

A paragraph with *emphasis*, **strong emphasis**, ***both***, `code`, a [link](https://commonmark.org "CommonMark")
and an autolink: <https://commonmark.org>. This line ends with a hard break.\
This one follows it.

Setext heading
--------------

> A quote
>
> > with a quote inside it
> >
> > 1. and a list inside that

- A bullet
   - A nested bullet

     with a second paragraph
- Another bullet

1. First
2. Second
   ```csharp
   var fenced = "code inside a list item";
   ```

* * *

    Indented code

```
Fenced code with no language
```

![An image that comes with the application](sample-image.png) and one that is never loaded, since it's on the web: ![An image on the web](https://commonmark.org/help/images/favicon.png)

Escaped \*asterisks\*, an &amp; entity, and <em>inline HTML</em>.

[A reference link][reference]

[reference]: https://spec.commonmark.org
