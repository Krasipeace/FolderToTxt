class Program
{
    static void Main()
    {
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string? exePath = Environment.ProcessPath;
        if (exePath == null) return;

        string exeFileName = Path.GetFileName(exePath);
        if (exeFileName == null) return;

        bool includeHidden = PromptYesNo("Include hidden files and folders? (y/n): ");
        string? fileTypeFilter = PromptFileType();

        const string outputFileName = "folder-contents.txt";
        string outputPath = Path.Combine(baseDirectory, outputFileName);

        int totalItems = CountItems(baseDirectory, includeHidden, fileTypeFilter, exeFileName, outputFileName);

        int fileCount = 0, folderCount = 0, processedItems = 0;
        using (StreamWriter writer = new(outputPath))
        {
            WriteFiles(baseDirectory, writer, "", exeFileName, outputFileName, includeHidden, fileTypeFilter, ref fileCount, ref folderCount, ref processedItems, totalItems);
        }

        Console.WriteLine($"\nDone. Folder content saved to {outputPath}");
        Console.WriteLine($"Total files listed: {fileCount}");
        Console.WriteLine($"Total folders listed: {folderCount}");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }

    static bool PromptYesNo(string message)
    {
        Console.Write(message);
        string? input = Console.ReadLine();
        return input?.Trim().ToLower() == "y";
    }

    static string? PromptFileType()
    {
        Console.Write("Filter by specific file type (e.g., txt, mp3)? Leave empty for all: ");
        string? input = Console.ReadLine();
        return string.IsNullOrWhiteSpace(input) ? null : "." + input.Trim().ToLower();
    }

    static int CountItems(string dir, bool includeHidden, string? fileTypeFilter, string exeFileName, string outputFileName)
    {
        int count = 0;

        foreach (var filePath in Directory.GetFiles(dir))
        {
            string fileName = Path.GetFileName(filePath);
            var attrs = File.GetAttributes(filePath);

            if ((!includeHidden && attrs.HasFlag(FileAttributes.Hidden)) ||
                fileName.Equals(exeFileName, StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals(outputFileName, StringComparison.OrdinalIgnoreCase) ||
                (fileTypeFilter != null && !fileName.ToLower().EndsWith(fileTypeFilter)))
                continue;

            count++;
        }

        foreach (var subDir in Directory.GetDirectories(dir))
        {
            DirectoryInfo dirInfo = new(subDir);
            if (!includeHidden && dirInfo.Attributes.HasFlag(FileAttributes.Hidden))
                continue;

            count++;
            count += CountItems(subDir, includeHidden, fileTypeFilter, exeFileName, outputFileName);
        }

        return count;
    }

    static void WriteFiles(
        string dir,
        StreamWriter writer,
        string relativePath,
        string exeFileName,
        string outputFileName,
        bool includeHidden,
        string? fileTypeFilter,
        ref int fileCount,
        ref int folderCount,
        ref int processedItems,
        int totalItems)
    {
        if (!string.IsNullOrEmpty(relativePath))
        {
            writer.WriteLine($"{relativePath.Replace("\\", "/")}/");
            folderCount++;
            processedItems++;
            UpdateProgressBar(processedItems, totalItems);
        }

        foreach (string filePath in Directory.GetFiles(dir))
        {
            string fileName = Path.GetFileName(filePath);
            var attrs = File.GetAttributes(filePath);

            if ((!includeHidden && attrs.HasFlag(FileAttributes.Hidden)) ||
                fileName.Equals(exeFileName, StringComparison.OrdinalIgnoreCase) ||
                fileName.Equals(outputFileName, StringComparison.OrdinalIgnoreCase) ||
                (fileTypeFilter != null && !fileName.ToLower().EndsWith(fileTypeFilter)))
                continue;

            string fileLine = string.IsNullOrEmpty(relativePath) ? fileName : $"../{fileName}";
            writer.WriteLine(fileLine);
            fileCount++;
            processedItems++;
            UpdateProgressBar(processedItems, totalItems);
        }

        foreach (string subDir in Directory.GetDirectories(dir))
        {
            DirectoryInfo dirInfo = new(subDir);
            if (!includeHidden && dirInfo.Attributes.HasFlag(FileAttributes.Hidden))
                continue;

            string folderName = Path.GetFileName(subDir);
            string newRelativePath = string.IsNullOrEmpty(relativePath)
                ? folderName
                : Path.Combine(relativePath, folderName);

            WriteFiles(subDir, writer, newRelativePath, exeFileName, outputFileName, includeHidden, fileTypeFilter, ref fileCount, ref folderCount, ref processedItems, totalItems);
        }
    }

    static void UpdateProgressBar(int current, int total)
    {
        int percent = (int)((current / (double)total) * 100);
        Console.Write($"\rProgress: {percent}% [{current}/{total}]");
    }
}