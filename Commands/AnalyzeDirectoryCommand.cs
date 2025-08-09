using ECF;
using System.IO;

namespace Diskor;

[Command("analyze")]
[CmdDescription("Analyzes size for specified directory")]
class AnalyzeDirectoryCommand : CommandBase
{
    private const long DefaultQuota = 10L * 1024 * 1024 * 1024;
    private const int DefaultMaxDepth = 5;

    [Argument(0, Name = "path", Description = "path to analyze")]
    public string? TargetPath { get; set; }

    [Parameter("-q --quota", Description = "Min. size for directory to be displayed. By default it is 10 GB. If no unit is provided it will use MB as a default.")]
    public string? QuotaStr { get; set; }

    [Parameter("-d --depth", Description = "Max depth of displayed tree of folders that exceeds quota")]
    public int? MaxDepth { get; set; }

    public override void Execute()
    {
        long quota = !string.IsNullOrWhiteSpace(QuotaStr) ? ParseSize(QuotaStr) : DefaultQuota;
        int maxDepth = MaxDepth ?? DefaultMaxDepth;

        if (string.IsNullOrEmpty(TargetPath))
        {
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                Console.WriteLine($"Scanning drive: {drive.Name} - {FormatSize(GetDiskSize(drive.Name))}");
                foreach (var dir in Directory.GetDirectories(drive.Name))
                {
                    ScanDirectory(dir, quota, maxDepth, 0, false);
                }
            }
        }
        else
        {
            ScanDirectory(TargetPath, quota, maxDepth, 0, false);
        }
    }

    static void ScanDirectory(string path, long quota, int maxDepth, int currentDepth, bool displayFullPath)
    {
        try
        {
            if (currentDepth > maxDepth) return;
            if (!Directory.Exists(path)) return;

            bool isDiskRoot = Path.GetPathRoot(path!)!.Equals(path, StringComparison.OrdinalIgnoreCase);

            bool displayFullPathForChildren = false;

            long folderSize = isDiskRoot
                ? GetDiskSize(path)
                : GetDirectorySize(path);

            if (folderSize > quota)
            {
                string currentPathStr = displayFullPath || isDiskRoot ? path : Path.GetFileName(path);
                string prefix = currentDepth > 0 ? new string('-', currentDepth) + " " : "";
                Console.WriteLine($"{prefix}{currentPathStr} - {FormatSize(folderSize)}");
            }
            else
            {
                displayFullPathForChildren = true;
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                ScanDirectory(dir, quota, maxDepth, currentDepth + 1, displayFullPathForChildren);
            }
        }
        catch (UnauthorizedAccessException) { }
        catch (IOException) { }
    }

    static long GetDiskSize(string path)
    {
        var driveInfo = new DriveInfo(path);
        return driveInfo.TotalSize - driveInfo.TotalFreeSpace;
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

    static long ParseSize(string sizeString)
    {
        if (string.IsNullOrWhiteSpace(sizeString))
            return 0;

        if (long.TryParse(sizeString, out long integer))
            return integer * 1024 * 1024; // by default specify in MB

        long totalBytes = 0;
        var regex = new System.Text.RegularExpressions.Regex(@"(\d+(?:\.\d+)?)\s*(GB|MB|KB|B)?", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        var matches = regex.Matches(sizeString);

        foreach (System.Text.RegularExpressions.Match match in matches)
        {
            if (!match.Success) continue;

            double value = double.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
            string unit = match.Groups[2].Value.ToUpperInvariant();

            switch (unit)
            {
                case "GB":
                    totalBytes += (long)(value * 1024 * 1024 * 1024);
                    break;
                case "MB":
                    totalBytes += (long)(value * 1024 * 1024);
                    break;
                case "KB":
                    totalBytes += (long)(value * 1024);
                    break;
                case "B":
                case "":
                    totalBytes += (long)value;
                    break;
            }
        }

        return totalBytes;
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