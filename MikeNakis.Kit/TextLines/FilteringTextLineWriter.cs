namespace MikeNakis.Kit.TextLines;

using MikeNakis.Kit.Collections;
using RegEx = System.Text.RegularExpressions;

public sealed class FilteringTextLineWriter : TextLineWriter
{
	readonly TextLineWriter textLineWriter;
	readonly IReadOnlyCollection<RegEx.Regex> regularExpressions;

	public FilteringTextLineWriter( TextLineWriter textLineWriter, IEnumerable<string> exclusionPatterns )
	{
		this.textLineWriter = textLineWriter;
		regularExpressions = exclusionPatterns.Select( e => new RegEx.Regex( e, RegEx.RegexOptions.CultureInvariant | RegEx.RegexOptions.Compiled ) ).Collect();
	}

	public TextLineWriter TextLineWriter => this;

	void TextLineWriter.WriteLine( string text )
	{
		foreach( RegEx.Regex regex in regularExpressions )
			if( regex.IsMatch( text ) )
				return;
		textLineWriter.WriteLine( text );
	}
}
