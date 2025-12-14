namespace MikeNakis.Kit.FileSystem;

using System.Collections.Generic;
using MikeNakis.Kit.Extensions;
using static MikeNakis.Kit.GlobalStatics;
using Sys = System;
using SysIo = System.IO;
using SysText = System.Text;
using SysThread = System.Threading;

public sealed class FilePath : FileSystemPath
{
	public static FilePath FromAbsolutePath( string pathName )
	{
		Assert( IsAbsolute( pathName ) );
		Assert( IsNormalized( pathName ) );
		return new FilePath( pathName );
	}

	public static FilePath FromRelativeOrAbsolutePath( string relativeOrAbsolutePathName, DirectoryPath basePathIfRelative )
	{
		if( SysIoPathIsPathRooted( relativeOrAbsolutePathName ) )
			return FromAbsolutePath( SysIoPathGetFullPath( relativeOrAbsolutePathName ) );
		return basePathIfRelative.RelativeFile( relativeOrAbsolutePathName );
	}

	FilePath( string path )
			: base( path )
	{
		Assert( !SysIoPathEndsInDirectorySeparator( path ) );
	}

	public string Extension => SysIoPathGetExtension( Path );
	public string GetFileNameAndExtension() => SysIoPathGetFileName( Path );
	public string GetFileNameWithoutExtension() => SysIoPathGetFileNameWithoutExtension( Path );
	[Sys.Obsolete] public override bool Equals( object? other ) => other is FilePath kin && Equals( kin );
	public DirectoryPath Directory => DirectoryPath.FromAbsolutePath( SysIoPathGetDirectoryName( Path ).OrThrow() ); //Note: Unlike GetParent(), the GetDirectoryName() function does not access the fileSystem.
	public long GetFileLength() => new SysIo.FileInfo( Path ).Length;
	public bool StartsWith( DirectoryPath other ) => Path.StartsWithIgnoreCase( other.Path );
	public bool EndsWith( string suffix ) => Path.EndsWithIgnoreCase( suffix );
	public bool Equals( FilePath other ) => Path.Equals( other.Path, Sys.StringComparison.OrdinalIgnoreCase );
	public override int GetHashCode() => Path.GetHashCode( Sys.StringComparison.OrdinalIgnoreCase );

	public FilePath WithReplacedExtension( string extension )
	{
		Assert( IsValidPart( extension ) );
		return FromAbsolutePath( SysIoPathChangeExtension( Path, extension ) );
	}

	public bool Exists()
	{
		if( !Directory.Exists() )
			return false;
		SysIo.FileInfo fileSystemInfo = new( Path );
		return fileSystemInfo.Exists;
	}

	public string[] ReadAllLines( SysText.Encoding? encoding = null )
	{
		throwIfNetworkInaccessible();
		return SysIoFileReadAllLines( Path, encoding ?? DotNetHelpers.BomlessUtf8 );
	}

	public void WriteAllLines( IEnumerable<string> lines, SysText.Encoding? encoding = null )
	{
		throwIfNetworkInaccessible();
		SysIoFileWriteAllLines( Path, lines, encoding ?? DotNetHelpers.BomlessUtf8 );
	}

	public string ReadAllText( SysText.Encoding? encoding = null )
	{
		throwIfNetworkInaccessible();
		return SysIoFileReadAllText( Path, encoding ?? DotNetHelpers.BomlessUtf8 );
	}

	public byte[] ReadAllBytes()
	{
		throwIfNetworkInaccessible();
		return SysIoFileReadAllBytes( Path );
	}

	public void WriteAllText( string text, SysText.Encoding? encoding = null )
	{
		Directory.CreateIfNotExist();
		SysIoFileWriteAllText( Path, text, encoding ?? DotNetHelpers.BomlessUtf8 );
	}

	public void WriteAllBytes( byte[] bytes )
	{
		Directory.CreateIfNotExist();
		SysIoFileWriteAllBytes( Path, bytes );
	}

	public void Truncate()
	{
		Directory.CreateIfNotExist();
		SysIoFileWriteAllBytes( Path, Sys.Array.Empty<byte>() );
	}

	///<summary>Moves the file to a new location, allowing it to also be renamed. This will probably fail if the new
	///location is on a different drive, and this is due to a limitation of Windows.</summary>
	public void MoveTo( FilePath newPathName ) //This is essentially 'Rename'
	{
		throwIfNetworkInaccessible();
		try
		{
			SysIoFileMove( Path, newPathName.Path );
		}
		catch( SysIo.IOException exception )
		{
			throw new SysIo.IOException( $"Failed to move '{this}' to '{newPathName}'", exception );
		}
	}

	///<summary>Copies this file to the given destination file.</summary>
	///<param name="destination">The <see cref="FilePath"/> to copy to.</param>
	///<param name="overwrite">Whether overwriting of an already-existing file should be allowed.</param>
	///<remarks>Note: <paramref name="overwrite"/> controls what happens if the destination file already exists. If
	///set to <b><see langword="true"/></b>, then the destination file is overwritten. If set to <b><see langword="false"/></b>,
	///then an exception is thrown.</remarks>
	public void CopyTo( FilePath destination, bool overwrite )
	{
		throwIfNetworkInaccessible();
		SysIoFileCopy( Path, destination.Path, overwrite );
	}

	///<summary>Copies this file to the given destination directory.</summary>
	///<param name="destinationDirectoryPath">The <see cref="Directory"/> to copy to.</param>
	///<param name="overwrite">Whether overwriting of an already-existing file should be allowed.</param>
	///<remarks>Note: <paramref name="overwrite"/> controls what happens if a file with the same name already exists in
	///the destination. If set to <b><see langword="true"/></b>, then the destination file is overwritten. If set to
	///<b><see langword="false"/></b>, then an exception is thrown.</remarks>
	public void CopyTo( DirectoryPath destinationDirectoryPath, bool overwrite )
	{
		FilePath destinationFilePath = destinationDirectoryPath.File( GetFileNameAndExtension() );
		CopyTo( destinationFilePath, overwrite );
	}

	public Sys.DateTime CreationTimeUtc
	{
		get
		{
			throwIfNetworkInaccessible();
			return SysIoFileGetCreationTimeUtc( Path );
		}
	}

	public void SetCreationTimeUtc( Sys.DateTime utc )
	{
		throwIfNetworkInaccessible();
		SysIoFileSetCreationTimeUtc( Path, utc );
	}

	public Sys.DateTime LastWriteTimeUtc
	{
		get
		{
			throwIfNetworkInaccessible();
			return SysIoFileGetLastWriteTimeUtc( Path );
		}
	}

	public void SetLastWriteTimeUtc( Sys.DateTime utc )
	{
		throwIfNetworkInaccessible();
		SysIoFileSetLastWriteTimeUtc( Path, utc );
	}

	public void Delete()
	{
		throwIfNetworkInaccessible();
		try
		{
			SysIoFileDelete( Path );
		}
		catch( Sys.Exception exception )
		{
			throw MapException( exception, Path );
		}
	}

	public void Delete( int retryCount )
	{
		for( int retry = 0; true; retry++ )
			try
			{
				Delete();
				break;
			}
			catch( SharingViolationException )
			{
				if( retry < retryCount )
				{
					Log.Info( $"Retry {retry + 1} of {retryCount} while {Path} is in use..." );
					SysThread.Thread.Sleep( 100 );
					continue;
				}
				throw;
			}
		return;
	}

	public void DeleteIfExists()
	{
		if( Exists() )
			Delete();
	}

	public void CreateParentDirectoryIfNotExists()
	{
		DirectoryPath parent = Directory;
		if( parent.Exists() ) //avoids a huge timeout penalty if this is a network path and the network is inaccessible.
			return;
		Log.Info( $"Creating directory '{parent}'" );
		parent.Create();
	}

	public DirectoryPath WithoutRelativePath( string relativePath )
	{
		Assert( Path.EndsWith( relativePath, Sys.StringComparison.Ordinal ) );
		return DirectoryPath.FromAbsolutePath( Path[..^relativePath.Length] );
	}

	public SysIo.FileStream OpenBinaryForReading( SysIo.FileShare fileShare = SysIo.FileShare.Read )
	{
		throwIfNetworkInaccessible();
		return openBinary( SysIo.FileMode.Open, SysIo.FileAccess.Read, fileShare );
	}

	public SysIo.FileStream OpenBinaryForWriting()
	{
		Directory.ThrowIfNetworkInaccessible();
		return openBinary( SysIo.FileMode.OpenOrCreate, SysIo.FileAccess.ReadWrite, SysIo.FileShare.None );
	}

	public SysIo.FileStream OpenBinaryForAppending()
	{
		Directory.ThrowIfNetworkInaccessible();
		return openBinary( SysIo.FileMode.Append, SysIo.FileAccess.Write, SysIo.FileShare.Read );
	}

	SysIo.FileStream openBinary( SysIo.FileMode fileMode, SysIo.FileAccess fileAccess, SysIo.FileShare fileShare )
	{
		try
		{
			return new SysIo.FileStream( Path, fileMode, fileAccess, fileShare );
		}
		catch( Sys.Exception exception )
		{
			throw MapException( exception, Path );
		}
	}

	public SysIo.FileStream CreateBinary() => createBinary( SysIo.FileMode.Create );

	public SysIo.FileStream CreateNewBinary() => createBinary( SysIo.FileMode.CreateNew );

	SysIo.FileStream createBinary( SysIo.FileMode fileMode )
	{
		Directory.ThrowIfNetworkInaccessible();
		try
		{
			return new SysIo.FileStream( Path, fileMode, SysIo.FileAccess.ReadWrite, SysIo.FileShare.None );
		}
		catch( Sys.Exception exception )
		{
			throw MapException( exception, Path );
		}
	}

	public SysIo.TextReader OpenText( SysText.Encoding? encoding = null )
	{
		SysIo.Stream fileStream = OpenBinaryForReading(); //will be disposed by StreamReader
		return new SysIo.StreamReader( fileStream, encoding ?? DotNetHelpers.BomlessUtf8 );
	}

	public SysIo.TextWriter CreateText( bool createDirectoryIfNotExist = false )
	{
		if( createDirectoryIfNotExist )
			Directory.CreateIfNotExist();
		return SysIoFileCreateText( Path );
	}

	//avoids a huge timeout penalty if this is a network path and the network is inaccessible.
	void throwIfNetworkInaccessible()
	{
		Directory.ThrowIfNetworkInaccessible();
	}
}
