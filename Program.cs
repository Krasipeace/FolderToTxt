string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;

string? exePath = Environment.ProcessPath;

string? exeFileName = Path.GetFileName(exePath);
if (exeFileName is null) return;

string outputFileName = "folder-contents.txt";
string outputPath = Path.Combine(currentDirectory, outputFileName);

using (StreamWriter writer = new(outputPath))
{
    WriteFiles(currentDirectory, writer, "", exeFileName, outputFileName);
}

Console.WriteLine($"Done. Folder content saved to {outputPath}");

static void WriteFiles(string dir, StreamWriter writer, string relativePath, string exeFileName, string outputFileName)
{
    foreach (string filePath in Directory.GetFiles(dir))
    {
        string fileName = Path.GetFileName(filePath);

        if (fileName.Equals(exeFileName, StringComparison.OrdinalIgnoreCase) ||
            fileName.Equals(outputFileName, StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        writer.WriteLine(Path.Combine(relativePath, fileName).Replace("\\", "/"));
    }

    foreach (string subDir in Directory.GetDirectories(dir))
    {
        string folderName = Path.GetFileName(subDir);
        string newRelativePath = Path.Combine(relativePath, folderName);

        writer.WriteLine($"{newRelativePath.Replace("\\", "/")}/");

        WriteFiles(subDir, writer, newRelativePath, exeFileName, outputFileName);
    }
}
