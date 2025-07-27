namespace Ventoy_Hash_Calculator;

public static class FileManager
{
    public static List<FileInfo> FindSupportedFiles(string ventoyPath, ProgramConfig config, string[] supportedExtensions, string[] hashNames)
    {
        List<FileInfo> allFiles = [];
        SearchOption searchOption = config.Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        foreach (string pattern in supportedExtensions)
        {
            try
            {
                IEnumerable<FileInfo> files = Directory.GetFiles(ventoyPath, pattern, searchOption)
                    .Select(f => new FileInfo(f))
                    .Where(f => !hashNames.Any(h => f.FullName.EndsWith($".{h}", StringComparison.OrdinalIgnoreCase)));

                allFiles.AddRange(files);
            }
            catch (Exception ex)
            {
                if (config.Verbose)
                    Console.WriteLine($"Warning: Could not search for {pattern}: {ex.Message}");
            }
        }

        return [.. allFiles.OrderBy(f => f.Name)];
    }

    public static List<FileInfo> FilterFilesByConfig(List<FileInfo> allFiles, ProgramConfig config, string[] hashNames)
    {
        IEnumerable<FileInfo> filteredFiles = allFiles.AsEnumerable();

        // File pattern fillter
        if (!string.IsNullOrEmpty(config.FilePattern))
        {
            string pattern = config.FilePattern.Replace("*", ".*").Replace("?", ".");
            System.Text.RegularExpressions.Regex regex = new(pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            filteredFiles = filteredFiles.Where(f => regex.IsMatch(f.Name));
        }

        // file type filter
        if (!string.IsNullOrEmpty(config.FileFilter))
            filteredFiles = config.FileFilter switch
            {
                "all" => filteredFiles,
                "iso" => filteredFiles.Where(f => f.Extension.Equals(".iso", StringComparison.OrdinalIgnoreCase)),
                "wim" => filteredFiles.Where(f => f.Extension.Equals(".wim", StringComparison.OrdinalIgnoreCase)),
                "img" => filteredFiles.Where(f => f.Extension.Equals(".img", StringComparison.OrdinalIgnoreCase)),
                "vhd" => filteredFiles.Where(f => f.Extension.Equals(".vhd", StringComparison.OrdinalIgnoreCase) ||
                                                  f.Extension.Equals(".vhdx", StringComparison.OrdinalIgnoreCase)),
                "efi" => filteredFiles.Where(f => f.Extension.Equals(".efi", StringComparison.OrdinalIgnoreCase)),
                "win" => filteredFiles.Where(f => f.Name.Contains("Win", StringComparison.OrdinalIgnoreCase)),
                "missing" => filteredFiles.Where(f => CheckExistingHashes(f, hashNames).Count < hashNames.Length),
                "large" => filteredFiles.Where(f => f.Length > 1024L * 1024 * 1024),
                _ => filteredFiles
            };

        // Size filter
        if (config.MinSizeMB.HasValue)
            filteredFiles = filteredFiles.Where(f => f.Length >= config.MinSizeMB.Value * 1024 * 1024);


        if (config.MaxSizeMB.HasValue)
            filteredFiles = filteredFiles.Where(f => f.Length <= config.MaxSizeMB.Value * 1024 * 1024);

        if (config.SkipExisting)
            filteredFiles = filteredFiles.Where(f => CheckExistingHashes(f, hashNames).Count < hashNames.Length);


        return [.. filteredFiles];
    }

    public static List<FileInfo> SelectFiles(List<FileInfo> allFiles, string[] hashNames)
    {
        Console.WriteLine($"\nFound {allFiles.Count} supported file(s):");

        IEnumerable<IGrouping<string, FileInfo>> fileGroups = allFiles.GroupBy(f => Path.GetExtension(f.Name).ToLowerInvariant()).OrderBy(g => g.Key);

        Console.WriteLine("\nFile types breakdown:");
        foreach (IGrouping<string, FileInfo> group in fileGroups)
        {
            long totalSizeMB = group.Sum(f => f.Length) / (1024 * 1024);
            Console.WriteLine($"  • {group.Key.ToUpperInvariant()}: {group.Count()} files ({totalSizeMB} MB total)");
        }

        Console.WriteLine("\nFile list:");
        for (int i = 0; i < allFiles.Count; i++)
        {
            FileInfo file = allFiles[i];
            long fileSizeMB = file.Length / (1024 * 1024);
            string relativePath = GetRelativePath(file.FullName);

            // Mevcut hash dosyalarını kontrol et
            List<string> existingHashes = CheckExistingHashes(file, hashNames);
            string hashStatus = existingHashes.Count > 0 ?
                $"[Has: {string.Join(", ", existingHashes)}]" :
                "[No hashes]";

            Console.WriteLine($"  {i + 1,2}. {file.Name} ({fileSizeMB} MB) {hashStatus}");
            if (relativePath != file.Name)
            {
                Console.WriteLine($"       {relativePath}\n");
            }
        }

        Console.WriteLine("\nSelection options:");
        Console.WriteLine("  • Enter numbers separated by commas (e.g., 1,3,5)");
        Console.WriteLine("  • Enter 'all' to select all files");
        Console.WriteLine("  • Enter 'iso' to select only ISO files");
        Console.WriteLine("  • Enter 'wim' to select only WIM files");
        Console.WriteLine("  • Enter 'img' to select only IMG files");
        Console.WriteLine("  • Enter 'vhd' to select VHD/VHDX files");
        Console.WriteLine("  • Enter 'efi' to select only EFI files");
        Console.WriteLine("  • Enter 'win' to select files containing 'Win'");
        Console.WriteLine("  • Enter 'missing' to select files with missing hashes");
        Console.WriteLine("  • Enter 'large' to select files larger than 1GB");
        Console.WriteLine("  • Enter '0' to exit");

        while (true)
        {
            Console.Write("\nYour selection: ");
            string input = Console.ReadLine()?.Trim().ToLowerInvariant() ?? "";

            switch (input)
            {
                case "0":
                    return [];
                case "all":
                    return allFiles;
                case "iso":
                    return [.. allFiles.Where(f => f.Extension.Equals(".iso", StringComparison.OrdinalIgnoreCase))];
                case "wim":
                    return [.. allFiles.Where(f => f.Extension.Equals(".wim", StringComparison.OrdinalIgnoreCase))];
                case "img":
                    return [.. allFiles.Where(f => f.Extension.Equals(".img", StringComparison.OrdinalIgnoreCase))];
                case "vhd":
                    return [.. allFiles.Where(f => f.Extension.Equals(".vhd", StringComparison.OrdinalIgnoreCase) || f.Extension.Equals(".vhdx", StringComparison.OrdinalIgnoreCase))];
                case "efi":
                    return [.. allFiles.Where(f => f.Extension.Equals(".efi", StringComparison.OrdinalIgnoreCase))];
                case "win":
                    return [.. allFiles.Where(f => f.Name.Contains("Win", StringComparison.OrdinalIgnoreCase))];
                case "missing":
                    return [.. allFiles.Where(f => CheckExistingHashes(f, hashNames).Count < hashNames.Length)];
                case "large":
                    return [.. allFiles.Where(f => f.Length > 1024L * 1024 * 1024)]; // > 1GB
            }

            if (input.Contains(',') || int.TryParse(input, out _))
            {
                List<FileInfo> selectedFiles = [];
                string[] numbers = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

                bool validSelection = true;
                foreach (string numStr in numbers)
                {
                    if (int.TryParse(numStr.Trim(), out int fileIndex) && fileIndex >= 1 && fileIndex <= allFiles.Count)
                        selectedFiles.Add(allFiles[fileIndex - 1]);
                    else
                    {
                        validSelection = false;
                        break;
                    }
                }

                if (validSelection && selectedFiles.Count > 0)
                    return [.. selectedFiles.Distinct()];
            }

            Console.WriteLine("Invalid selection. Please try again.");
        }
    }

    public static List<string> CheckExistingHashes(FileInfo file, string[] hashNames)
    {
        List<string> existingHashes = [];

        foreach (string hashName in hashNames)
        {
            string hashFilePath = $"{file.FullName}.{hashName}";
            if (File.Exists(hashFilePath))
                existingHashes.Add(hashName.ToUpperInvariant());
        }

        return existingHashes;
    }

    public static string GetRelativePath(string fullPath)
    {
        return fullPath.Contains("\\ISO\\") ? fullPath[(fullPath.IndexOf("\\ISO\\") + 1)..] : Path.GetFileName(fullPath);
    }
}