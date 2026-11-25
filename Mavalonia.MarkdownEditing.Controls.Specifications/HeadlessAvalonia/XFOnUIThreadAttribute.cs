using System.Runtime.CompilerServices;
using Compze.xUnitBDD;
using Xunit;
using Xunit.v3;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

///<summary>An <see cref="XFAttribute">[XF]</see> that runs on the UI thread, constructors included.<br/>
/// For specifications that create or use Avalonia controls.</summary>
[AttributeUsage(AttributeTargets.Method)]
[XunitTestCaseDiscoverer(typeof(XFOnUIThreadDiscoverer))]
sealed class XFOnUIThreadAttribute([CallerFilePath] string? sourceFilePath = null, [CallerLineNumber] int sourceLineNumber = -1)
   : FactAttribute(sourceFilePath, sourceLineNumber);
