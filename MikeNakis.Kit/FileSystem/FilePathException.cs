namespace MikeNakis.Kit.FileSystem;

using MikeNakis.Kit.Extensions;

///<summary>Base class for file-system exceptions.</summary>
public abstract class FilePathException : SaneException
{
	public string Path { get; }
	public string OperationName { get; }

	protected FilePathException( string path, string operationName, Sys.Exception? innerException = null )
			: base( innerException )
	{
		Path = path;
		OperationName = operationName;
	}

	public override string Message => $"Path={Path}; Operation={OperationName}";
}

///<summary>Indicates that a certain operation on a certain file-path failed.</summary>
// PEARL: the dotnet I/O exceptions will tell you that something failed, but they will not tell you what kind of
// operation failed, and quite often they will not even tell you which path failed. We fix this here.
public class FilePathWrapperException : FilePathException
{
	public FilePathWrapperException( string path, string operationName, Sys.Exception innerException )
			: base( path, operationName, innerException )
	{ }

	public override string Message => $"{base.Message}; InnerException = {InnerException.OrThrow().GetType().Name}: \"{InnerException!.Message}\"";
}

///<summary>Indicates that the caller does not have the required permission to access a certain resource.</summary>
///<remarks>For example, trying to read from a file without the necessary read permission.</remarks>
// PEARL: due to the fucked up way windows/dotnet works, an "Access Denied" error may also be reported in various other
// fundamentally different situations, such as:
//   - Trying to access an executable file that is in use.
//     (This is not a permissions error, it is a sharing violation.)
//   - Trying to open a file which is in fact a directory.
//     (This is not a permissions error, it is a "this is not what you think it is" error.)
//   - Trying to write a read-only file.
//     (This is not a permissions error, it is a "file is not even writable" error.)
public class AccessDeniedException : FilePathWrapperException
{
	public AccessDeniedException( string path, string operationName, SysIo.IOException innerException )
			: base( path, operationName, innerException )
	{ }
}

///<summary>Indicates that the process cannot access a file because it is being used by another process.</summary>
public class SharingViolationException : FilePathWrapperException
{
	public SharingViolationException( string path, string operationName, SysIo.IOException innerException )
			: base( path, operationName, innerException )
	{ }
}

///<summary>Indicates that a file or directory cannot be created because it already exists.</summary>
public class PathAlreadyExistsException : FilePathException
{
	public PathAlreadyExistsException( string path, string operationName )
			: base( path, operationName )
	{ }
}
