namespace MikeNakis.Kit.Logging;

using MikeNakis.Kit.Collections;
using MikeNakis.Kit.Extensions;
using MikeNakis.Kit.FileSystem;

public class TextFileLogger : Logger
{
	readonly DirectoryPath logDirectory;
	readonly string prefix;
	readonly Logger textWriterLogger;
	public FilePath LogPathName { get; }

	public TextFileLogger( DirectoryPath logDirectory, string prefix )
	{
		this.logDirectory = logDirectory;
		this.prefix = prefix;
		logDirectory.CreateIfNotExist();
		LogPathName = logDirectory.File( buildLogFileName( prefix, "" ) );
		if( LogPathName.Exists() )
			archive( LogPathName );
		SysIo.TextWriter textWriter = openLogFile( LogPathName );
		textWriterLogger = new TextWriterLogger( textWriter );
	}

	public override void AddLogEntry( LogEntry logEntry ) => textWriterLogger.AddLogEntry( logEntry );

	void archive( FilePath logPathName )
	{
		Sys.DateTime t = logPathName.CreationTimeUtc;
		string timeString = $"{t.Year:D4}-{t.Month:D2}-{t.Day:D2}.{t.Hour:D2}-{t.Minute:D2}-{t.Second:D2}-{t.Millisecond:D3}";
		FilePath archivedLogPathName = logDirectory.File( buildLogFileName( prefix, timeString ) );
		archive( logPathName, archivedLogPathName );
	}

	/*
	PEARL: Beware of one of the most trolling features of Microsoft Windows called "file system tunneling"!

	If you delete a file and within a few milliseconds create a new file with the same name, Windows will (...get a load of this!) RETAIN THE FILE CREATION
	TIME OF THE OLD FILE! 
	
	The same applies if instead of deleting the file you rename it to something else: if you rename file `a` to `b`, and then create a new file `a`, the 
	creation time of `a` will not be the current time as you would expect, it will be the creation time of the old file which is now called `b`.

	Essentially, this means that once file `a` has been created, it will always retain its original creation time, no matter how many times you rename it
	to something else and re-create it.

	This is reported here: https://superuser.com/q/558213/111757 and here: https://stackoverflow.com/q/2109152/773113
	A long article by Raymond Chen discusses it here: https://devblogs.microsoft.com/oldnewthing/20050715-14/?p=34923

	This causes a big problem if you want to archive logs by renaming `application.log` to `application.timestamp.log` and build the 'timestamp' part of
	the filename from the creation time of the file: the renaming will succeed the first time, but it will fail each subsequent time, because an archived
	log with the same name will already exist, BECAUSE THE CREATION TIME DID NOT CHANGE as you would expect.

	Under normal circumstances, the workaround to this would be to manually set the creation timestamp of every new log to "now" before starting to use it.
	However, we cannot do that either, because of the next pearl:

	PEARL-IN-PEARL: Notepad++ monitoring breaks if the file gets momentarily deleted or renamed.

	Notepad has a very useful feature called "monitoring" (tail -f) for viewing a log file as it is being written by another process. 
	
	However, if the file being monitored gets deleted or renamed, Notepad++ will lose track of it, even if a new file with the exact same name gets created
	immediately afterwards. 
	
	So, the next time we switch to Notepad++, it very annoyingly pops up a modal dialog box telling us that the file has been modified and asking us if we
	want to reload it. (Even though it should not, since it is monitoring the file.)

	The solution which works around both of the above issues is to:
	1. Suffer the overhead of copying (instead of renaming) `application.log` to `application.timestamp.log`, so that Windows will not mess with our 
		timestamps.
	2. Truncate `application.log` (essentially sacrificing all that previous overhead of copying the file) so that Notepad++ does not get confused.
	3. Manually set the creation time of `application.log` to the current time.
	4. Then proceed to start appending to it.
	*/
	static void archive( FilePath logPathName, FilePath archivedLogPathName )
	{
		Log.Debug( $"Archiving {logPathName} to {archivedLogPathName}" );
		logPathName.CopyTo( archivedLogPathName, overwrite: true );
		KitHelpers.SwallowException( LogLevel.Warn, "Log file truncation", logPathName.Truncate );
		KitHelpers.SwallowException( LogLevel.Warn, "Log file touch", () => logPathName.SetCreationTimeUtc( DotNetHelpers.GetWallClockTimeUtc() ) );
	}

	static SysIo.TextWriter openLogFile( FilePath filePath )
	{
		for( int attempt = 1; ; attempt++ )
			try
			{
				SysIo.StreamWriter streamWriter = new( filePath.Path, true, DotNetHelpers.BomlessUtf8, bufferSize: 8192 );
				Log.Info( $"Appending to log: {filePath}" ); //this will not appear in the file log, but it will appear in the debug log, which is supposed to always be added, and also possibly in the console log if it has already been added.
				streamWriter.AutoFlush = true;
				return streamWriter;
			}
			catch( SysIo.IOException exception )
			{
				string message = KitHelpers.BuildMediumExceptionMessage( "Log file creation failed with ", exception ).MakeString( "; " );
				Log.Warn( message );
				if( attempt > 10 )
					throw Failure();
				filePath = filePath.Directory.File( filePath.GetFileNameWithoutExtension() + "-" + attempt + filePath.Extension );
			}
	}

	public void DeleteOld( Sys.TimeSpan maxAge, int minToKeep, int maxToKeep, long maxSize )
	{
		Assert( minToKeep <= maxToKeep );
		Sys.DateTime cutOffTime = DotNetHelpers.GetWallClockTimeUtc() - maxAge;
		long totalSize = 0;
		int number = 0;
		IEnumerable<FilePath> sortedFilePaths = getSortedFilePaths( logDirectory, prefix );
		foreach( FilePath filePath in sortedFilePaths )
		{
			number++;
			totalSize += filePath.FileLength;
			if( number > minToKeep && totalSize > maxSize )
				Log.Debug( $"deleting {filePath} because Number:{number} is above MinToKeep:{minToKeep} and TotalSize:{totalSize} is above MaxSize:{maxSize}" );
			else if( number > minToKeep && filePath.LastWriteTimeUtc < cutOffTime )
				Log.Debug( $"deleting {filePath} because Number:{number} is above MinToKeep:{minToKeep} and Time:{filePath.LastWriteTimeUtc} is older than CutOffTime:{cutOffTime}" );
			else if( number > maxToKeep )
				Log.Debug( $"deleting {filePath} because Number:{number} is above MaxToKeep:{maxToKeep}" );
			else
				continue;
			try
			{
				filePath.Delete();
			}
			catch( SysIo.IOException e )
			{
				Log.Warn( $"Log file could not be deleted, {e.Message}" );
			}
		}
	}

	static IEnumerable<FilePath> getSortedFilePaths( DirectoryPath logDirectory, string prefix )
	{
		string pattern = buildLogFileName( prefix, "*" );
		return logDirectory //
				.EnumerateFiles( pattern )
				.Sorted( ( a, b ) => b.LastWriteTimeUtc.CompareTo( a.LastWriteTimeUtc ) )
				.AsEnumerable;
	}

	static string buildLogFileName( string prefix, string middle )
	{
		string[] parts = { prefix, middle };
		string name = parts.Where( s => s != "" ).MakeString( "." );
		return $"{name}.log";
	}
}
