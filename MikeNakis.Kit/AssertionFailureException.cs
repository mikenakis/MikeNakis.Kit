namespace MikeNakis.Kit;

using Sys = System;

/// An exception to throw in the event of an assertion failure.
public sealed class AssertionFailureException : Sys.Exception
{
	/// Constructor
	public AssertionFailureException()
	{ }

	/// Constructor
	public AssertionFailureException( string message )
			: base( message )
	{ }

	/// Constructor
	public AssertionFailureException( string message, Sys.Exception? cause )
			: base( message, cause )
	{ }

	/// Constructor
	public AssertionFailureException( Sys.Exception? cause )
			: base( null, cause )
	{ }
}
