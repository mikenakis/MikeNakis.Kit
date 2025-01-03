namespace MikeNakis.Kit;

using System;

/// An exception to throw in the event of an assertion failure.
public sealed class AssertionFailureException : SaneException
{
	public override string Message { get; }

	/// Constructor
	public AssertionFailureException()
	{
		Message = "";
	}

	/// Constructor
	public AssertionFailureException( string message )
	{
		Message = message;
	}

	/// Constructor
	public AssertionFailureException( string message, Exception? cause )
			: base( cause )
	{
		Message = message;
	}
}
