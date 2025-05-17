namespace MikeNakis.Kit.FileSystem;

using MikeNakis.Kit.Extensions;

public sealed class FilePath : FileSystemPath
{
	public static FilePath FromAbsolutePath( string path )
	{
		Assert( IsAbsolute( path ) );
		path = SysIoGetFullPath( path );
		return new FilePath( path );
	}

	public static FilePath FromRelativePath( string relativePath )
	{
		Assert( !IsAbsolute( relativePath ) );
		string path = SysIoGetFullPath( relativePath );
		return FromAbsolutePath( path );
	}

	public static FilePath FromRelativeOrAbsolutePath( string path )
	{
		if( IsAbsolute( path ) )
			return FromAbsolutePath( SysIoGetFullPath( path ) );
		return FromRelativePath( path );
	}

	public static FilePath FromRelativeOrAbsolutePath( string path, DirectoryPath basePathIfRelative )
	{
		if( !IsAbsolute( path ) )
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

	//TODO: get rid of, replace with call to DirectoryPath.File()
	public static FilePath Of( DirectoryPath directoryPath, string fileName ) => directoryPath.File( fileName );

	public static FilePath Join( DirectoryPath directoryPath, string relativePath )
	{
		Assert( !IsAbsolute( relativePath ) );
		//string path = SysIoJoin( directoryPath.Path, relativePath );
		//Assert( path == SysIoGetFullPath( path ) );
		//Assert( path.StartsWith2( directoryPath.Path ) );
		//return new FilePath( path );
		string joinedPath = SysIoJoin( directoryPath.Path, relativePath );
		string fullPath = SysIoGetFullPath( joinedPath );
		return new FilePath( fullPath );
	}

	FilePath( string path )
			: base( path )
	{
		Assert( !SysIoEndsInDirectorySeparator( path ) );
	}

	public string Extension => SysIoGetExtension( Path );
	public DirectoryPath GetDirectoryPath() => DirectoryPath.FromAbsolutePath( SysIoGetDirectoryName( Path )! );
	public string GetFileNameAndExtension() => NotNull( SysIoGetFileName( Path ) );
	public string GetFileNameWithoutExtension() => NotNull( SysIoGetFileNameWithoutExtension( Path ) );
	public DirectoryPath Directory => DirectoryPath.FromAbsolutePath( NotNull( SysIoGetParent( Path )! ).FullName );
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
		GetDirectoryPath().CreateIfNotExist();
		retryOnSharingViolation( () => SysIoWriteAllText( Path, text, encoding ?? DotNetHelpers.BomlessUtf8 ) );
	}

	public void WriteAllBytes( byte[] bytes )
	{
		GetDirectoryPath().CreateIfNotExist();
		SysIoWriteAllBytes( Path, bytes );
	}

	public void Truncate()
	{
		GetDirectoryPath().CreateIfNotExist();
		SysIoWriteAllText( Path, "" );
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
				SysThread.Thread.Sleep( 100 );
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

	public DirectoryPath WithoutRelativePath( string relativePath )
	{
		Assert( Path.EndsWith2( relativePath ) );
		return DirectoryPath.FromAbsolutePath( Path[..^relativePath.Length] );
	}

	static SysIo.FileShare getDefaultFileShare( SysIo.FileAccess fileAccess )
	{
		return fileAccess switch
		{
			SysIo.FileAccess.Read => SysIo.FileShare.Read,
			SysIo.FileAccess.Write => SysIo.FileShare.None,
			SysIo.FileAccess.ReadWrite => SysIo.FileShare.None,
			_ => throw new Sys.ArgumentOutOfRangeException( nameof( fileAccess ), fileAccess, null )
		};
	}

	public SysIo.FileStream NewStream( SysIo.FileMode fileMode, SysIo.FileAccess fileAccess, SysIo.FileShare? fileShare = null, int bufferSize = 4096, SysIo.FileOptions fileOptions = SysIo.FileOptions.None, bool createDirectoryIfNotExist = false )
	{
		if( createDirectoryIfNotExist )
			Directory.CreateIfNotExist();
		return SysIoNewFileStream( Path, fileMode, fileAccess, fileShare ?? getDefaultFileShare( fileAccess ), bufferSize, fileOptions );
	}

	public SysIo.TextWriter NewTextWriter( bool createDirectoryIfNotExist = false, SysIo.FileMode fileMode = SysIo.FileMode.Create, SysIo.FileShare? fileShare = null, int fileStreamBufferSize = 4096, int textWriterBufferSize = -1, bool deleteOnClose = false, bool writeThrough = false, SysText.Encoding? encoding = null )
	{
		if( createDirectoryIfNotExist )
			Directory.CreateIfNotExist();
		SysIo.FileOptions fileOptions = SysIo.FileOptions.None;
		if( deleteOnClose )
			fileOptions |= SysIo.FileOptions.DeleteOnClose;
		if( writeThrough )
			fileOptions |= SysIo.FileOptions.WriteThrough;
		SysIo.Stream fileStream = NewStream( fileMode, SysIo.FileAccess.Write, fileShare ?? SysIo.FileShare.ReadWrite, fileStreamBufferSize, fileOptions, createDirectoryIfNotExist: createDirectoryIfNotExist );
		return new SysIo.StreamWriter( fileStream, encoding ?? DotNetHelpers.BomlessUtf8, textWriterBufferSize, leaveOpen: false );
	}

	public SysIo.TextReader NewTextReader( SysIo.FileAccess access = SysIo.FileAccess.Read, SysIo.FileShare? share = null, int fileStreamBufferSize = 4096, int textReaderBufferSize = -1, SysIo.FileOptions fileOptions = SysIo.FileOptions.None, SysText.Encoding? encoding = null )
	{
		SysIo.Stream fileStream = NewStream( SysIo.FileMode.Open, access, share, fileStreamBufferSize, fileOptions, createDirectoryIfNotExist: false );
		return new SysIo.StreamReader( fileStream, encoding, detectEncodingFromByteOrderMarks: true, textReaderBufferSize, leaveOpen: false );
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
