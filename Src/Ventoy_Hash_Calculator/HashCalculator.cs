using System.Security.Cryptography;
using System.Text;

namespace Ventoy_Hash_Calculator;
public static class HashCalculator
{
    public static async Task ProcessFileAsync(FileInfo file, string[] activeHashNames, string[] allHashNames, int bufferSize)
    {
        FileNameWriter(file.Name);

        FileManager.CheckExistingHashes(file, allHashNames);
        List<int> hashesToCalculate = [];

        for (int i = 0; i < activeHashNames.Length; i++)
        {
            string hashName = activeHashNames[i];
            string hashFilePath = $"{file.FullName}.{hashName}";

            if (File.Exists(hashFilePath))
            {
                Console.WriteLine($"{hashName.ToUpperInvariant()} hash already exists - SKIPPING");
                continue;
            }
            
            int globalIndex = Array.IndexOf(allHashNames, hashName);
            if (globalIndex >= 0)
                hashesToCalculate.Add(globalIndex);

        }

        if (hashesToCalculate.Count == 0)
        {
            Console.WriteLine("All requested hash files already exist for this file!");
            return;
        }

        Console.WriteLine($"Will calculate: {string.Join(", ", hashesToCalculate.Select(i => allHashNames[i].ToUpperInvariant()))}");

        string[] hashes = await FileHashsCalculateAsync(file, hashesToCalculate, allHashNames, bufferSize);

        for (int i = 0; i < hashesToCalculate.Count; i++)
        {
            int hashIndex = hashesToCalculate[i];
            string newFilePath = $"{file.FullName}.{allHashNames[hashIndex]}";

            Console.WriteLine($"\nCreating: {file.Name}.{allHashNames[hashIndex]}");

            try
            {
                await using StreamWriter writer = new(newFilePath, append: false, Encoding.UTF8);
                string line = $"{hashes[i].ToLowerInvariant()} *{file.Name}";
                await writer.WriteLineAsync(line);
                Console.WriteLine($"Hash file created successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating hash file: {ex.Message}");
            }
        }
    }

    private static async Task<string[]> FileHashsCalculateAsync(FileInfo fileInfo, List<int> hashesToCalculate, string[] allHashNames, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(allHashNames);
        await using FileStream stream = fileInfo.OpenRead();

        List<(HashAlgorithm Algorithm, int Index)> hashAlgorithms = [];

        foreach (int index in hashesToCalculate)
        {
            HashAlgorithm algorithm = index switch
            {
                0 => MD5.Create(),
                1 => SHA1.Create(),
                2 => SHA256.Create(),
                3 => SHA512.Create(),
                _ => throw new ArgumentException("Invalid hash index")
            };
            hashAlgorithms.Add((algorithm, index));
        }

        try
        {
            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            long totalBytesRead = 0;
            long fileSize = fileInfo.Length;

            Console.WriteLine($"📏 File size: {fileSize / (1024 * 1024)} MB");
            Console.WriteLine("🔄 Calculating hashes...");

            long startTime = Environment.TickCount64;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                foreach ((HashAlgorithm algorithm, int _) in hashAlgorithms)
                    algorithm.TransformBlock(buffer, 0, bytesRead, null, 0);

                totalBytesRead = UpdatePercentage(bytesRead, fileSize, totalBytesRead, startTime);
            }

            Console.WriteLine();

            foreach ((HashAlgorithm algorithm, int _) in hashAlgorithms)
                algorithm.TransformFinalBlock([], 0, 0);
            

            string[] results = new string[hashesToCalculate.Count];
            for (int i = 0; i < hashAlgorithms.Count; i++)
                results[i] = Convert.ToHexString(hashAlgorithms[i].Algorithm.Hash!);

            long elapsed = Environment.TickCount64 - startTime;
            Console.WriteLine($"Completed in {elapsed / 60000.0:F1} minutes");

            return results;
        }
        finally
        {
            foreach ((HashAlgorithm algorithm, int _) in hashAlgorithms)
                algorithm.Dispose();
        }
    }

    private static long UpdatePercentage(int bytesRead, long fileSize, long totalBytesRead, long startTime)
    {
        totalBytesRead += bytesRead;
        double progressPercentage = (double)totalBytesRead * 100.0 / fileSize;

        long elapsedMs = Environment.TickCount64 - startTime;
        if (elapsedMs > 0 && totalBytesRead > 0)
        {
            long estimatedTotalMs = elapsedMs * fileSize / totalBytesRead;
            long remainingMs = Math.Max(0, estimatedTotalMs - elapsedMs);
            TimeSpan eta = TimeSpan.FromMilliseconds(remainingMs);

            DrawProgressBar(progressPercentage, eta);
        }
        else
            DrawProgressBar(progressPercentage, TimeSpan.Zero);

        return totalBytesRead;
    }

    private static void DrawProgressBar(double percentage, TimeSpan eta, int totalWidth = 40)
    {
        Console.CursorVisible = false;
        int charsToShow = (int)Math.Floor(percentage / 100 * totalWidth);
        string bar = "[" + new string('█', charsToShow) + new string('░', totalWidth - charsToShow) + "]";

        string percentageText = $"{percentage:F2}%";
        string etaText = eta.TotalMinutes > 1 ? $"ETA: {eta:mm\\:ss}" : "ETA: <1min";

        Console.Write($"\r{bar} {percentageText} | {etaText}");
    }

    private static void FileNameWriter(string fileName)
    {
        Console.WriteLine();
        Console.WriteLine("═══════════════════════════════════════════════");
        Console.WriteLine($"Processing: {fileName}");
        Console.WriteLine("═══════════════════════════════════════════════");
    }
}
