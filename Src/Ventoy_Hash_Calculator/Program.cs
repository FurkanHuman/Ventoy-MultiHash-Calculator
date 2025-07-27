namespace Ventoy_Hash_Calculator;

public static class Program
{
    private static readonly string[] HashNames = ["md5", "sha1", "sha256", "sha512"];
    private static readonly string[] SupportedExtensions = ["*.iso", "*.wim", "*.img", "*.vhd", "*.vhdx", "*.efi"];
    private static readonly int BufferSize = 16384; // 16KB buffer for better performance

    static async Task Main(string[] args)
    {
        try
        {
            Console.WriteLine("═══════════════════════════════════════════════");
            Console.WriteLine("        Multi-Format Hash Calculator v0.0.1");
            Console.WriteLine("    Supports: ISO | WIM | IMG | VHD/VHDX | EFI");
            Console.WriteLine("═══════════════════════════════════════════════");

            ProgramConfig? config = ArgumentParser.ParseArguments(args);
            if (config == null) return;

            IList<DriveInfo> driveInfos = DriveManager.GetVentoyDrives();

            if (driveInfos == null || driveInfos.Count == 0)
            {
                Console.WriteLine("Ventoy disk not found.");
                if (!config.Interactive)
                {
                    Environment.Exit(1);
                }
                await Task.Delay(5000);
                Console.WriteLine("Program Exited.");
                Environment.Exit(0);
            }

            DriveInfo? selectedDrive;

            if (config.Interactive)
                selectedDrive = config.DriveLetter != null
                ? DriveManager.SelectDriveByLetter(driveInfos, config.DriveLetter)
                : (DriveManager.SelectDrive(driveInfos));

            else
                selectedDrive = config.DriveLetter != null
                ? DriveManager.SelectDriveByLetter(driveInfos, config.DriveLetter)
                : driveInfos[0];

            if (selectedDrive == null)
            {
                Console.WriteLine("Drive not found or not selected.");
                Environment.Exit(1);
            }

            string ventoyPath = config.DirectoryPath ?? DriveManager.FindVentoyDirectory(selectedDrive);
            if (string.IsNullOrEmpty(ventoyPath))
            {
                Console.WriteLine($"Ventoy directory not found on drive {selectedDrive.Name}");
                Environment.Exit(1);
            }

            if (!Directory.Exists(ventoyPath))
            {
                Console.WriteLine($"Directory does not exist: {ventoyPath}");
                Environment.Exit(1);
            }

            List<FileInfo> allFiles = FileManager.FindSupportedFiles(ventoyPath, config, SupportedExtensions, HashNames);

            if (allFiles.Count == 0)
            {
                Console.WriteLine("No supported files found.");
                Console.WriteLine($"Supported formats: {string.Join(", ", SupportedExtensions.Select(ext => ext.Replace("*", "")).Select(ext => ext.ToUpperInvariant()))}");
                Environment.Exit(1);
            }

            List<FileInfo> selectedFiles = config.Interactive
                ? FileManager.SelectFiles(allFiles, HashNames)
                : FileManager.FilterFilesByConfig(allFiles, config, HashNames);

            if (selectedFiles.Count == 0)
            {
                Console.WriteLine("No files selected or matched criteria.");
                Environment.Exit(1);
            }

            Console.WriteLine($"\nProcessing {selectedFiles.Count} file(s)...\n");

            string[] activeHashNames = config.HashTypes?.Length > 0
                ? [.. HashNames.Where(h => config.HashTypes.Contains(h))]
                : HashNames;

            foreach (FileInfo file in selectedFiles)
            {
                await HashCalculator.ProcessFileAsync(file, activeHashNames, HashNames, BufferSize);
            }

            Console.WriteLine("\nAll files processed successfully!");

            if (config.Interactive)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            if (args.Contains("--interactive") || args.Length == 0)
            {
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
            Environment.Exit(1);
        }
    }
}
