namespace MikeNakis.Kit;

/// An exception to throw in the event of an expectation failure.
public sealed class ExpectationFailureException : SaneException
{
	public Expectation Expectation { get; }

	/// Constructor
	public ExpectationFailureException( Expectation expectation )
	{
		Expectation = expectation;
	}
}
