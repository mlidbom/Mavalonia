namespace Mavalonia.Markdown.Controls;

static class TaskEX
{
   extension(Task @this)
   {
      ///<summary>Has an exception in the task surface where all of the application's do. For a task that nobody awaits.</summary>
      // ReSharper disable once AsyncVoidMethod
      internal async void SurfaceExceptions() => await @this;
   }
}
