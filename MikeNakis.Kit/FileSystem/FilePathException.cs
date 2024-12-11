namespace MikeNakis.Kit.FileSystem;

using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysIo = System.IO;

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

///Indicates that the process insufficient privileges to access a certain resource. (The caller does not have the
/// required permission.)
// For example, trying to read from a file without the necessary read permission.
// PEARL: this "Access Denied" error may also be reported in various other fundamentally different situations, such as:
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

///Indicates that the process cannot access a file because it is being used by another process.
public class SharingViolationException : FilePathWrapperException
{
	public SharingViolationException( string path, string operationName, SysIo.IOException innerException )
			: base( path, operationName, innerException )
	{ }
}

///Indicates that a directory cannot be created because it already exists.
public class PathAlreadyExistsException : FilePathException
{
	public PathAlreadyExistsException( string path )
			: base( path )
	{ }
}
