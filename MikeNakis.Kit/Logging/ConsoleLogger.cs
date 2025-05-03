namespace MikeNakis.Kit.Logging;

public static class ConsoleLogger
{
	public static readonly Logger Instance = new TextWriterLogger( Sys.Console.Out );
}
