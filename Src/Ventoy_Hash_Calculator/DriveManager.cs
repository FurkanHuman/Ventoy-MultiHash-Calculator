namespace Ventoy_Hash_Calculator;
public static class DriveManager
{
    public static IList<DriveInfo> GetVentoyDrives()
    {
        return [.. DriveInfo.GetDrives().Where(VentoyDiskCheck)];
    }

    public static DriveInfo? SelectDriveByLetter(IList<DriveInfo> driveInfos, string driveLetter)
    {
        string targetDrive = $"{driveLetter.TrimEnd(':')}:\\";
        return driveInfos.FirstOrDefault(d => d.Name.Equals(targetDrive, StringComparison.OrdinalIgnoreCase));
    }

    public static DriveInfo? SelectDrive(IList<DriveInfo> driveInfos)
    {
        if (driveInfos.Count == 1)
        {
            Console.WriteLine($"Found 1 Ventoy disk: {driveInfos[0].Name} ({driveInfos[0].TotalSize / (1024 * 1024 * 1024)} GB)");
            return driveInfos[0];
        }

        Console.WriteLine($"Found {driveInfos.Count} Ventoy disks:");
        for (int i = 0; i < driveInfos.Count; i++)
        {
            DriveInfo drive = driveInfos[i];
            long totalGB = drive.TotalSize / (1024 * 1024 * 1024);
            long freeGB = drive.AvailableFreeSpace / (1024 * 1024 * 1024);
            Console.WriteLine($"  {i + 1}. Drive: {drive.Name} | Total: {totalGB} GB | Free: {freeGB} GB | Label: {drive.VolumeLabel}");
        }

        while (true)
        {
            Console.Write($"\nSelect drive (1-{driveInfos.Count}) or 0 to exit: ");
            string input = Console.ReadLine()?.Trim() ?? "";

            if (int.TryParse(input, out int choice))
            {
                if (choice == 0)
                {
                    return null;
                }
                if (choice >= 1 && choice <= driveInfos.Count)
                {
                    return driveInfos[choice - 1];
                }
            }

            Console.WriteLine("Invalid selection. Please try again.");
        }
    }

    public static string FindVentoyDirectory(DriveInfo drive)
    {

        string[] SupportedExtensions = ["*.iso", "*.wim", "*.img", "*.vhd", "*.vhdx", "*.efi"];

        string[] possiblePaths =
            [
                Path.Combine(drive.Name, "ISO"),           // Standard ISO folder
                drive.Name.TrimEnd('\\'),                  // Direct disk root
                Path.Combine(drive.Name, "Images"),        // Alternative folder
                Path.Combine(drive.Name, "Boot"),          // Boot files
                Path.Combine(drive.Name, "EFI")            // EFI files
            ];

        for (int i = 0; i < possiblePaths.Length; i++)
        {
            string path = possiblePaths[i];
            if (Directory.Exists(path))
            {
                bool hasFiles = SupportedExtensions.Any(pattern => Directory.GetFiles(path, pattern, SearchOption.AllDirectories).Length > 0);

                if (hasFiles)
                    return path;
            }
        }

        if (Directory.Exists(drive.Name))
        {
            bool hasFiles = SupportedExtensions.Any(pattern =>
                Directory.GetFiles(drive.Name, pattern, SearchOption.AllDirectories).Length > 0);

            if (hasFiles)
                return drive.Name.TrimEnd('\\');
        }

        return string.Empty;
    }

    private static bool VentoyDiskCheck(DriveInfo drive)
    {
        try
        {
            string[] SupportedExtensions = ["*.iso", "*.wim", "*.img", "*.vhd", "*.vhdx", "*.efi"];

            bool isRemovable = drive.DriveType == DriveType.Removable;
            bool isReady = drive.IsReady;
            bool hasValidFormat = drive.DriveFormat.Equals("exFAT", StringComparison.OrdinalIgnoreCase) ||
                                 drive.DriveFormat.Equals("NTFS", StringComparison.OrdinalIgnoreCase) ||
                                 drive.DriveFormat.Equals("FAT32", StringComparison.OrdinalIgnoreCase);

            bool hasVentoyLabel = drive.VolumeLabel.Contains("Ventoy", StringComparison.OrdinalIgnoreCase) ||
                                 drive.VolumeLabel.Contains("VTOYEFI", StringComparison.OrdinalIgnoreCase) ||
                                 string.IsNullOrEmpty(drive.VolumeLabel);

            bool hasVentoyStructure = false;

            if (isReady)
            {
                string[] ventoyIndicators = [
                    Path.Combine(drive.Name, "ventoy"),
                    Path.Combine(drive.Name, "ISO"),
                    Path.Combine(drive.Name, "EFI"),
                    Path.Combine(drive.Name, "grub"),
                    Path.Combine(drive.Name, "boot")
                ];

                hasVentoyStructure = ventoyIndicators.Any(path => Directory.Exists(path) || File.Exists(path));

                if (!hasVentoyStructure)
                    hasVentoyStructure = SupportedExtensions.Any(pattern => Directory.GetFiles(drive.Name, pattern, SearchOption.AllDirectories).Length > 0);

            }

            return isRemovable && isReady && hasValidFormat && (hasVentoyLabel || hasVentoyStructure);
        }
        catch
        {
            return false;
        }
    }
}
