namespace MikeNakis.Kit.TextLines;

using System.Text;

public sealed class TextLineWriterOnStringBuilder : TextLineWriter
{
	public StringBuilder StringBuilder { get; }
	public string EndOfLine { get; }

	public TextLineWriterOnStringBuilder( StringBuilder stringBuilder, string endOfLine = "\n" )
	{
		StringBuilder = stringBuilder;
		EndOfLine = endOfLine;
	}

	public TextLineWriter TextLineWriter => this;

	void TextLineWriter.WriteLine( string text )
	{
		StringBuilder.Append( text ).Append( EndOfLine );
	}
}
