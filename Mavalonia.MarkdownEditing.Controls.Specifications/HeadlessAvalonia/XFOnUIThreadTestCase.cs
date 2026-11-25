using Xunit.Sdk;
using Xunit.v3;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

///<summary>A specification that has <see cref="XFOnUIThreadAttribute"/>.</summary>
class XFOnUIThreadTestCase : XunitTestCase, ISelfExecutingXunitTestCase
{
   [Obsolete("Called by the deserializer")]
   // ReSharper disable once UnusedMember.Global
   public XFOnUIThreadTestCase() {}

   public XFOnUIThreadTestCase(IXunitTestMethod testMethod,
                               string testCaseDisplayName,
                               string uniqueID,
                               bool @explicit,
                               Type[]? skipExceptions,
                               string? skipReason,
                               Type? skipType,
                               string? skipUnless,
                               string? skipWhen,
                               Dictionary<string, HashSet<string>> traits,
                               string? sourceFilePath,
                               int? sourceLineNumber,
                               int? timeout)
      : base(testMethod, testCaseDisplayName, uniqueID, @explicit, skipExceptions, skipReason, skipType, skipUnless, skipWhen, traits, testMethodArguments: null, sourceFilePath, sourceLineNumber, timeout) {}

   public async ValueTask<RunSummary> Run(ExplicitOption explicitOption,
                                          IMessageBus messageBus,
                                          object?[] constructorArguments,
                                          ExceptionAggregator aggregator,
                                          CancellationTokenSource cancellationTokenSource,
                                          ParallelMode parallelMode,
                                          ExecutionScheduler scheduler,
                                          FixtureMappingManager fixtureMappings)
   {
      var tests = await aggregator.RunAsync(CreateTests, []);

      return await UIThread.Run(async () => await XunitTestCaseRunner.Instance.Run(this, tests, messageBus, aggregator, cancellationTokenSource, parallelMode, scheduler, TestCaseDisplayName, SkipReason, explicitOption, constructorArguments, fixtureMappings),
                                cancellationTokenSource.Token);
   }
}
