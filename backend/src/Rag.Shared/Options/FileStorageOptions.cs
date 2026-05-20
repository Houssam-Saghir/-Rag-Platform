namespace Rag.Shared.Options;

public class FileStorageOptions
{
    public const string SectionName = "FileStorage";
    public string UploadPath { get; set; } = "wwwroot/uploads";
    public int MaxFileSizeMB { get; set; } = 50;
    public List<string> AllowedExtensions { get; set; } = new();
}
