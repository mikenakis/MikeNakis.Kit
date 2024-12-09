namespace MikeNakis.Kit;

//NOTE: this exception class exists for convenience, but ideally, it should not be used.
//      Try to use a more specific exception class instead.
public sealed class GenericException : SaneException
{
	public override string Message { get; }

	public GenericException( string message )
	{
		Message = message;
	}
}
