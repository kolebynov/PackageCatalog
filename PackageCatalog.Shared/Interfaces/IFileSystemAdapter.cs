using System.Text;

namespace PackageCatalog.Shared.Interfaces;

public interface IFileSystemAdapter
{
	string ReadAllText(string path, Encoding encoding);

	Task<string> ReadAllTextAsync(string path, Encoding encoding, CancellationToken cancellationToken);

	void WriteAllText(string path, string content);

	Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken);

	bool DirectoryExists(string path);

	void CreateDirectory(string path);

	void EnsureDirectoryExists(string path);

	IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption searchOption);

	void SafeDeleteDirectory(string path);

	void DeleteFile(string path);

	IEnumerable<string> EnumerateDirectories(string path, string searchPattern = "*", SearchOption searchOption = SearchOption.TopDirectoryOnly);

	Stream OpenRead(string path);

	Stream Open(string path, FileMode mode, FileAccess access, FileShare share);

	void Copy(string sourceFileName, string destFileName, bool overwrite);

	void SafeDeleteFile(string path);

	bool FileExists(string file);

	void Move(string from, string to);

	string GetTempPath();

	long GetFileLength(string file);

	DateTimeOffset GetCreationTime(string file);

	DateTimeOffset GetLastWriteTime(string file);

	string EscapeInvalidChars(string file);
}