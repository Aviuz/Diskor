using ECF;

namespace Diskor;

[Command("analyze")]
[CmdDescription("Analyzes size for specified directory")]
class AnalyzeDirectoryCommand : CommandBase
{
    private const long DefaultQuota = 512 * 1024 * 1024;
    private const int DefaultMaxDepth = 5;

    [Argument(0, Name = "path", Description = "path to analyze")]
    public string? TargetPath { get; set; }

    [Parameter("-q --quota", Description = "Min. size for directory to be displayed. By default it is 512 MB.")]
    public int? QuotaMb { get; set; }

    [Parameter("-d --depth", Description = "Max depth of displayed tree of folders that exceeds quota")]
    public int? MaxDepth { get; set; }

    public override void Execute()
    {
        long quota = QuotaMb.HasValue ? QuotaMb.Value * 1024 * 1024 : DefaultQuota;
        int maxDepth = MaxDepth ?? DefaultMaxDepth;

        if (string.IsNullOrEmpty(TargetPath))
        {
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                Console.WriteLine($"Scanning drive: {drive.Name}");
                ScanDirectory(drive.Name, quota, maxDepth, 0);
            }
        }
        else
        {
            ScanDirectory(TargetPath, quota, maxDepth, 0);
        }
    }

    static void ScanDirectory(string path, long quota, int maxDepth, int currentDepth)
    {
        try
        {
            if (currentDepth > maxDepth) return;
            long folderSize = GetDirectorySize(path);

            if (folderSize > quota)
            {
                Console.WriteLine($"{new string(' ', currentDepth * 2)}{Path.GetFileName(path)} - {FormatSize(folderSize)}");
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                ScanDirectory(dir, quota, maxDepth, currentDepth + 1);
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }
    }

    static long GetDirectorySize(string path)
    {
        try
        {
            return Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                .Sum(file => new FileInfo(file).Length);
        }
        catch (UnauthorizedAccessException) { return 0; }
        catch (IOException) { return 0; }
    }

    static string FormatSize(long size)
    {
        if (size >= 1_073_741_824) // GB
            return $"{size / 1_073_741_824.0:F2} GB";
        if (size >= 1_048_576) // MB
            return $"{size / 1_048_576.0:F2} MB";
        if (size >= 1024) // KB
            return $"{size / 1024.0:F2} KB";
        return $"{size} B";
    }
}