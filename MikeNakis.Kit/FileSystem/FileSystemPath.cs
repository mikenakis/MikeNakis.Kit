namespace MikeNakis.Kit.FileSystem;

using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;

public abstract class FileSystemPath
{
	public static bool IsAbsolute( string path )
	{
		bool result = SysIoPathIsPathRooted( path );
		Assert( result == SysIoPathIsPathFullyQualified( path ) );
		Assert( result == (SysIoPathGetFullPath( path ) == path) );
		return result;
	}

	public static bool IsNormalized( string path )
	{
		//Assert( path == SysIo.Path.Combine( SysIo.Path.GetDirectoryName( path ).OrThrow(), SysIo.Path.GetFileName( path ) ) ); Does not work with UNC paths. The following line, however, does.
		return SysIoPathGetFullPath( path ) == path;
	}

	public static bool IsValidPart( string part )
	{
		return part.IndexOfAny( SysIoPathGetInvalidFileNameChars() ) == -1;
	}

	public static bool IsValidPathName( string pathName )
	{
		return pathName.IndexOfAny( SysIoPathGetInvalidPathChars() ) == -1;
	}

	public static bool IsValidFileName( string fileName )
	{
		if( fileName == "." )
			return false; //invalid because it refers to a directory.
		return IsValidPart( fileName );
	}

	public static bool IsRelativePathName( string pathName )
	{
		return !SysIoPathIsPathRooted( pathName );
	}

	public static string? RemoveInvalidFileNameCharacters( string filename )
	{
		string result = string.Join( "", filename.Split( SysIoPathGetInvalidFileNameChars() ) );
		return result == "" ? null : result;
	}

	public static string RemoveExtension( string filename )
	{
		return SysIoPathGetFileNameWithoutExtension( filename );
	}

	protected static string StripTrailingPathSeparator( string path )
	{
		char lastCharacter = path[^1];
		if( IsDirectorySeparator( lastCharacter ) )
			return path[..^1];
		return path;
	}

#pragma warning disable RS0030 // Do not use banned APIs
	protected static bool IsDirectorySeparator( char c ) => c == SysIo.Path.DirectorySeparatorChar || c == SysIo.Path.AltDirectorySeparatorChar;
	protected static bool ContainsDirectorySeparator( string s ) => s.IndexOfAny( new char[] { SysIo.Path.DirectorySeparatorChar, SysIo.Path.AltDirectorySeparatorChar } ) != -1;

	protected static string SysIoPathGetTempPath() => SysIo.Path.GetTempPath();
	protected static string SysIoPathGetTempFileName() => SysIo.Path.GetTempFileName();
	protected static string SysIoPathGetRandomFileName() => SysIo.Path.GetRandomFileName();
	protected static char[] SysIoPathGetInvalidFileNameChars() => SysIo.Path.GetInvalidFileNameChars(); //TODO: rename
	protected static char[] SysIoPathGetInvalidPathChars() => SysIo.Path.GetInvalidPathChars(); //TODO: rename
	protected static string SysIoPathJoin( string path, string fileName ) => SysIo.Path.Join( path, fileName );
	protected static string? SysIoPathGetDirectoryName( string path ) => SysIo.Path.GetDirectoryName( path );
	protected static string SysIoPathGetFileNameWithoutExtension( string path ) => SysIo.Path.GetFileNameWithoutExtension( path ); //TOIDO: rename
	protected static string SysIoPathChangeExtension( string path, string extension ) => SysIo.Path.ChangeExtension( path, extension );
	protected static bool SysIoPathEndsInDirectorySeparator( string path ) => SysIo.Path.EndsInDirectorySeparator( path );
	protected static string? SysIoPathGetPathRoot( string path ) => SysIo.Path.GetPathRoot( path );
	protected static string SysIoPathGetCurrentDirectory() => SysIo.Directory.GetCurrentDirectory();
	protected static string SysIoPathGetExtension( string path ) => wrap( path, () => SysIo.Path.GetExtension( path ) );

	//PEARL: This will return `true` for "C:a".
	protected static bool SysIoPathIsPathRooted( string path ) => wrap( path, () => SysIo.Path.IsPathRooted( path ) );

	protected static bool SysIoPathIsPathFullyQualified( string path ) => SysIo.Path.IsPathFullyQualified( path ); //TODO: rename
	protected static string SysIoPathGetFullPath( string path ) => wrap( path, () => SysIo.Path.GetFullPath( path ) );
	protected static string SysIoPathGetFileName( string path ) => wrap( path, () => SysIo.Path.GetFileName( path ) );
	protected static SysIo.DirectoryInfo? SysIoDirectoryGetParent( string path ) => wrap( path, () => SysIo.Directory.GetParent( path ) );
	protected static string SysIoPathCombine( string path, string name ) => wrap( path, () => SysIo.Path.Combine( path, name ) );
	protected static void SysIoDirectoryMove( string path, string newPath ) => wrap( path, () => SysIo.Directory.Move( path, newPath ) );
	protected static void SysIoDirectoryCreateDirectory( string path ) => wrap( path, () => SysIo.Directory.CreateDirectory( path ) );
	protected static string[] SysIoDirectoryGetFiles( string path, string pattern ) => wrap( path, () => SysIo.Directory.GetFiles( path, pattern ) );
	protected static string[] SysIoDirectoryGetDirectories( string path ) => wrap( path, () => SysIo.Directory.GetDirectories( path ) );
	protected static string SysIoPathGetRelativePath( string fromPath, string toPath ) => wrap( fromPath, () => SysIo.Path.GetRelativePath( fromPath, toPath ) );
	protected static bool SysIoDirectoryExists( string path ) => wrap( path, () => SysIo.Directory.Exists( path ) );
	protected static void SysIoDirectoryDelete( string path, bool recursive ) => wrap( path, () => SysIo.Directory.Delete( path, recursive ) );
	protected static string[] SysIoDirectoryGetFiles( string path, string searchPattern, SysIo.SearchOption searchOption ) => wrap( path, () => SysIo.Directory.GetFiles( path, searchPattern, searchOption ) );
	protected static string SysIoDirectoryGetDirectoryRoot( string path ) => wrap( path, () => SysIo.Directory.GetDirectoryRoot( path ) );
	protected static IEnumerable<string> SysIoDirectoryEnumerateFileSystemEntries( string path ) => wrap( path, () => SysIo.Directory.EnumerateFileSystemEntries( path ) );
	protected static SysIo.FileAttributes SysIoFileGetAttributes( string path ) => wrap( path, () => SysIo.File.GetAttributes( path ) );
	protected static Sys.DateTime SysIoFileGetCreationTimeUtc( string path ) => wrap( path, () => SysIo.File.GetCreationTimeUtc( path ) );
	protected static void SysIoFileSetCreationTimeUtc( string path, Sys.DateTime utc ) => wrap( path, () => SysIo.File.SetCreationTimeUtc( path, utc ) );
	protected static Sys.DateTime SysIoFileGetLastWriteTimeUtc( string path ) => wrap( path, () => SysIo.File.GetLastWriteTimeUtc( path ) );
	protected static void SysIoFileSetLastWriteTimeUtc( string path, Sys.DateTime utc ) => wrap( path, () => SysIo.File.SetLastWriteTimeUtc( path, utc ) );
	protected static string[] SysIoFileReadAllLines( string path, SysText.Encoding? encoding ) => wrap( path, () => SysIo.File.ReadAllLines( path, encoding ?? DotNetHelpers.BomlessUtf8 ) );
	protected static void SysIoFileWriteAllLines( string path, IEnumerable<string> lines, SysText.Encoding? encoding ) => wrap( path, () => SysIo.File.WriteAllLines( path, lines, encoding ?? DotNetHelpers.BomlessUtf8 ) );
	protected static string SysIoFileReadAllText( string path, SysText.Encoding? encoding ) => wrap( path, () => SysIo.File.ReadAllText( path, encoding ?? DotNetHelpers.BomlessUtf8 ) );
	protected static byte[] SysIoFileReadAllBytes( string path ) => wrap( path, () => SysIo.File.ReadAllBytes( path ) );
	protected static void SysIoFileWriteAllText( string path, string text, SysText.Encoding encoding ) => wrap( path, () => SysIo.File.WriteAllText( path, text, encoding ) );
	protected static IEnumerable<string> SysIoFileReadLines( string path ) => wrap( path, () => SysIo.File.ReadLines( path ) );
	protected static void SysIoFileWriteAllBytes( string path, byte[] bytes ) => wrap( path, () => SysIo.File.WriteAllBytes( path, bytes ) );
	protected static void SysIoFileWriteAllText( string path, string text ) => wrap( path, () => SysIo.File.WriteAllText( path, text ) );
	protected static void SysIoFileDelete( string path ) => wrap( path, () => SysIo.File.Delete( path ) );
	protected static SysIo.StreamWriter SysIoFileCreateText( string path ) => wrap( path, () => SysIo.File.CreateText( path ) );
	protected static void SysIoFileMove( string path, string targetPath ) => wrap( path, () => SysIo.File.Move( path, targetPath ) );
	protected static void SysIoFileCopy( string path, string targetPath, bool overwrite ) => wrap( path, () => SysIo.File.Copy( path, targetPath, overwrite ) );
	protected static void SysIoDirectorySetCurrentDirectory( string path ) => wrap( path, () => SysIo.Directory.SetCurrentDirectory( path ) );
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
			throw MapException( exception, path, operationName.OrThrow() );
		}
	}

	protected static Sys.Exception MapException( Sys.Exception exception, string path, [SysCompiler.CallerMemberName] string operationName = "" )
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
					case 0x80070050: //Facility 0x007 = WIN32, Code 0x0050 = FILE_EXISTS
						return new PathAlreadyExistsException( path, operationName );
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

	protected FileSystemPath( string pathName )
	{
		Assert( IsAbsolute( pathName ) );
		Assert( IsNormalized( pathName ) );
		Path = pathName;
	}

	public sealed override string ToString() => Path;
}
