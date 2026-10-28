namespace MikeNakis.Kit;

using Sys = System;

/// An exception thrown for a failed <see cref="Result2{S}"/>. The inner exception of this exception
/// is the exception that initially caused the failure.
public sealed class ResultFailureException : SaneException
{
	/// The message of the inner exception; <see cref="SaneException"/> includes it in <see cref="SaneException.Message"/>,
	/// which would otherwise be empty.
	public string CauseMessage => InnerException!.Message;

	/// Constructor
	public ResultFailureException( Sys.Exception cause )
			: base( cause )
	{ }
}
