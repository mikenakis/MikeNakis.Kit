namespace MikeNakis.Kit.FileSystem;

using System.Collections.Generic;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysIo = System.IO;
using SysText = System.Text;
using SysThreading = System.Threading;

public sealed class FilePath : FileSystemPath
{
	public static FilePath FromAbsolutePath( string path )
	{
		Assert( SysIoIsPathRooted( path ) );
		path = SysIoGetFullPath( path );
		return new FilePath( path );
	}

	public static FilePath FromRelativePath( string relativePath )
	{
		Assert( !SysIoIsPathRooted( relativePath ) );
		string path = SysIoGetFullPath( relativePath );
		return FromAbsolutePath( path );
	}

	public static FilePath FromRelativeOrAbsolutePath( string path )
	{
		if( SysIoIsPathRooted( path ) )
			return FromAbsolutePath( SysIoGetFullPath( path ) );
		return FromRelativePath( path );
	}

	public static FilePath FromRelativeOrAbsolutePath( string path, DirectoryPath basePathIfRelative )
	{
		if( !SysIoIsPathRooted( path ) )
			path = SysIoGetFullPath( SysIoCombine( basePathIfRelative.Path, path ) );
		return FromAbsolutePath( path );
	}

	public static FilePath GetTempFileName()
	{
		// PEARL: System.IO.Path.GetTempFileName() returns a unique filename with a ".tmp" extension, and there is
		//        nothing we can do about that.
		//        We cannot replace the ".tmp" extension with our own nor append our own extension to it, because:
		//        - there would be no guarantees anymore that the filename is unique.
		//        - a zero-length file with the returned filename has already been created.
		// PEARL ON PEARL: The Win32::GetTempFileName() which is used internally to implement this function DOES support
		//        passing the desired extension as a parameter; however, System.IO.Path.GetTempFileName() passes the
		//        hard-coded extension ".tmp" to it.
		string tempFileName = SysIoGetTempFileName();
		return FromAbsolutePath( tempFileName );
	}

	public static FilePath Of( DirectoryPath directoryPath, string fileName )
	{
		Assert( IsValidPart( fileName ) );
		string path = SysIoJoin( directoryPath.Path, fileName );
		return new FilePath( path );
	}

	public static FilePath Join( DirectoryPath directoryPath, string relativePath )
	{
		Assert( !SysIoIsPathRooted( relativePath ) );
		//string path = SysIoJoin( directoryPath.Path, relativePath );
		//Assert( path == SysIoGetFullPath( path ) );
		//Assert( path.StartsWith2( directoryPath.Path ) );
		//return new FilePath( path );
		string joinedPath = SysIoJoin( directoryPath.Path, relativePath );
		string fullPath = SysIoGetFullPath( joinedPath );
		return new FilePath( fullPath );
	}

	public FilePath( string path )
			: base( path )
	{
		Assert( !SysIoEndsInDirectorySeparator( path ) );
	}

	public string Extension => SysIoGetExtension( Path );
	public DirectoryPath GetDirectoryPath() => new( SysIoGetDirectoryName( Path )! );
	public string GetFileNameAndExtension() => NotNull( SysIoGetFileName( Path ) );
	public string GetFileNameWithoutExtension() => NotNull( SysIoGetFileNameWithoutExtension( Path ) );
	public DirectoryPath Directory => new( NotNull( SysIoGetParent( Path )! ).FullName );
	public SysIo.FileInfo FileInfo => new( Path );
	public bool StartsWith( DirectoryPath other ) => Path.StartsWithIgnoreCase( other.Path );
	public bool EndsWith( string suffix ) => Path.EndsWithIgnoreCase( suffix );
	public override bool Equals( object? other ) => other is FilePath kin && Equals( kin );
	public override int GetHashCode() => Path.GetHashCode2();

	public FilePath WithReplacedExtension( string extension )
	{
		Assert( IsValidPart( extension ) );
		return FromAbsolutePath( SysIoChangeExtension( Path, extension ) );
	}

	public bool Exists()
	{
		if( !Directory.Exists() )
			return false;
		SysIo.FileInfo fileSystemInfo = new( Path );
		return fileSystemInfo.Exists;
	}

	public bool ExistsAndIsWritable()
	{
		try
		{
			using( SysIoNewFileStream( Path, SysIo.FileMode.Open, SysIo.FileAccess.ReadWrite, SysIo.FileShare.Read ) )
			{ }
		}
		catch( SysIo.FileNotFoundException )
		{
			return false;
		}
		return true;
	}

	public string ReadAllText( SysText.Encoding? encoding = null )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		return SysIoReadAllText( Path, encoding );
	}

	public byte[] ReadAllBytes()
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		return SysIoReadAllBytes( Path );
	}

	public void WriteAllText( string text, SysText.Encoding? encoding = null )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		retryOnSharingViolation( () => SysIoWriteAllText( Path, text, encoding ?? DotNetHelpers.UtfBomlessEncoding ) );
	}

	public void MoveTo( FilePath newPathName ) //This is essentially 'Rename'
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		SysIoDirectoryMove( Path, newPathName.Path );
	}

	public void CopyTo( FilePath other )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		SysIoCopy( Path, other.Path, true );
	}

	public IEnumerable<string> ReadLines()
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		return SysIoReadLines( Path );
	}

	public void WriteAllBytes( byte[] bytes )
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		SysIoWriteAllBytes( Path, bytes );
	}

	public void Truncate()
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		SysIoWriteAllText( Path, "" );
	}

	public void Delete()
	{
		AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible();
		retryOnSharingViolation( () => SysIoFileDelete( Path ) );
	}

	static T retryOnSharingViolation<T>( Sys.Func<T> function, int retryCount = 10 )
	{
		for( int retry = 0; true; retry++ )
			try
			{
				return function.Invoke();
			}
			catch( SharingViolationException sharingViolationException )
			{
				if( retry >= retryCount )
					throw;
				Log.Warn( $"Retry {retry + 1} of {retryCount} due to: {sharingViolationException.Message}" );
				SysThreading.Thread.Sleep( 100 );
			}
	}

	static void retryOnSharingViolation( Sys.Action action, int retryCount = 10 )
	{
		_ = retryOnSharingViolation( () =>
		{
			action.Invoke();
			return Unit.Instance;
		}, retryCount );
	}

	public void DeleteIfExists()
	{
		if( Exists() )
			Delete();
	}

	public void CreateParentDirectoryIfNotExists()
	{
		GetDirectoryPath().CreateIfNotExist();
	}

	public DirectoryPath WithoutRelativePath( string relativePath )
	{
		Assert( Path.EndsWith2( relativePath ) );
		return DirectoryPath.FromAbsolutePath( Path[..^relativePath.Length] );
	}

	public SysIo.Stream OpenBinary( SysIo.FileAccess access = SysIo.FileAccess.Read, SysIo.FileShare? share = null )
	{
		share ??= access switch
		{
			SysIo.FileAccess.Read => SysIo.FileShare.Read,
			SysIo.FileAccess.Write => SysIo.FileShare.None,
			SysIo.FileAccess.ReadWrite => SysIo.FileShare.None,
			_ => throw new Sys.ArgumentOutOfRangeException( nameof( access ), access, null )
		};
		return SysIoNewFileStream( Path, SysIo.FileMode.Open, access, share.Value );
	}

	public SysIo.Stream CreateBinary( SysIo.FileAccess access = SysIo.FileAccess.Write, SysIo.FileShare share = SysIo.FileShare.None )
	{
		Assert( access is SysIo.FileAccess.Write or SysIo.FileAccess.ReadWrite );
		return SysIoNewFileStream( Path, SysIo.FileMode.Create, access, share );
	}

	public SysIo.TextReader OpenText( SysText.Encoding? encoding = null )
	{
		SysIo.Stream fileStream = OpenBinary();
		return new SysIo.StreamReader( fileStream, encoding ?? SysText.Encoding.UTF8 );
	}

	public SysIo.TextWriter CreateText( bool createDirectoryIfNotExist = false )
	{
		if( createDirectoryIfNotExist )
			Directory.CreateIfNotExist();
		return SysIoCreateText( Path );
	}

	public void RenameTo( FilePath targetFilePath )
	{
		SysIoFileMove( Path, targetFilePath.Path );
	}

	protected override void AvoidHugeTimeoutPenaltyIfThisIsANetworkPathAndTheNetworkIsInaccessible()
	{
		DirectoryPath directoryPath = GetDirectoryPath();
		if( !directoryPath.Exists() ) //avoids a huge timeout penalty if this is a network path and the network is inaccessible.
			throw new SysIo.FileNotFoundException( directoryPath.Path ); //this is really a "path not found" exception.
	}
}
