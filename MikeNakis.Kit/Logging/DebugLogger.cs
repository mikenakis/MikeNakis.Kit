namespace MikeNakis.Kit.Logging;

public static class DebugLogger
{
	public static readonly Logger Instance = new TextWriterLogger( new DebugTextWriter() );
}
