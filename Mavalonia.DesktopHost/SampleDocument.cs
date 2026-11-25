namespace Mavalonia.DesktopHost;

///<summary>A markdown document that comes with the application, to try the editor on.<br/>
/// Each markdown file in the <c>SampleDocuments</c> folder is one.</summary>
class SampleDocument
{
   readonly FileInfo _file;

   SampleDocument(FileInfo file) => _file = file;

   public string Name => Path.GetFileNameWithoutExtension(_file.Name);

   public string ReadMarkdown() => File.ReadAllText(_file.FullName);

   public static IReadOnlyList<SampleDocument> All() =>
      [..new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "SampleDocuments"))
        .GetFiles("*.md")
        .OrderBy(it => it.Name, StringComparer.Ordinal)
        .Select(it => new SampleDocument(it))];
}
