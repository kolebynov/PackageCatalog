using System.Text;
using Microsoft.Extensions.Logging;
using PackageCatalog.Shared.Interfaces;

namespace PackageCatalog.Shared.Internal;

internal class FileSystemAdapter : IFileSystemAdapter
{
	private readonly ILogger<FileSystemAdapter> logger;

	public FileSystemAdapter(ILogger<FileSystemAdapter> logger)
	{
		this.logger = logger;
	}

	public string ReadAllText(string path, Encoding encoding) => File.ReadAllText(path, encoding);

	public Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken) =>
		File.ReadAllTextAsync(path, encoding, cancellationToken);

	public void WriteAllText(string path, string content) => File.WriteAllText(path, content);

	public Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken) =>
		File.WriteAllTextAsync(path, content, cancellationToken);

	public bool DirectoryExists(string path) => Directory.Exists(path);

	public void CreateDirectory(string path) => Directory.CreateDirectory(path);

	public void EnsureDirectoryExists(string path)
	{
		if (!DirectoryExists(path))
		{
			logger.LogDebug("{Path} does not exist. Creating...", path);
			CreateDirectory(path);
		}
	}

	public IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption searchOption) =>
		Directory.EnumerateFiles(path, pattern, searchOption);

	public void SafeDeleteDirectory(string path)
	{
		try
		{
			Directory.Delete(path, true);
		}
#pragma warning disable CA1031
		catch
		{
			logger.LogWarning("Can't delete directory {Path}", path);
		}
#pragma warning restore CA1031
	}

	public void Copy(string sourceFileName, string destFileName, bool overwrite) =>
		File.Copy(sourceFileName, destFileName, overwrite);

	public void SafeDeleteFile(string path)
	{
		try
		{
			File.Delete(path);
		}
#pragma warning disable CA1031
		catch
		{
			logger.LogWarning("Can't delete file {Path}", path);
		}
#pragma warning restore CA1031
	}

	public bool FileExists(string file) => File.Exists(file);

	public void Move(string @from, string to) => File.Move(@from, to);

	public string GetTempPath() => Path.GetTempPath();

	public long GetFileLength(string file) => new FileInfo(file).Length;

	public DateTimeOffset GetCreationTime(string file) => File.GetCreationTime(file);

	public DateTimeOffset GetLastWriteTime(string file) => File.GetLastWriteTime(file);

	public string EscapeInvalidChars(string file) =>
		Path.GetInvalidFileNameChars()
			.Aggregate(file, (current, invalidFileNameChar) => current.Replace(invalidFileNameChar, '_'));

	public void DeleteFile(string path) => File.Delete(path);

	public IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*",
		SearchOption searchOption = SearchOption.TopDirectoryOnly)
		=> Directory.EnumerateDirectories(path, searchPattern, searchOption);

	public Stream OpenRead(string path) => File.OpenRead(path);

	public Stream Open(string path, FileMode mode, FileAccess access, FileShare share)
		=> File.Open(path, mode, access, share);
}