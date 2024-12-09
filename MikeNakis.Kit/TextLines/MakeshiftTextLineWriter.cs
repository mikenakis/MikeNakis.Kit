namespace MikeNakis.Kit.TextLines;

public sealed class MakeshiftTextLineWriter : TextLineWriter
{
	readonly System.Action<string> lineConsumer;

	public MakeshiftTextLineWriter( System.Action<string> lineConsumer )
	{
		this.lineConsumer = lineConsumer;
	}

	public void WriteLine( string text )
	{
		lineConsumer.Invoke( text );
	}
}
