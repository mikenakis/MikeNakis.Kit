namespace MikeNakis.Kit.FileSystem;

using MikeNakis.Kit.Extensions;

public abstract class FileSystemPath
{
	public static bool IsAbsolute( string path )
	{
		return SysIoIsPathFullyQualified( path );
		//return SysIoGetFullPath( path ) == path;
	}

	public static bool IsNormalized( string path )
	{
		//Assert( path == SysIo.Path.Combine( NotNull( SysIo.Path.GetDirectoryName( path ) ), SysIo.Path.GetFileName( path ) ) ); Does not work with UNC paths. The following line, however, does.
		return SysIoGetFullPath( path ) == path;
	}

	public static bool IsValidPart( string s )
	{
		return s.IndexOfAny( SysIoGetInvalidFileNameChars() ) == -1;
	}

#pragma warning disable RS0030 // Do not use banned APIs
	protected static string SysIoGetTempPath() => SysIo.Path.GetTempPath();
	protected static string SysIoGetTempFileName() => SysIo.Path.GetTempFileName();
	protected static string SysIoGetRandomFileName() => SysIo.Path.GetRandomFileName();
	protected static char[] SysIoGetInvalidFileNameChars() => SysIo.Path.GetInvalidFileNameChars();
	protected static string SysIoJoin( string path, string fileName ) => SysIo.Path.Join( path, fileName );
	protected static string? SysIoGetDirectoryName( string path ) => SysIo.Path.GetDirectoryName( path );
	protected static string SysIoGetFileNameWithoutExtension( string path ) => SysIo.Path.GetFileNameWithoutExtension( path );
	protected static string SysIoChangeExtension( string path, string extension ) => SysIo.Path.ChangeExtension( path, extension );
	protected static bool SysIoEndsInDirectorySeparator( string path ) => SysIo.Path.EndsInDirectorySeparator( path );
	protected static string? SysIoGetPathRoot( string path ) => SysIo.Path.GetPathRoot( path );
	protected static string SysIoGetCurrentDirectory() => SysIo.Directory.GetCurrentDirectory();

	protected static string SysIoGetExtension( string path ) => wrap( path, () => SysIo.Path.GetExtension( path ) );

	//PEARL: This will return `true` for "C:a".
	//protected static bool SysIoIsPathRooted( string path ) => wrap( path, () => SysIo.Path.IsPathRooted( path ) );

	protected static bool SysIoIsPathFullyQualified( string path ) => SysIo.Path.IsPathFullyQualified( path );

	protected static string SysIoGetFullPath( string path ) => wrap( path, () => SysIo.Path.GetFullPath( path ) );
	protected static string SysIoGetFileName( string path ) => wrap( path, () => SysIo.Path.GetFileName( path ) );
	protected static SysIo.DirectoryInfo? SysIoGetParent( string path ) => wrap( path, () => SysIo.Directory.GetParent( path ) );
	protected static string SysIoCombine( string path, string name ) => wrap( path, () => SysIo.Path.Combine( path, name ) );
	protected static void SysIoDirectoryMove( string path, string newPath ) => wrap( path, () => SysIo.Directory.Move( path, newPath ) );
	protected static void SysIoCreateDirectory( string path ) => wrap( path, () => SysIo.Directory.CreateDirectory( path ) );
	protected static string[] SysIoGetFiles( string path, string pattern ) => wrap( path, () => SysIo.Directory.GetFiles( path, pattern ) );
	protected static string[] SysIoGetDirectories( string path ) => wrap( path, () => SysIo.Directory.GetDirectories( path ) );
	protected static string SysIoGetRelativePath( string fromPath, string toPath ) => wrap( fromPath, () => SysIo.Path.GetRelativePath( fromPath, toPath ) );
	protected static bool SysIoExists( string path ) => wrap( path, () => SysIo.Directory.Exists( path ) );
	protected static void SysIoDirectoryDelete( string path, bool recursive ) => wrap( path, () => SysIo.Directory.Delete( path, recursive ) );
	protected static string[] SysIoGetFiles( string path, string searchPattern, SysIo.SearchOption searchOption ) => wrap( path, () => SysIo.Directory.GetFiles( path, searchPattern, searchOption ) );
	protected static string SysIoGetDirectoryRoot( string path ) => wrap( path, () => SysIo.Directory.GetDirectoryRoot( path ) );
	protected static IEnumerable<string> SysIoEnumerateFileSystemEntries( string path ) => wrap( path, () => SysIo.Directory.EnumerateFileSystemEntries( path ) );
	protected static SysIo.FileAttributes SysIoGetAttributes( string path ) => wrap( path, () => SysIo.File.GetAttributes( path ) );
	protected static Sys.DateTime SysIoGetCreationTimeUtc( string path ) => wrap( path, () => SysIo.File.GetCreationTimeUtc( path ) );
	protected static void SysIoSetCreationTimeUtc( string path, Sys.DateTime utc ) => wrap( path, () => SysIo.File.SetCreationTimeUtc( path, utc ) );
	protected static Sys.DateTime SysIoGetLastWriteTimeUtc( string path ) => wrap( path, () => SysIo.File.GetLastWriteTimeUtc( path ) );
	protected static void SysIoSetLastWriteTimeUtc( string path, Sys.DateTime utc ) => wrap( path, () => SysIo.File.SetLastWriteTimeUtc( path, utc ) );
	protected static string SysIoReadAllText( string path, SysText.Encoding? encoding ) => wrap( path, () => SysIo.File.ReadAllText( path, encoding ?? DotNetHelpers.BomlessUtf8 ) );
	protected static byte[] SysIoReadAllBytes( string path ) => wrap( path, () => SysIo.File.ReadAllBytes( path ) );
	protected static void SysIoWriteAllText( string path, string text, SysText.Encoding encoding ) => wrap( path, () => SysIo.File.WriteAllText( path, text, encoding ) );
	protected static void SysIoCopy( string path, string otherPath, bool recursive ) => wrap( path, () => SysIo.File.Copy( path, otherPath, recursive ) );
	protected static IEnumerable<string> SysIoReadLines( string path ) => wrap( path, () => SysIo.File.ReadLines( path ) );
	protected static void SysIoWriteAllBytes( string path, byte[] bytes ) => wrap( path, () => SysIo.File.WriteAllBytes( path, bytes ) );
	protected static void SysIoWriteAllText( string path, string text ) => wrap( path, () => SysIo.File.WriteAllText( path, text ) );
	protected static void SysIoFileDelete( string path ) => wrap( path, () => SysIo.File.Delete( path ) );
	protected static SysIo.StreamWriter SysIoCreateText( string path ) => wrap( path, () => SysIo.File.CreateText( path ) );
	protected static void SysIoFileMove( string path, string targetPath ) => wrap( path, () => SysIo.File.Move( path, targetPath ) );
	protected static void SysIoSetCurrentDirectory( string path ) => wrap( path, () => SysIo.Directory.SetCurrentDirectory( path ) );
	protected static SysIo.FileStream SysIoNewFileStream( string path, SysIo.FileMode mode, SysIo.FileAccess access, SysIo.FileShare share, int bufferSize = 4096, SysIo.FileOptions fileOptions = SysIo.FileOptions.None ) => wrap( path, () => new SysIo.FileStream( path, mode, access, share, bufferSize, fileOptions ) );
#pragma warning restore RS0030 // Do not use banned APIs

	static void wrap( string path, Sys.Action action, [SysCompiler.CallerMemberName] string? operationName = null )
	{
		_ = wrap( path, () =>
		{
			action.Invoke();
			return Unit.Instance;
		}, operationName );
	}

	static T wrap<T>( string path, Sys.Func<T> function, [SysCompiler.CallerMemberName] string? operationName = null )
	{
		try
		{
			return function.Invoke();
		}
		catch( Sys.Exception exception )
		{
			throw MapException( exception, path, NotNull( operationName ) );
		}
	}

	protected static Sys.Exception MapException( Sys.Exception exception, string path, string operationName )
	{
		switch( exception )
		{
			case Sys.UnauthorizedAccessException: // The caller does not have the required permission.-or- The file is an executable file that is in use.-or- is a directory.-or- it is a read-only file.
			case SysIo.DirectoryNotFoundException: // The specified path is invalid (for example, it is on an unmapped drive)
			case SysIo.DriveNotFoundException:
			case SysIo.EndOfStreamException:
			case SysIo.FileLoadException:
			case SysIo.FileNotFoundException:
			case SysIo.PathTooLongException:
			case SysIo.InternalBufferOverflowException:
			case SysIo.InvalidDataException:
				break;
			case SysIo.IOException ioException:
				// See https://www.hresult.info -- for example, https://www.hresult.info/FACILITY_WIN32/0x80070020
				switch( unchecked((uint)ioException.HResult) )
				{
					// PEARL: both HResult 0x80000009 and HResult 0x80070005 map to ACCESS_DENIED.
					case 0x80000009:
						return new AccessDeniedException( path, operationName, ioException );
					case 0x80070005: //Facility 0x007 = WIN32, Code 0x0005 = ACCESS_DENIED
						return new AccessDeniedException( path, operationName, ioException );
					case 0x800704C8: //Facility 0x007 = WIN32, Code 0x04C8 = ERROR_USER_MAPPED_FILE ("The requested operation cannot be performed on a file with a user-mapped section open.")
						return new SharingViolationException( path, operationName, ioException );
					case 0x80070020: //Facility 0x007 = WIN32, Code 0x0020 = SHARING_VIOLATION
						return new SharingViolationException( path, operationName, ioException );
					case 0x80070079: //The semaphore timeout period has expired
						break;
					case 0x800700e8: //The pipe is broken
						break;
					case 0x800700E7: //"All pipe instances are busy"
						break;
					case 0x80131620: //"Error during managed I/O". Has been observed to have message "Pipe is broken" but without an inner exception.
						break;
				}
				break;
		}
		return new FilePathWrapperException( path, operationName, exception );
	}

	public string Path { get; }

	protected FileSystemPath( string path )
	{
		Assert( IsAbsolute( path ) );
		Assert( IsNormalized( path ) );
		Path = path;
	}

	public bool Equals( FileSystemPath other ) => Path.EqualsIgnoreCase( other.Path );
	public override int GetHashCode() => Path.GetHashCode2();
	public sealed override string ToString() => Path;

	protected static bool IsNetworkPath0( SysIo.FileSystemInfo fileSystemInfo )
	{
		string path = fileSystemInfo.FullName;
		if( path.StartsWith2( "//" ) || path.StartsWith2( @"\\" ) )
			return true; // is a UNC path
		string rootPath = NotNull( SysIoGetPathRoot( path ) ); // get drive letter or \\host\share (will not return null because `path` is not null)
		SysIo.DriveInfo driveInfo = new( rootPath ); // get info about the drive
		return driveInfo.DriveType == SysIo.DriveType.Network; // return true if a network drive
	}

	// PEARL: If you attempt to access a non-existent network path, Windows will hit you with an insanely long timeout
	//        before it reports an error. I am not sure how long this timeout is, but it is certainly far longer than my
	//        patience.
	//        To remedy this, each time we access a file or directory we first invoke this method to check whether the
	//        path exists. This method detects whether the path is a network path, and if so, it checks for its presence
	//        using a reasonably short timeout.
	//        See https://stackoverflow.com/a/52661569/773113
	protected static bool FileSystemInfoExists( SysIo.FileSystemInfo fileSystemInfo )
	{
		if( IsNetworkPath0( fileSystemInfo ) )
		{
			Sys.TimeSpan timeout = Sys.TimeSpan.FromSeconds( 2 );
			bool? result = invokeWithTimeout( timeout, () => fileSystemInfo.Exists );
			if( !result.HasValue )
			{
				Log.Error( $"Waiting for network path {fileSystemInfo} timed out after {timeout.TotalSeconds} seconds" );
				return false;
			}
			return result.Value;
		}
		return fileSystemInfo.Exists;
	}

	static T? invokeWithTimeout<T>( Sys.TimeSpan timeout, Sys.Func<T> function ) where T : notnull
	{
		SysTask.Task<T> task = new( function );
		task.Start();
		if( !task.Wait( timeout ) )
			return default;
		return task.Result;
	}

	protected abstract void AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();

	public Sys.DateTime GetCreationTime()
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		return SysIoGetCreationTimeUtc( Path );
	}

	public void SetCreationTime( Sys.DateTime utc )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		SysIoSetCreationTimeUtc( Path, utc );
	}

	public Sys.DateTime GetLastWriteTime()
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		return SysIoGetLastWriteTimeUtc( Path );
	}

	public void SetLastWriteTime( Sys.DateTime utc )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		SysIoSetLastWriteTimeUtc( Path, utc );
	}
}
