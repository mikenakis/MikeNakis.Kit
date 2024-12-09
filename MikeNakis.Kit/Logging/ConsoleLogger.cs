namespace MikeNakis.Kit.Logging;

using Sys = System;

public static class ConsoleLogger
{
#pragma warning disable CA2211 // Non-constant fields should not be visible
	public static Logger Instance = new TextWriterLogger( Sys.Console.Out );
#pragma warning restore CA2211 // Non-constant fields should not be visible
}
