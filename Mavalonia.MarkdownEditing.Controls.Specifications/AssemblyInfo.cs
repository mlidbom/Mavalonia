using Xunit.Sdk;
using Xunit.v3;

//There is only one UI thread, and code that uses Avalonia on other threads at the same time makes the headless platform fail and hang: https://github.com/AvaloniaUI/Avalonia/issues/22021
[assembly: Parallelization(Mode = ParallelMode.None)]
