namespace MikeNakis.Kit.Logging;

using System.Collections.Generic;
using SysIo = System.IO;
using SysText = System.Text;
using SysThreading = System.Threading;

public class TextWriterLogger : Logger
{
	readonly SysIo.TextWriter textWriter;
	int longestFirstPartLength;

	public TextWriterLogger( SysIo.TextWriter textWriter )
	{
		this.textWriter = textWriter;
	}

	public override void AddLogEntry( LogEntry logEntry )
	{
		IReadOnlyList<string> parts = logEntry.ToStrings();
		SysText.StringBuilder stringBuilder = new();
		for( int i = 0; i < parts.Count; i++ )
		{
			stringBuilder.Append( parts[i] );
			if( i == 0 )
			{
				while( parts[i].Length > longestFirstPartLength )
					SysThreading.Interlocked.Increment( ref longestFirstPartLength );
				stringBuilder.Append( new string( ' ', longestFirstPartLength - parts[i].Length ) );
			}
		}
		string text = stringBuilder.ToString();
		textWriter.WriteLine( text );
	}
}
