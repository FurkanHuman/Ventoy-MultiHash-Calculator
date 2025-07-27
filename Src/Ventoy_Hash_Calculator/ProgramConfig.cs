namespace Ventoy_Hash_Calculator;
public class ProgramConfig
{
    public bool Interactive { get; set; } = false;
    public string? DriveLetter { get; set; }
    public string? DirectoryPath { get; set; }
    public string? FileFilter { get; set; }
    public string? FilePattern { get; set; }
    public string[]? HashTypes { get; set; }
    public bool SkipExisting { get; set; } = false;
    public bool Recursive { get; set; } = true;
    public long? MinSizeMB { get; set; }
    public long? MaxSizeMB { get; set; }
    public bool Quiet { get; set; } = false;
    public bool Verbose { get; set; } = false;
}