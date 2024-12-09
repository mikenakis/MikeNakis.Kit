namespace MikeNakis.Kit.TextLines;

using SysIo = System.IO;
using SysText = System.Text;

public class TextWriterOnTextLineWriter : SysIo.TextWriter
{
	readonly TextLineWriter textLineWriter;
	readonly SysText.StringBuilder stringBuilder = new();

	public TextWriterOnTextLineWriter( TextLineWriter textLineWriter ) => this.textLineWriter = textLineWriter;

	void write( char c )
	{
		if( c == '\r' )
			return;
		if( c == '\n' )
		{
			flush();
			return;
		}
		stringBuilder.Append( c );
	}

	public override void Write( char c )
	{
		write( c );
	}

	public override void Write( char[] buffer, int index, int count )
	{
		for( int i = index; i < count; i++ )
			write( buffer[i] );
	}

	void flush()
	{
		textLineWriter.WriteLine( stringBuilder.ToString() );
		stringBuilder.Clear();
	}

	public override SysText.Encoding Encoding => SysText.Encoding.UTF8;
}
