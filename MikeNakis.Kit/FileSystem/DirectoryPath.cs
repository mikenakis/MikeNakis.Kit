namespace MikeNakis.Kit.FileSystem;

using System.Collections.Generic;
using System.Linq;
using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysIo = System.IO;

public sealed class DirectoryPath : FileSystemPath
{
	public static DirectoryPath GetTempPath() => FromAbsolutePath( SysIoGetTempPath() );

	public static DirectoryPath FromAbsolutePath( string path ) => new( path );

	public static DirectoryPath FromRelativePath( string path )
	{
		Assert( !SysIoIsPathRooted( path ) );
		string fullPath = SysIoGetFullPath( path );
		return FromAbsolutePath( fullPath );
	}

	public static DirectoryPath FromAbsoluteOrRelativePath( string path )
	{
		if( SysIoIsPathRooted( path ) )
			return FromAbsolutePath( SysIoGetFullPath( path ) );
		return FromRelativePath( path.Length == 0 ? "." : path );
	}

	public static DirectoryPath NewTempPath()
	{
		//PEARL: DotNet offers a way to create a unique file, but no way to create a unique directory.
		//       We attempt to remedy this here by using the SysIo.Path.GetRandomFileName() function, which creates a
		//       random 8+3 filename, and then we use that filename as a directory name. There is a small possibility
		//       that the filename already exists; we ignore it.
		DirectoryPath result = GetTempPath().SubDirectory( SysIoGetRandomFileName() );
		result.CreateDirectory();
		return result;
	}

	DirectoryPath( string path )
			: base( path )
	{ }

	public string GetDirectoryName() => NotNull( SysIoGetFileName( Path ) );
	public DirectoryPath GetParent() => new( NotNull( SysIoGetParent( Path )! ).FullName );
	public bool StartsWith( DirectoryPath other ) => Path.StartsWithIgnoreCase( other.Path );
	public string GetRelativePath( DirectoryPath fullPath ) => SysIoGetRelativePath( Path, fullPath.Path );
	public string GetRelativePath( FilePath fullPath ) => SysIoGetRelativePath( Path, fullPath.Path );
	public override bool Equals( object? other ) => other is DirectoryPath kin && Equals( kin );
	public override int GetHashCode() => base.GetHashCode();

	public DirectoryPath SubDirectory( string name )
	{
		Assert( name.IndexOfAny( SysIoGetInvalidFileNameChars() ) == -1 );
		return new DirectoryPath( SysIoCombine( Path, name ) );
	}

	public DirectoryPath SubPath( string name )
	{
		return new DirectoryPath( SysIoJoin( Path, name ) );
	}

	public DirectoryPath TemporaryUniqueSubDirectory() => SubDirectory( SysIoGetRandomFileName() );

	public bool IsNetworkPath()
	{
		SysIo.DirectoryInfo fileSystemInfo = new( Path );
		return IsNetworkPath0( fileSystemInfo );
	}

	public void CreateIfNotExist()
	{
		if( Exists() ) //avoids a huge timeout penalty if this is a network path and the network is inaccessible.
			return;
		// if( IsNetworkPath() ) I am not sure why I used to have this check here.
		// 	throw new Sys.UnauthorizedAccessException();
		Log.Info( $"Creating directory '{Path}'" );
		SysIoCreateDirectory( Path ); //PEARL: does not fail if the directory already exists!
	}

	public void MoveTo( DirectoryPath newPathName )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		SysIoDirectoryMove( Path, newPathName.Path );
	}

	public bool Exists()
	{
		SysIo.DirectoryInfo fileSystemInfo = new( Path );
		return FileSystemInfoExists( fileSystemInfo );
	}

	public IEnumerable<SysIo.FileInfo> GetFileInfos( string pattern )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		return new SysIo.DirectoryInfo( Path ).GetFiles( pattern );
	}

	public void CreateDirectory()
	{
		if( Exists() ) //avoids a huge timeout penalty if this is a network path and the network is inaccessible.
			throw new PathAlreadyExistsException( Path );
		// if( IsNetworkPath() )
		// 	throw new Sys.AccessViolationException( Path );
		Log.Info( $"Creating directory '{Path}'" );
		SysIoCreateDirectory( Path );
	}

	public IEnumerable<FilePath> EnumerateFiles( string pattern )
	{
		//PEARL: System.IO.Directory.GetFiles() will completely ignore the "path" parameter if the "pattern" parameter
		//       contains a path, and instead it will enumerate the files at the path contained within the pattern.
		//       To avoid this, we assert that the "pattern" parameter does not start with a path.
		Assert( !SysIoIsPathRooted( pattern ) );
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		foreach( string s in SysIoGetFiles( Path, pattern ) )
			yield return FilePath.FromAbsolutePath( s );
	}

	public IEnumerable<DirectoryPath> EnumerateDirectories()
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		foreach( string s in SysIoGetDirectories( Path ) )
			yield return new DirectoryPath( s );
	}

	public void DeleteIfExists()
	{
		// if( IsNetworkPath() )
		// 	throw new Sys.AccessViolationException( Path );
		if( SysIoExists( Path ) )
			Delete();
	}

	public void Delete()
	{
		// PEARL: the System.IO.Directory.Delete() method will throw an exception if it fails to delete a directory, but the exception does not tell which
		//        directory could not be deleted. We correct this imbecility here.
		try
		{
			SysIoDirectoryDelete( Path, true );
		}
		catch( Sys.Exception exception )
		{
			throw new SysIo.IOException( $"Could not delete directory {Path}", exception );
		}
	}

	public IEnumerable<FilePath> GetFiles( string pattern, bool recurse ) //
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		return SysIoGetFiles( Path, pattern, recurse ? SysIo.SearchOption.AllDirectories : SysIo.SearchOption.TopDirectoryOnly ).Select( FilePath.FromAbsolutePath );
	}

	IEnumerable<SysIo.FileInfo> getMatchingFiles( string pattern )
	{
		foreach( SysIo.FileSystemInfo entry in new SysIo.DirectoryInfo( Path ).EnumerateFileSystemInfos( pattern, SysIo.SearchOption.TopDirectoryOnly ) )
			if( entry is SysIo.FileInfo fileInfo )
				yield return fileInfo;
	}

	IEnumerable<SysIo.DirectoryInfo> getMatchingDirectories( string pattern )
	{
		foreach( SysIo.FileSystemInfo entry in new SysIo.DirectoryInfo( Path ).EnumerateFileSystemInfos( pattern, SysIo.SearchOption.TopDirectoryOnly ) )
			if( entry is SysIo.DirectoryInfo directoryInfo )
				yield return directoryInfo;
	}

	public FilePath GetSingleMatchingFile( string pattern ) => TryGetSingleMatchingFile( pattern ) ?? throw new SysIo.IOException( $"No file found matching {pattern} in {Path}" );

	public FilePath? TryGetSingleMatchingFile( string pattern )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		IReadOnlyCollection<SysIo.FileInfo> matchingEntries = getMatchingFiles( pattern ).Collect();
		if( matchingEntries.IsEmpty() )
			return null;
		return FilePath.FromAbsolutePath( matchingEntries.Single().FullName );
	}

	public DirectoryPath GetSingleMatchingSubdirectory( string pattern )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		IReadOnlyCollection<SysIo.DirectoryInfo> matchingEntries = getMatchingDirectories( pattern ).Collect();
		if( matchingEntries.IsEmpty() )
			throw new SysIo.IOException( $"Path not found: {Path}/{pattern}" );
		return new DirectoryPath( matchingEntries.Single().FullName );
	}

	public DirectoryPath WithoutRelativePath( string relativePath )
	{
		Assert( Path.EndsWith2( relativePath ) );
		return FromAbsolutePath( Path[..^relativePath.Length] );
	}

	public void CopyTo( DirectoryPath target, bool ifNewer )
	{
		recursiveCopyTo( new SysIo.DirectoryInfo( Path ), new SysIo.DirectoryInfo( target.Path ), ifNewer );
	}

	static void recursiveCopyTo( SysIo.DirectoryInfo sourceDirectoryInfo, SysIo.DirectoryInfo targetDirectoryInfo, bool ifNewer )
	{
		foreach( SysIo.DirectoryInfo sourceSubDirectoryInfo in sourceDirectoryInfo.GetDirectories() )
		{
			//PEARL: System.IO.DirectoryInfo.CreateSubdirectory() fails silently if the directory to be created already exists!
			//       By definition, silent failure is when the requested operation is not performed and no exception is thrown.
			//       By definition, the operation that we request a method to perform is what the name of the method says.
			//       The name of the method is "CreateSubdirectory", it is not "CreateSubdirectoryUnlessItAlreadyExists".
			SysIo.DirectoryInfo targetSubDirectoryInfo = targetDirectoryInfo.CreateSubdirectory( sourceSubDirectoryInfo.Name );
			recursiveCopyTo( sourceSubDirectoryInfo, targetSubDirectoryInfo, ifNewer );
		}
		foreach( SysIo.FileInfo sourceFileInfo in sourceDirectoryInfo.GetFiles() )
		{
			SysIo.FileInfo targetFileInfo = new( SysIoCombine( targetDirectoryInfo.FullName, sourceFileInfo.Name ) );
			if( ifNewer && targetFileInfo.Exists && sourceFileInfo.LastWriteTimeUtc > targetFileInfo.LastWriteTimeUtc )
				continue;
			sourceFileInfo.CopyTo( targetFileInfo.FullName, true );
		}
	}

	public static DirectoryPath RootOf( DirectoryPath directoryPath )
	{
		return FromAbsolutePath( SysIoGetDirectoryRoot( directoryPath.Path ) );
	}

	protected override void AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible()
	{
		if( !Exists() ) //avoids a huge timeout penalty if this is a network path and the network is inaccessible.
			throw new SysIo.DirectoryNotFoundException( Path );
	}

	public static IEnumerable<(string EntryPath, bool isDirectory)> EnumerateFileSystemEntries( string directoryPath )
	{
		Stack<string> directoryStack = new( new[] { directoryPath } );

		while( directoryStack.Count > 0 )
		{
			string path = directoryStack.Pop();
			foreach( string fileSystemEntry in SysIoEnumerateFileSystemEntries( path ) )
			{
				bool isDirectory = (SysIoGetAttributes( fileSystemEntry ) & (SysIo.FileAttributes.Directory | SysIo.FileAttributes.ReparsePoint)) == SysIo.FileAttributes.Directory;

				yield return (fileSystemEntry, isDirectory);

				if( isDirectory )
					directoryStack.Push( fileSystemEntry );
			}
		}
	}

	public IEnumerable<FileSystemPath> EnumerateRecursive()
	{
		Stack<DirectoryPath> directoryStack = new( new[] { this } );
		SysIo.EnumerationOptions enumerationOptions = new();
		enumerationOptions.RecurseSubdirectories = true;
		enumerationOptions.IgnoreInaccessible = false;
		enumerationOptions.AttributesToSkip = 0;
		enumerationOptions.MatchType = SysIo.MatchType.Simple;
		while( directoryStack.Count > 0 )
		{
			DirectoryPath currentDirectoryPath = directoryStack.Pop();
			SysIo.DirectoryInfo currentDirectoryInfo = new( currentDirectoryPath.Path );
			foreach( SysIo.FileSystemInfo fileSystemInfo in currentDirectoryInfo.EnumerateFileSystemInfos( "*", enumerationOptions ) )
			{
				switch( fileSystemInfo )
				{
					case SysIo.DirectoryInfo directoryInfo:
					{
						DirectoryPath directoryPath = new( directoryInfo.FullName );
						directoryStack.Push( directoryPath );
						yield return directoryPath;
						break;
					}
					case SysIo.FileInfo fileInfo:
						yield return FilePath.FromAbsolutePath( fileInfo.FullName );
						break;
					default:
						throw new Sys.ArgumentOutOfRangeException( nameof( fileSystemInfo ) );
				}
			}
		}
	}

	public IEnumerable<DirectoryPath> EnumerateDirectories( bool recursive )
	{
		SysIo.EnumerationOptions enumerationOptions = new();
		enumerationOptions.RecurseSubdirectories = recursive;
		enumerationOptions.IgnoreInaccessible = false;
		enumerationOptions.AttributesToSkip = 0;
		enumerationOptions.MatchType = SysIo.MatchType.Simple;
		SysIo.DirectoryInfo currentDirectoryInfo = new( Path );
		foreach( SysIo.DirectoryInfo directoryInfo in currentDirectoryInfo.EnumerateDirectories( "*", enumerationOptions ) )
			yield return new DirectoryPath( directoryInfo.FullName );
	}

	public IEnumerable<FilePath> EnumerateFiles( bool recursive, string pattern = "*" )
	{
		SysIo.EnumerationOptions enumerationOptions = new();
		enumerationOptions.RecurseSubdirectories = recursive;
		enumerationOptions.IgnoreInaccessible = false;
		enumerationOptions.AttributesToSkip = 0;
		enumerationOptions.MatchType = SysIo.MatchType.Simple;
		SysIo.DirectoryInfo currentDirectoryInfo = new( Path );
		foreach( SysIo.FileInfo fileInfo in currentDirectoryInfo.EnumerateFiles( pattern, enumerationOptions ) )
			yield return FilePath.FromAbsolutePath( fileInfo.FullName );
	}

	public bool ContainsDirectory( string directoryName )
	{
		DirectoryPath directoryPath = SubDirectory( directoryName );
		return directoryPath.Exists();
	}

	public static void CopyTo( SysIo.DirectoryInfo self, SysIo.DirectoryInfo targetDirectory, Sys.Func<SysIo.FileSystemInfo, SysIo.FileSystemInfo, bool> filter )
	{
		foreach( SysIo.FileSystemInfo fileSystemInfo in self.EnumerateFileSystemInfos( "*", SysIo.SearchOption.TopDirectoryOnly ) )
			switch( fileSystemInfo )
			{
				case SysIo.FileInfo sourceFile:
				{
					SysIo.FileInfo targetFile = new( SysIoCombine( targetDirectory.FullName, sourceFile.Name ) );
					if( !filter( sourceFile, targetFile ) )
						continue;
					SysIoCreateDirectory( targetDirectory.FullName );
					sourceFile.CopyTo( targetFile.FullName, true );
					break;
				}
				case SysIo.DirectoryInfo sourceSubDirectory:
				{
					string combined = SysIoCombine( targetDirectory.FullName, sourceSubDirectory.Name );
					SysIo.DirectoryInfo targetSubDirectory = new( combined );
					if( !filter( sourceSubDirectory, targetSubDirectory ) )
						continue;
					CopyTo( sourceSubDirectory, targetSubDirectory, filter );
					break;
				}
				default:
					Assert( false ); //what do we do with this?
					break;
			}
	}

	// PEARL: In windows programming, there is this notion of "the current directory".
	//        The notion of "the current directory" is entirely misguided. There should be no such thing.
	//        All paths should be absolute, the only piece of software that should perhaps have a notion of such
	//        a thing is the command prompt, and it should be keeping it for itself, meaning that absolutely no
	//        application should ever be aware of what some command prompt considers to be "the current directory".
	//        Unfortunately, that's not how things are.
	//        Under windows, the notion of "the current directory" is very popular.
	//        As such, it has come to be that when doing Windows programming you cannot simply refrain from dealing
	//        with the current directory; there are certain things that absolutely require getting dirty with it.
	//        For example, if you want to load an assembly and execute code in it, you have to set the current
	//        directory to be the same as the directory where the assembly is located, because the code that you
	//        are going to execute expects it to be so.
	//        To make things worse, even though AppDomains isolate lots of things, they do not isolate the damned
	//        stupid current directory: when you change its value, you are changing it for all AppDomains in the
	//        current process. (By the way, you are also changing it for all threads. How do you like that?)
	//        What all this means that you must have a mechanism for restoring the current directory to what it used
	//        to be before you set it to anything. The following method provides such a mechanism.
	public static Sys.IDisposable TemporarilySwitchCurrentDirectory( DirectoryPath newCurrentDirectory )
	{
		DirectoryPath oldCurrentDirectory = FromAbsolutePath( SysIoGetCurrentDirectory() );
		SysIoSetCurrentDirectory( newCurrentDirectory.Path );
		return new MakeshiftDisposable( () => SysIoSetCurrentDirectory( oldCurrentDirectory.Path ) );
	}

	public static DirectoryPath CurrentDirectory => FromAbsolutePath( SysIoGetCurrentDirectory() );
}
