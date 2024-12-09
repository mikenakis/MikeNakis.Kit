namespace MikeNakis.Kit.TextLines;

using SysIo = System.IO;

public sealed class TextLineWriterOnTextWriter : TextLineWriter
{
	readonly SysIo.TextWriter textWriter;

	public TextLineWriterOnTextWriter( SysIo.TextWriter textWriter )
	{
		this.textWriter = textWriter;
	}

	public TextLineWriter TextLineWriter => this;

	void TextLineWriter.WriteLine( string text )
	{
		textWriter.WriteLine( text );
	}
}
