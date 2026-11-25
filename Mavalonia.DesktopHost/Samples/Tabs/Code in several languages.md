# Code in several languages

C#:

```csharp
public sealed record Order(Guid Id, IReadOnlyList<Line> Lines)
{
   public decimal Total => Lines.Sum(it => it.Price * it.Count);
}
```

Python:

```python
def total(lines: list[dict]) -> float:
    return sum(line["price"] * line["count"] for line in lines)
```

TypeScript:

```typescript
export const total = (lines: Line[]): number =>
  lines.reduce((sum, line) => sum + line.price * line.count, 0);
```

JSON:

```json
{ "id": "7f3e", "lines": [{ "price": 9.5, "count": 2 }], "paid": false }
```

PowerShell:

```powershell
Get-ChildItem -Recurse -Filter *.cs | Measure-Object -Property Length -Sum
```

Bash:

```bash
for file in src/*.cs; do grep -c "class " "$file"; done
```

SQL:

```sql
SELECT o.Id, SUM(l.Price * l.Count) AS Total
FROM Orders o JOIN Lines l ON l.OrderId = o.Id
GROUP BY o.Id;
```

XML:

```xml
<PackageReference Include="Avalonia" Version="12.1.5" />
```

A diff:

```diff
- var total = lines.Count;
+ var total = lines.Sum(it => it.Price * it.Count);
```

And code with no language, which isn't coloured:

```
just text, in a fixed width
```
