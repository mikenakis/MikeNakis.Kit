namespace MikeNakis.Kit.FileSystem;

using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysIo = System.IO;
using SysTask = System.Threading.Tasks;

public sealed class DirectoryPath : FileSystemPath
{
	public static DirectoryPath FromAbsolutePath( string absolutePathName )
	{
		Assert( IsAbsolute( absolutePathName ) );
		return new( absolutePathName );
	}

	public static DirectoryPath FromAbsoluteOrRelativePath( string absoluteOrRelativePathName, DirectoryPath basePathIfRelative )
	{
		if( SysIoPathIsPathRooted( absoluteOrRelativePathName ) )
			return FromAbsolutePath( absoluteOrRelativePathName );
		return basePathIfRelative.RelativeDirectory( absoluteOrRelativePathName );
	}

	///<summary>Constructor.</summary>
	///<remarks>This class is only meant to instantiated via one of its static factory methods.</remarks>
	DirectoryPath( string path )
		: base( StripTrailingPathSeparator( path ) )
	{ }

	///<summary>Tries to get the directory name.</summary>
	///<remarks>The directory name is the last element of the path.<para/>
	///Returns <c>null</c> if the <see cref="DirectoryPath"/> points at the root of a drive.<para/>
	///See also <seealso cref="GetDirectoryName()" /></remarks>
	public string? TryGetDirectoryName() => SysIoPathGetFileName( Path );

	///<summary>Gets the directory name.</summary>
	///<remarks>The directory name is the last element of the path.<para/>
	///This method will throw if the <see cref="DirectoryPath"/> points at the root of a drive.<para/>
	///See also <seealso cref="TryGetDirectoryName()"/></remarks>
	public string GetDirectoryName() => TryGetDirectoryName().OrThrow();

	public DirectoryPath? GetParent()
	{
		string? parentPathName = SysIoPathGetDirectoryName( Path );
		if( parentPathName == null )
			return null;
		Assert( parentPathName.Length < Path.Length );
		return FromAbsolutePath( parentPathName );
	}

	public bool StartsWith( DirectoryPath other ) => Path.StartsWith( other.Path, Sys.StringComparison.OrdinalIgnoreCase );

	public string GetRelativePath( DirectoryPath directoryPath ) => getRelativePath( Path, directoryPath.Path ) ?? throw new AssertionFailureException();
	public string GetRelativePath( FilePath filePath ) => getRelativePath( Path, filePath.Path ) ?? throw new AssertionFailureException();
	public string GetRelativePathToChild( DirectoryPath childDirectoryPath ) => tryGetRelativePathToChild( Path, childDirectoryPath.Path ) ?? throw new AssertionFailureException();
	public string GetRelativePathToChild( FilePath childFilePath ) => tryGetRelativePathToChild( Path, childFilePath.Path ) ?? throw new AssertionFailureException();

	[Sys.Obsolete] public override bool Equals( object? other ) => other is DirectoryPath kin && Equals( kin );
	public bool Equals( DirectoryPath other ) => Path.Equals( other.Path, Sys.StringComparison.OrdinalIgnoreCase );
	public override int GetHashCode() => Path.GetHashCode( Sys.StringComparison.OrdinalIgnoreCase );

	/// <summary>Creates a new <see cref="DirectoryPath"/> from this <see cref="DirectoryPath"/> and a directory name.</summary>
	/// <param name="directoryName">The directory name.</param>
	/// <remarks>An exception will be thrown if <paramref name="directoryName"/> is rooted.<para/>
	/// An exception will be thrown if <paramref name="directoryName"/> contains invalid characters.</remarks>
	public DirectoryPath Directory( string directoryName )
	{
		IsRelativePathName( directoryName ).OrThrow();
		IsValidFileName( directoryName ).OrThrow();
		string fullPathName = SysIoPathCombine( Path, directoryName );
		return new DirectoryPath( fullPathName );
	}

	/// <summary>Creates a new <see cref="DirectoryPath"/> from this <see cref="DirectoryPath"/> and a relative path.</summary>
	/// <param name="relativePathName">The relative path. If empty, '.' will be used.</param>
	/// <remarks>An exception will be thrown if <paramref name="relativePathName"/> is rooted.<para/>
	/// An exception will be thrown if <paramref name="relativePathName"/> contains invalid characters.</remarks>
	public DirectoryPath RelativeDirectory( string relativePathName )
	{
		if( relativePathName == "" )
			relativePathName = ".";
		Assert( IsRelativePathName( relativePathName ) );
		Assert( IsValidPathName( relativePathName ) );
		string fullPathName = SysIoPathGetFullPath( SysIoPathCombine( Path, relativePathName ) );
		return new DirectoryPath( fullPathName );
	}

	/// <summary>Creates a new <see cref="FilePath"/> from this <see cref="DirectoryPath"/> and a relative path.</summary>
	/// <param name="relativePathName">The relative path. If empty, '.' will be used.</param>
	/// <remarks>An exception will be thrown if <paramref name="relativePathName"/> is rooted.<para/>
	/// An exception will be thrown if <paramref name="relativePathName"/> contains invalid characters.</remarks>
	public FilePath RelativeFile( string relativePathName )
	{
		if( relativePathName == "" )
			relativePathName = ".";
		IsRelativePathName( relativePathName ).OrThrow();
		IsValidPathName( relativePathName ).OrThrow();
		string fullPathName = SysIoPathGetFullPath( SysIoPathCombine( Path, relativePathName ) );
		return FilePath.FromAbsolutePath( fullPathName );
	}

	/// <summary>Creates a new <see cref="FilePath"/> from this <see cref="DirectoryPath"/> and a filename.</summary>
	/// <param name="fileName">The filename.</param>
	/// <remarks>An exception will be thrown if <paramref name="fileName"/> contains invalid characters.</remarks>
	public FilePath File( string fileName )
	{
		IsValidFileName( fileName ).OrThrow();
		string fullPathName = SysIoPathCombine( Path, fileName );
		return FilePath.FromAbsolutePath( fullPathName );
	}
	/// <summary>Returns a random, unique <see cref="FilePath"/> under this <see cref="DirectoryPath"/>. (A corresponding actual file is also created, but then closed.)</summary>
	/// <param name="extension">The file extension to append to the random filename.</param>
	public FilePath GenerateUniqueFilePath( string extension = ".tmp" )
	{
		(FilePath filePath, SysIo.Stream stream) = CreateUniqueFile( extension );
		stream.Close();
		return filePath;
	}

	/// <summary>Creates a new file with a random, unique <see cref="FilePath"/> under this <see cref="DirectoryPath"/>. (Both the stream and the filepath are returned.)</summary>
	/// <param name="extension">The file extension to append to the random filename.</param>
	public (FilePath, SysIo.Stream) CreateUniqueFile( string extension = ".tmp" )
	{
		FilePath filePath;
		SysIo.Stream stream;
		while( true )
		{
			string fileName = SysIoPathGetFileNameWithoutExtension( SysIoPathGetRandomFileName() ) + extension;
			filePath = File( fileName );
			Result<SysIo.Stream> result = tryCreateFile( filePath );
			if( !result.IsFailure )
			{
				stream = result.Payload;
				break;
			}
			Log.Info( $"failed to generate unique temporary filename because {result.Expectation.Message}" );
		}
		return (filePath, stream);

		static Result<SysIo.Stream> tryCreateFile( FilePath filePath )
		{
			// Old approach: try to create the file without allowing sharing, so as to fail if the file is locked.
			// Disadvantage: an existing file may be overwritten, unless it is kept locked.
			if( False )
			{
				try
				{
					return filePath.CreateBinary();
				}
				catch( SharingViolationException )
				{
					return new CustomExpectation( $"creation of file '{filePath}' failed due to sharing violation" );
				}
			}
			// New approach: try to create a **_new_** file, (meaning that it must not already exist,) so as to fail if
			// the file already exists.
			// Advantage: existing files will never be overwritten, even if they are not kept locked anymore.
			else
			{
				try
				{
					return filePath.CreateNewBinary();
				}
				catch( PathAlreadyExistsException )
				{
					return new CustomExpectation( $"creation of file '{filePath}' failed because it already exists" );
				}
			}
		}
	}

	/// <summary>Creates a new <see cref="DirectoryPath"/> from this <see cref="DirectoryPath"/> and a unique directory name.</summary>
	public DirectoryPath CreateUniqueDirectory()
	{
		DirectoryPath directoryPath;
		while( true )
		{
			string directoryName = SysIoPathGetFileNameWithoutExtension( SysIoPathGetRandomFileName() );
			directoryPath = Directory( directoryName );
			if( !directoryPath.Exists() )
				break;
		}
		directoryPath.Create();
		return directoryPath;
	}

	public bool IsNetworkPath()
	{
		SysIo.DirectoryInfo fileSystemInfo = new( Path );
		return isNetworkPath( fileSystemInfo );
	}

	public void CreateIfNotExist()
	{
		if( Exists() ) //avoids a huge timeout penalty if this is a network path and the network is inaccessible.
			return;
		SysIoDirectoryCreateDirectory( Path ); //PEARL: does not fail if the directory already exists!
	}

	public void MoveTo( DirectoryPath newPathName )
	{
		ThrowIfNetworkInaccessible();
		SysIoDirectoryMove( Path, newPathName.Path );
	}

	public bool Exists()
	{
		SysIo.DirectoryInfo fileSystemInfo = new( Path );
		return fileSystemInfoExists( fileSystemInfo );

		// PEARL: if you attempt to access a non-existent network path, Windows will hit you with an insanely long
		//        timeout before it reports an error. I am not sure how long this timeout is, but it is certainly far
		//        longer than my patience.
		//        To remedy this, each time we want to access a file or directory we need to first invoke this method to
		//        check whether it exists.
		//        This method detects whether the path is a network path, and if so, then it checks for its presence
		//        using a reasonably short timeout.
		//        See https://stackoverflow.com/a/52661569/773113
		static bool fileSystemInfoExists( SysIo.FileSystemInfo fileSystemInfo )
		{
			if( isNetworkPath( fileSystemInfo ) )
			{
				SysTask.Task<bool> task = new( () => fileSystemInfo.Exists );
				task.Start();
				Sys.TimeSpan timeout = Sys.TimeSpan.FromSeconds( 2 );
				if( !task.Wait( timeout ) )
				{
					Log.Error( $"Waiting for network path '{fileSystemInfo}' timed out after {timeout.TotalSeconds} seconds" );
					return false; //the operation timed out, so for all practical purposes, the file or directory does not exist.
				}
				return task.Result;
			}
			return fileSystemInfo.Exists;
		}
	}

	///<summary>Creates the directory; fails if the directory already exists.</summary>
	public void Create()
	{
		// PEARL: System.Io.Directory.CreateDirectory( path ) will create all directories and subdirectories in the
		//     specified path.  There is no function to create just the last element of the path and fail if any of the
		//     previous path elements is missing.
		// PEARL: if you try to create a directory that already exists, System.Io.Directory.CreateDirectory( path ) will
		// SILENTLY FAIL to report the error.  We fix this here.
		if( Exists() )
			throw new Sys.Data.DuplicateNameException();
		SysIoDirectoryCreateDirectory( Path );
	}

	///<summary>Enumerates files and directories matching a given search pattern, optionally recursing into subdirectories.</summary>
	///<remarks>Beware: the search pattern is not a regular expression; it only supports the '?' and '*' wildcards.
	///Also, beware of quirks discussed in the "Remarks" section of the documentation of <see cref="SysIo.Directory.EnumerateFiles( string, string, SysIo.SearchOption )" />
	///https://learn.microsoft.com/en-us/dotnet/api/system.io.directory.enumeratefiles?view=net-9.0
	///Also, beware of bugs, like the one discussed here: https://stackoverflow.com/q/250834/773113</remarks>
	public IEnumerable<FileSystemPath> Enumerate( string searchPattern, bool recursive = false )
	{
		//PEARL: if the "searchPattern" parameter contains a path, then System.IO.Directory.EnumerateFiles() will
		//       completely ignore the "path" parameter, and instead it will enumerate the files at the path contained
		//       within the pattern.
		//       To avoid this, we assert that the "pattern" parameter does not start with a path.
		Assert( searchPattern == searchPattern.Trim() );
		Assert( !SysIoPathIsPathRooted( searchPattern ) );
		Assert( !ContainsDirectorySeparator( searchPattern ) );
		Assert( searchPattern != ".." );
		ThrowIfNetworkInaccessible();

		//TODO: make use of the "EnumerationOptions" mentioned here: https://stackoverflow.com/a/78698857/773113
		SysIo.DirectoryInfo directoryInfo = new( Path );
		return directoryInfo.EnumerateFileSystemInfos( searchPattern, recursive ? SysIo.SearchOption.AllDirectories : SysIo.SearchOption.TopDirectoryOnly ) //
			.Select( fileSystemPathFromFileSystemInfo );

		static FileSystemPath fileSystemPathFromFileSystemInfo( SysIo.FileSystemInfo fileSystemInfo )
		{
			return fileSystemInfo switch
			{
				SysIo.FileInfo fileInfo => FilePath.FromAbsolutePath( fileInfo.FullName ),
				SysIo.DirectoryInfo directoryInfo => FromAbsolutePath( directoryInfo.FullName ),
				_ => throw new AssertionFailureException()
			};
		}
	}

	///<summary>Enumerates files matching a given search pattern, optionally recursing into subdirectories.</summary>
	///<remarks>See <see cref="Enumerate(string,bool)"/></remarks>
	public IEnumerable<FilePath> EnumerateFiles( string searchPattern, bool recursive = false ) => Enumerate( searchPattern, recursive ).OfType<FilePath>();

	///<summary>Enumerates directories, optionally recursively.</summary>
	///<remarks>See <see cref="Enumerate(string,bool)"/></remarks>
	public IEnumerable<DirectoryPath> EnumerateDirectories( bool recursive = false ) => Enumerate( "*", recursive ).OfType<DirectoryPath>();

	//public IEnumerable<FileSystemPath> EnumerateEntries( bool recursive )
	//{
	//	SysIo.EnumerationOptions enumerationOptions = new();
	//	enumerationOptions.RecurseSubdirectories = recursive;
	//	enumerationOptions.IgnoreInaccessible = false;
	//	enumerationOptions.AttributesToSkip = 0;
	//	enumerationOptions.MatchType = SysIo.MatchType.Simple;
	//	SysIo.DirectoryInfo currentDirectoryInfo = new( Path );
	//	foreach( SysIo.FileSystemInfo fileSystemInfo in currentDirectoryInfo.EnumerateFileSystemInfos( "*", enumerationOptions ) )
	//	{
	//		yield return fileSystemInfo switch
	//		{
	//			SysIo.DirectoryInfo directoryInfo => FromAbsolutePath( fileSystemInfo.FullName ),
	//			SysIo.FileInfo fileInfo => FilePath.FromAbsolutePath( fileSystemInfo.FullName ),
	//			_ => throw new AssertionFailureException()
	//		};
	//	}
	//}

	public void DeleteIfExists( bool recursive = false )
	{
		if( SysIoDirectoryExists( Path ) )
			Delete( recursive );
	}

	///<summary>Deletes the directory; fails if the directory does not exist. Fails if it is not empty, unless recursive.</summary>
	///<param name="recursive">If <code>true</code>, deletes all files in this directory, and all directories
	///recursively, before deleting this directory.</param>
	public void Delete( bool recursive = false )
	{
		try
		{
			SysIoDirectoryDelete( Path, recursive );
		}
		catch( Sys.Exception exception )
		{
			throw new SysIo.IOException( $"Could not delete directory {Path}", exception );
		}
	}

	public FilePath GetSingleMatchingFile( string pattern )
	{
		ThrowIfNetworkInaccessible();
		IReadOnlyList<SysIo.FileInfo> matchingEntries = getMatchingFiles( pattern ).Collect();
		if( matchingEntries.Count == 0 )
			throw new SysIo.FileNotFoundException( $"Path not found: {Path}/{pattern}" );
		return FilePath.FromAbsolutePath( matchingEntries.Single().FullName );

		IEnumerable<SysIo.FileInfo> getMatchingFiles( string pattern )
		{
			foreach( SysIo.FileSystemInfo? entry in new SysIo.DirectoryInfo( Path ).EnumerateFileSystemInfos( pattern, SysIo.SearchOption.TopDirectoryOnly ) )
				if( entry is SysIo.FileInfo fileInfo )
					yield return fileInfo;
		}
	}

	public DirectoryPath GetSingleMatchingSubdirectory( string pattern )
	{
		ThrowIfNetworkInaccessible();
		IReadOnlyList<SysIo.DirectoryInfo> matchingEntries = getMatchingDirectories( pattern ).Collect();
		if( matchingEntries.Count == 0 )
			throw new SysIo.FileNotFoundException( $"Path not found: {Path}/{pattern}" );
		return new DirectoryPath( matchingEntries.Single().FullName );

		IEnumerable<SysIo.DirectoryInfo> getMatchingDirectories( string pattern )
		{
			foreach( SysIo.FileSystemInfo? entry in new SysIo.DirectoryInfo( Path ).EnumerateFileSystemInfos( pattern, SysIo.SearchOption.TopDirectoryOnly ) )
				if( entry is SysIo.DirectoryInfo directoryInfo )
					yield return directoryInfo;
		}
	}

	public IEnumerable<FilePath> Walk( bool depthFirst = false )
	{
		ThrowIfNetworkInaccessible();

		Sys.Action<DirectoryPath> push;
		Sys.Func<DirectoryPath> pop;
		Sys.Func<bool> nonEmpty;
		if( depthFirst )
		{
			Queue<DirectoryPath> queue = new();
			push = queue.Enqueue;
			pop = queue.Dequeue;
			nonEmpty = () => queue.Count > 0;
		}
		else
		{
			Stack<DirectoryPath> stack = new();
			push = stack.Push;
			pop = stack.Pop;
			nonEmpty = () => stack.Count > 0;
		}
		push.Invoke( this );
		while( nonEmpty.Invoke() )
		{
			DirectoryPath directory = pop.Invoke();
			IEnumerable<FileSystemPath> paths;
			try
			{
				paths = enumeratePaths( directory.Path );
			}
			catch( Sys.Exception exception )
			{
				Log.Error( $"Failed to list contents of '{directory}': ", exception.Message );
				continue;
			}
			foreach( FileSystemPath path in paths )
			{
				switch( path )
				{
					case DirectoryPath directoryPath:
						push.Invoke( directoryPath );
						break;
					case FilePath filePath:
						yield return filePath;
						break;
					default:
						throw new AssertionFailureException();
				}
			}
		}

		static IEnumerable<FileSystemPath> enumeratePaths( string path )
		{
			foreach( SysIo.FileSystemInfo? fileSystemInfo in new SysIo.DirectoryInfo( path ).EnumerateFileSystemInfos( "*", SysIo.SearchOption.TopDirectoryOnly ) )
			{
				yield return fileSystemInfo switch
				{
					SysIo.DirectoryInfo directoryInfo => new DirectoryPath( directoryInfo.FullName ),
					SysIo.FileInfo fileInfo => FilePath.FromAbsolutePath( fileInfo.FullName ),
					_ => throw new AssertionFailureException()
				};
			}
		}
	}

	static string? tryGetRelativePathToChild( string parentPath, string childPath )
	{
		if( !childPath.StartsWith( parentPath, Sys.StringComparison.Ordinal ) )
			return null;
		int start = skipSeparators( parentPath.Length, childPath );
		return childPath[start..];

		static int skipSeparators( int i, string pathName )
		{
			while( i < pathName.Length && "\\/".Contains2( pathName[i] ) )
				i++;
			return i;
		}
	}

	/// Unfortunately, dotnet has SysIo.Path.GetRelativePath(), but netframework does not.
	static string getRelativePath( string fromPath, string toPath )
	{
		int upCount = 0;
		while( true )
		{
			string? relativePath = tryGetRelativePathToChild( fromPath, toPath );
			if( relativePath != null )
				return Enumerable.Repeat( @"..\", upCount ).MakeString() + relativePath;
			string? parent = SysIoPathGetDirectoryName( fromPath );
			if( parent == null )
				return toPath;
			upCount++;
			fromPath = parent;
		}
	}

	public void CopyTo( DirectoryPath target, bool ifNewer = false )
	{
		recurse( new SysIo.DirectoryInfo( Path ), new SysIo.DirectoryInfo( target.Path ), ifNewer );

		static void recurse( SysIo.DirectoryInfo sourceDirectoryInfo, SysIo.DirectoryInfo targetDirectoryInfo, bool ifNewer )
		{
			foreach( SysIo.DirectoryInfo sourceSubDirectoryInfo in sourceDirectoryInfo.GetDirectories() )
			{
				//PEARL: System.IO.DirectoryInfo.CreateSubdirectory() fails silently if the directory to be created already exists!
				//       By definition, silent failure is when the requested operation is not performed and no exception is thrown.
				//       By definition, the operation that we request a method to perform is what the name of the method says.
				//       The name of the method is "CreateSubdirectory", it is not "CreateSubdirectoryUnlessItAlreadyExists".
				SysIo.DirectoryInfo targetSubDirectoryInfo = targetDirectoryInfo.CreateSubdirectory( sourceSubDirectoryInfo.Name );
				recurse( sourceSubDirectoryInfo, targetSubDirectoryInfo, ifNewer );
			}
			foreach( SysIo.FileInfo sourceFileInfo in sourceDirectoryInfo.EnumerateFiles() )
			{
				SysIo.FileInfo targetFileInfo = new( SysIoPathCombine( targetDirectoryInfo.FullName, sourceFileInfo.Name ) );
				if( ifNewer && targetFileInfo.Exists && sourceFileInfo.LastWriteTimeUtc > targetFileInfo.LastWriteTimeUtc )
					continue;
				_ = sourceFileInfo.CopyTo( targetFileInfo.FullName, true );
			}
		}
	}

	static bool isNetworkPath( SysIo.FileSystemInfo fileSystemInfo )
	{
		string path = fileSystemInfo.FullName;
		string rootPath = SysIoPathGetPathRoot( path ) ?? throw new AssertionFailureException(); // get drive letter or \\host\share
		if( path.StartsWith( @"//", Sys.StringComparison.Ordinal ) || path.StartsWith( @"\\", Sys.StringComparison.Ordinal ) )
			return true; // is a UNC path
		SysIo.DriveInfo driveInfo = new( rootPath ); // get info about the drive
		return driveInfo.DriveType == SysIo.DriveType.Network; // return true if a network drive
	}

	///<summary>Deletes all files in the directory and all subdirectories in the directory, recursively. Does not delete
	///the directory itself.</summary>
	public void Empty()
	{
		ThrowIfNetworkInaccessible();
		SysIo.DirectoryInfo directoryInfo = new( Path );
		foreach( SysIo.FileInfo fileInfo in directoryInfo.GetFiles() )
			fileInfo.Delete();
		foreach( SysIo.DirectoryInfo subDirectoryInfo in directoryInfo.GetDirectories() )
			subDirectoryInfo.Delete( recursive: true );
	}

	public void EmptyOrCreate()
	{
		if( Exists() )
			Empty();
		else
			Create();
	}

	public bool Contains( DirectoryPath child ) => contains( Path, child.Path );

	public bool Contains( FilePath child ) => contains( Path, child.Path );

	static bool contains( string pathName, string childPathName )
	{
		return childPathName.StartsWith( pathName, Sys.StringComparison.Ordinal );
	}

	//avoids a huge timeout penalty if this is a network path and the network is inaccessible.
	internal void ThrowIfNetworkInaccessible()
	{
		if( !Exists() )
			throw new SysIo.DirectoryNotFoundException( Path );
	}
}
