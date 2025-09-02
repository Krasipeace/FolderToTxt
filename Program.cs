using FolderScanToTxt;

class Program
{
    static void Main()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string exeFileName = Path.GetFileName(Environment.ProcessPath!) ?? "FolderToTxt.exe";

        bool includeHidden = ConsoleHelpers.PromptHiddenFiles();
        string? fileTypeFilter = ConsoleHelpers.PromptFileType();

        FolderScanOptions options = new()
        {
            IncludeHidden = includeHidden,
            FileTypeFilter = fileTypeFilter,
            ExcludeFileName1 = exeFileName,
            ExcludeFileName2 = "folder-contents.txt"
        };

        var result = FolderScannerService.ScanAndSave(baseDirectory, "folder-contents.txt", options);

        Console.WriteLine($"\nDone. Folder content saved to {result.OutputPath}");
        Console.WriteLine($"Total files listed: {result.FileCount}");
        Console.WriteLine($"Total folders listed: {result.FolderCount}");
    }
}