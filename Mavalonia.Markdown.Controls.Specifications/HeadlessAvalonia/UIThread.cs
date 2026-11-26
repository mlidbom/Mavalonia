using Avalonia.Headless;

namespace Mavalonia.Markdown.Controls.Specifications.HeadlessAvalonia;

///<summary>The thread that Avalonia's controls must be created and used on.</summary>
static class UIThread
{
   static readonly HeadlessUnitTestSession Session = HeadlessUnitTestSession.StartNew(typeof(HeadlessApplication));

   ///<summary>Runs <paramref name="action"/> on the UI thread, in a <see cref="HeadlessApplication"/> of its own.</summary>
   public static Task<TResult> Run<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken) => Session.Dispatch(action, cancellationToken);
}
