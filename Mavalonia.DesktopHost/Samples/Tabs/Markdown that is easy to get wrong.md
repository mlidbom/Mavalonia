# Markdown that is easy to get wrong

Escaped stars aren't bold: \*\*not bold\*\*. Nor are those in a sum: 2 * 3 * 4. Underscores inside a word aren't markup, snake_case_name, but those around one are: __init__ is bold.

Entities: &amp; &lt;tag&gt; &copy; &#9731; and a non-breaking&nbsp;space.

A line that ends with two spaces  
breaks, and one that ends with a backslash\
too.

HTML is shown as it's written, <kbd>Ctrl</kbd>+<kbd>C</kbd>, except for comments, <!-- like this one --> which aren't shown, and line breaks,<br>such as this one.

<div>
A block of HTML
</div>

Brackets that aren't links: [just brackets] and \[escaped ones\]. A link with no target is one, though: [no target](). And one [with *emphasis* and `code`](https://example.org "and a title").

A bare URL https://example.org/a_b?c=d&e=f#g, one in angle brackets <https://example.org>, and an email address <someone@example.org>.

1. A list that starts at one
1. and repeats the number
1. is numbered anyway

7. and a number after an empty line goes on with the list, as 4.

A paragraph between them, and then:

7. A list that starts at seven

- A list item

  with a second paragraph, and code in it:

  ```
  indented code inside an item
  ```
- [ ] An empty task
- [x] A done task
- [X] A done task, with a capital X

> A quote
> > inside a quote
> > > inside a quote

> [!WARNING]
> An alert
>
> > [!NOTE]
> > with another inside it, which GitHub shows as a quote

| Left | Middle | Right |
| :--- | :---: | ---: |
| a pipe \| in a cell | `code \| too` | **bold** |
| an empty cell |  | 3 |

Footnotes[^1] and $math$ are shown as they're written.

[^1]: The footnote.

A very long word: Pneumonoultramicroscopicsilicovolcanoconiosis_and_a_long_identifier_that_cannot_be_broken_at_any_space_since_it_has_none.
