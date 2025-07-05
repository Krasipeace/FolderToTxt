string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
string? exePath = Environment.ProcessPath;

string? exeFileName = Path.GetFileName(exePath);
if (exeFileName is null) return;

Console.Write("Include hidden files and folders? (y/n): ");
string? hiddenInput = Console.ReadLine();
bool includeHidden = hiddenInput?.Trim().ToLower() == "y";

Console.Write("Filter by specific file type (e.g., txt, mp3)? Leave empty for all: ");
string? fileTypeFilterInput = Console.ReadLine();
string? fileTypeFilter = string.IsNullOrWhiteSpace(fileTypeFilterInput)
    ? null
    : "." + fileTypeFilterInput.Trim().ToLower();

string outputFileName = "folder-contents.txt";
string outputPath = Path.Combine(currentDirectory, outputFileName);

int totalItems = CountItems(currentDirectory, includeHidden, fileTypeFilter, exeFileName, outputFileName);

int fileCount = 0;
int folderCount = 0;
int processedItems = 0;

using (StreamWriter writer = new(outputPath))
{
    WriteFiles(currentDirectory, writer, "", exeFileName, outputFileName, includeHidden, fileTypeFilter, ref fileCount, ref folderCount, ref processedItems, totalItems);
}

Console.WriteLine($"\nDone. Folder content saved to {outputPath}");
Console.WriteLine($"Total files listed: {fileCount}");
Console.WriteLine($"Total folders listed: {folderCount}");
Console.WriteLine("Press any key to exit...");
Console.ReadKey();

static int CountItems(string dir, bool includeHidden, string? fileTypeFilter, string exeFileName, string outputFileName)
{
    int count = 0;

    foreach (var filePath in Directory.GetFiles(dir))
    {
        string fileName = Path.GetFileName(filePath);
        FileAttributes attrs = File.GetAttributes(filePath);

        if (!includeHidden && attrs.HasFlag(FileAttributes.Hidden)) continue;

        if (fileName.Equals(exeFileName, StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals(outputFileName, StringComparison.OrdinalIgnoreCase)) continue;

        if (fileTypeFilter != null && !fileName.ToLower().EndsWith(fileTypeFilter)) continue;

        count++;
    }

    foreach (var subDir in Directory.GetDirectories(dir))
    {
        DirectoryInfo dirInfo = new(subDir);
        if (!includeHidden && dirInfo.Attributes.HasFlag(FileAttributes.Hidden)) continue;

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
        FileAttributes attributes = File.GetAttributes(filePath);

        if (!includeHidden && attributes.HasFlag(FileAttributes.Hidden)) continue;

        if (fileName.Equals(exeFileName, StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals(outputFileName, StringComparison.OrdinalIgnoreCase)) continue;

        if (fileTypeFilter != null && !fileName.ToLower().EndsWith(fileTypeFilter)) continue;

        string fileLine = string.IsNullOrEmpty(relativePath) ? fileName : $"../{fileName}";

        writer.WriteLine(fileLine);
        fileCount++;
        processedItems++;
        UpdateProgressBar(processedItems, totalItems);
    }

    foreach (string subDir in Directory.GetDirectories(dir))
    {
        DirectoryInfo dirInfo = new(subDir);
        if (!includeHidden && dirInfo.Attributes.HasFlag(FileAttributes.Hidden)) continue;

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