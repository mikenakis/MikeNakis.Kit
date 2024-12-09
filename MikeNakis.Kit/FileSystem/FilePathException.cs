namespace MikeNakis.Kit.FileSystem;

using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysIo = System.IO;

///<summary>Base class for file-system exceptions.</summary>
public abstract class FilePathException : SaneException
{
	public string Path { get; }

	protected FilePathException( string path, Sys.Exception? innerException = null )
			: base( innerException )
	{
		Path = path;
	}

	public override string Message => $"{GetType().Name}: Path = {Path}";
}

///<summary>Indicates that a certain operation on a certain file-path failed.</summary>
// PEARL: the dotnet I/O exceptions will tell you that something failed, but they will not tell you what kind of
// operation failed, and they will not tell you on which path it failed. We fix this here.
public class FilePathWrapperException : FilePathException
{
	public string OperationName { get; }
	public new int HResult { get; }

	public FilePathWrapperException( string path, string operationName, Sys.Exception innerException )
			: base( path, innerException )
	{
		OperationName = operationName;
		HResult = innerException.HResult;
	}

	public override string Message => $"{base.Message}; Operation = {OperationName}; HResult = 0x{HResult:X8}; InnerException = {NotNull( InnerException ).GetType().Name}: \"{InnerException!.Message}\"";
}

///<summary>Indicates that the caller does not have the required permission to access a certain resource.</summary>
///<remarks>For example, trying to read from a file without the necessary read permission.</remarks>
// PEARL: due to the fucked up way dotnet works, an "Access Denied" error may also be reported in various other
// fundamentally different situations, such as:
//   - Trying to access an executable file that is in use. (This is not a permissions error, it is a sharing violation.)
//   - Trying to open a file which is in fact a directory. (This is not a permissions error, it is a "this is not what
//     you think it is" error.)
//   - Trying to write a read-only file. (This is not a permissions error, it is a "file is not even writable" error.)
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

///<summary>Indicates that a directory cannot be created because it already exists.</summary>
public class PathAlreadyExistsException : FilePathException
{
	public PathAlreadyExistsException( string path )
			: base( path )
	{ }
}
