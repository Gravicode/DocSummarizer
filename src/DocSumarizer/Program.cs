using Microsoft.SemanticKernel.Text;
using Microsoft.SemanticMemory.DataFormats.Office;
using Microsoft.SemanticMemory.DataFormats.Pdf;
using Path = System.IO.Path;

namespace DocSumarizer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AppConstants.LoadFromSettings();

            Console.WriteLine("Summarize Document v0.1");
            Console.WriteLine("-----------------------");
            if (args.Length == 0)
            {
                Console.WriteLine("Type: DocSumarizer [Path/Folder]");
                return;
            }
            var folder = "content";// args[0];
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            var DestFolder = Path.Combine(currentDirectory, folder);
            if (!Directory.Exists(DestFolder))
            {
                if (Directory.Exists(folder))
                {
                    DestFolder = folder;
                }
                else
                {
                    Console.WriteLine("Please type correct path");
                    return;
                }
            }
            var dinfo = new DirectoryInfo(DestFolder);

            string[] extensions = new[] { ".doc", ".docx", ".pdf", ".txt" };
            FileInfo[] files =
                dinfo.EnumerateFiles()
                     .Where(f => extensions.Contains(f.Extension.ToLower()))
                     .ToArray();
            Parallel.ForEach(files,
    file =>
    {
        try
        {
            SummaryService svc = new SummaryService();
            var content = string.Empty;
            switch (file.Extension)
            {
                case ".doc":
                case ".docx":
                    content = new MsWordDecoder().DocToText(file.FullName);
                    break;
                case ".pdf":
                    content = new PdfDecoder().DocToText(file.FullName);
                    break;
                case ".txt":
                    content = File.ReadAllText(file.FullName);
                    break;
            }
            var newName = $"summary_{file.Name}.txt";
            Console.WriteLine($"starting to summarize file: {file.Name}");
            List<string> paragraphs =
    TextChunker.SplitPlainTextParagraphs(
        TextChunker.SplitPlainTextLines(
            content,
            128),
        1024);
            Dictionary<int, string> summary = new Dictionary<int, string>();
            var result = Parallel.For(0, paragraphs.Count,
                (index) =>
                {
                    var task1 = Task.Run(async () =>
                    {
                        var sumtext = await svc.Summarize(paragraphs[index]);
                        summary.Add(index, sumtext);
                    });
                    task1.Wait();

                });
            while (!result.IsCompleted)
            {
                Thread.Sleep(100);
            }
            var textContent = string.Join("\n\n", summary.OrderBy(x => x.Key).Select(x => x.Value).ToArray());
            File.WriteAllText(DestFolder + "/" + newName, textContent);
            Console.WriteLine($"generate new file summary: {newName}");
        }
        catch (Exception ex)
        {

            Console.WriteLine($"summarize {file.Name} is failed: {ex}");
        }
    });

            if (files.Length > 0)
            {
                Console.WriteLine($"Done, total summarized files: {files.Length}");
            }
        }
    }
}