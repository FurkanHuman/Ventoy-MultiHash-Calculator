namespace Ventoy_Hash_Calculator;

public static class ArgumentParser
{
    public static ProgramConfig? ParseArguments(string[] args)
    {
        ProgramConfig config = new()
        {
            Interactive = args.Length == 0 || args.Contains("--interactive") || args.Contains("-i")
        };

        if (args.Contains("--help") || args.Contains("-h"))
        {
            ShowHelp();
            return null;
        }

        int i = 0;
        while (i < args.Length)
        {
            string arg = args[i].ToLowerInvariant();
            switch (arg)
            {
                case "--drive":
                case "-d":
                    if (i + 1 < args.Length)
                    {
                        config.DriveLetter = args[i + 1].ToUpperInvariant();
                        i++;
                    }
                    break;

                case "--directory":
                case "--dir":
                    if (i + 1 < args.Length)
                    {
                        config.DirectoryPath = args[i + 1];
                        i++;
                    }
                    break;

                case "--filter":
                case "-f":
                    if (i + 1 < args.Length)
                    {
                        config.FileFilter = args[i + 1].ToLowerInvariant();
                        i++;
                    }
                    break;

                case "--pattern":
                case "-p":
                    if (i + 1 < args.Length)
                    {
                        config.FilePattern = args[i + 1];
                        i++;
                    }
                    break;

                case "--hash":
                case "-H":
                    if (i + 1 < args.Length)
                    {
                        config.HashTypes = args[i + 1].ToLowerInvariant().Split(',', StringSplitOptions.RemoveEmptyEntries);
                        i++;
                    }
                    break;

                case "--skip-existing":
                case "-s":
                    config.SkipExisting = true;
                    break;

                case "--recursive":
                case "-r":
                    config.Recursive = true;
                    break;

                case "--min-size":
                    if (i + 1 < args.Length && long.TryParse(args[i + 1], out long minSize))
                    {
                        config.MinSizeMB = minSize;
                        i++;
                    }
                    break;

                case "--max-size":
                    if (i + 1 < args.Length && long.TryParse(args[i + 1], out long maxSize))
                    {
                        config.MaxSizeMB = maxSize;
                        i++;
                    }
                    break;

                case "--quiet":
                case "-q":
                    config.Quiet = true;
                    break;

                case "--verbose":
                case "-v":
                    config.Verbose = true;
                    break;

                default:
                    if (config.Verbose)
                        Console.WriteLine($"Unknown argument: {args[i]}");
                    break;
            }
            i++; // Main increment
        }

        return config;
    }

    public static void ShowHelp()
    {
        Console.WriteLine("Multi-Format Hash Calculator v3.0");
        Console.WriteLine("Calculates MD5, SHA1, SHA256, SHA512 hashes for Ventoy supported files\n");

        Console.WriteLine("USAGE:");
        Console.WriteLine("  HashCalculator.exe [OPTIONS]\n");

        Console.WriteLine("OPTIONS:");
        Console.WriteLine("  -h, --help              Show this help message");
        Console.WriteLine("  -i, --interactive       Interactive mode (default if no args)");
        Console.WriteLine("  -d, --drive LETTER      Specify drive letter (e.g., E)");
        Console.WriteLine("  --dir PATH              Specify directory path");
        Console.WriteLine("  -f, --filter TYPE       Filter files by type:");
        Console.WriteLine("                            all, iso, wim, img, vhd, efi, win, missing, large");
        Console.WriteLine("  -p, --pattern PATTERN   File name pattern (wildcards supported)");
        Console.WriteLine("  -H, --hash TYPES        Hash types to calculate (md5,sha1,sha256,sha512)");
        Console.WriteLine("  -s, --skip-existing     Skip files that already have hash files");
        Console.WriteLine("  -r, --recursive         Search subdirectories recursively");
        Console.WriteLine("  --min-size SIZE         Minimum file size in MB");
        Console.WriteLine("  --max-size SIZE         Maximum file size in MB");
        Console.WriteLine("  -q, --quiet             Minimal output");
        Console.WriteLine("  -v, --verbose           Detailed output\n");

        Console.WriteLine("EXAMPLES:");
        Console.WriteLine("  HashCalculator.exe");
        Console.WriteLine("    Interactive mode");
        Console.WriteLine();
        Console.WriteLine("  HashCalculator.exe -d E -f iso");
        Console.WriteLine("    Calculate hashes for all ISO files on drive E:");
        Console.WriteLine();
        Console.WriteLine("  HashCalculator.exe -d F -f win -H md5,sha256");
        Console.WriteLine("    Calculate MD5 and SHA256 for Win files on drive F:");
        Console.WriteLine();
        Console.WriteLine("  HashCalculator.exe --dir \"E:\\ISO\" -p \"*Win*.iso\" -s");
        Console.WriteLine("    Process Win ISOs in specific directory, skip existing hashes");
        Console.WriteLine();
        Console.WriteLine("  HashCalculator.exe -d E -f large --min-size 1000 -q");
        Console.WriteLine("    Process files larger than 1GB quietly");
    }
}