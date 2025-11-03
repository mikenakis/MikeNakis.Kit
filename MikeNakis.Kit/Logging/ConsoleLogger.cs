namespace MikeNakis.Kit.Logging;

using Sys = System;

public static class ConsoleLogger
{
	public static readonly Logger Instance = new TextWriterLogger( Sys.Console.Out );
}
