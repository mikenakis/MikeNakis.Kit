namespace MikeNakis.Kit;

/// An exception to throw in the event of an error.
public sealed class ErrorException : SaneException
{
	public Error Error { get; }

	/// Constructor
	/// [Claude] When the error is an <see cref="ExceptionError"/>, its exception becomes the inner exception, so that its
	/// original stack trace is kept.
	public ErrorException( Error error )
			: base( (error as ExceptionError)?.Exception )
	{
		Error = error;
	}
}
