using Xunit.Internal;
using Xunit.Sdk;
using Xunit.v3;

namespace Mavalonia.MarkdownEditing.Controls.Specifications.HeadlessAvalonia;

///<summary>Finds the specifications that have <see cref="XFOnUIThreadAttribute"/>, only in the classes that declare them.</summary>
class XFOnUIThreadDiscoverer : IXunitTestCaseDiscoverer
{
   public ValueTask<IReadOnlyCollection<IXunitTestCase>> Discover(ITestFrameworkDiscoveryOptions discoveryOptions, IXunitTestMethod testMethod, IFactAttribute factAttribute)
   {
      //todo:compze: Compze.xUnitBDD's ExclusiveFactDiscoverer decides the same two things, but is internal. Once it can be
      //   derived from, derive from it and only create the test case here.
      var declaringType = testMethod.Method.DeclaringType!;
      var isInherited = declaringType != testMethod.TestClass.Class;
      var runsOnlyInTheClassThatDeclaresIt = isInherited && !declaringType.IsAbstract;

      if(runsOnlyInTheClassThatDeclaresIt) return TestCases([]);

      var details = TestIntrospectionHelper.GetTestCaseDetails(discoveryOptions, testMethod, factAttribute);

      return TestCases([isInherited ? ErrorThatTheSpecificationWouldNeverRun() : SpecificationOnUIThread()]);

      static ValueTask<IReadOnlyCollection<IXunitTestCase>> TestCases(IReadOnlyCollection<IXunitTestCase> testCases) => ValueTask.FromResult(testCases);

      XFOnUIThreadTestCase SpecificationOnUIThread() =>
         new(testMethod: details.ResolvedTestMethod,
             testCaseDisplayName: details.TestCaseDisplayName,
             uniqueID: details.UniqueID,
             @explicit: details.Explicit,
             skipExceptions: details.SkipExceptions,
             skipReason: details.SkipReason,
             skipType: details.SkipType,
             skipUnless: details.SkipUnless,
             skipWhen: details.SkipWhen,
             traits: testMethod.Traits.ToReadWrite(StringComparer.OrdinalIgnoreCase),
             sourceFilePath: details.SourceFilePath,
             sourceLineNumber: details.SourceLineNumber,
             timeout: details.Timeout);

      ExecutionErrorTestCase ErrorThatTheSpecificationWouldNeverRun() =>
         new(testMethod,
             details.TestCaseDisplayName,
             details.UniqueID,
             details.SourceFilePath,
             details.SourceLineNumber,
             $"[XFOnUIThread] runs a specification only in the class that declares it, and {declaringType}, which declares {testMethod.Method.Name}, is abstract, so it would never run.");
   }
}
