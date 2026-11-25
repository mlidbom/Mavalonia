## What I changed

The build **passes** now. Three things were wrong, see https://example.org/run/7 or [the log](https://example.org/log):

1. `Answer` threw for an empty question.
2. The *cache* kept what it should have forgotten.
3. ~~The clock~~ was right after all.

```csharp
// Answers a question, as long as it isn't empty.
public static int Answer(string question) =>
   question.Length > 0 ? 42 : throw new ArgumentException("No question", nameof(question));
```

> [!WARNING]
> Clear the cache before you start it.

| File | Lines | What for |
| --- | --- | --- |
| `Answers.cs` | 12 | Checks the question |
| `Cache.cs` | 3 | Forgets in time |

- [x] specifications
- [ ] documentation
  - of the control
  - of the host
