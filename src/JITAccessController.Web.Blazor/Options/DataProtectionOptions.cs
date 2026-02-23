namespace JITAccessController.Web.Blazor.Options;

public class DataProtectionOptions
{
    public bool PersistKeysToFileSystem { get; set; } = false;
    public string FileSystemPath { get; set; } = "/app/.dpkeys";
    public string CertificatePath { get; set; } = string.Empty;
    public string CertificateKeyPath { get; set; } = string.Empty;
    public string CertificatePasswordPath { get; set; } = string.Empty;
}