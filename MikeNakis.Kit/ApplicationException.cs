namespace MikeNakis.Kit;

///<summary>An exception about some problem that can be expected to happen, so we want to terminate with a single error
///message instead of a full diagnostic stack trace.</summary>
[Sys.Serializable]
public class ApplicationException : Sys.Exception
{
	/// Constructor
	public ApplicationException( string message )
		: base( message )
	{ }

	/// Constructor
	public ApplicationException( string message, Sys.Exception cause )
		: base( message, cause )
	{ }

	/// Constructor
	public ApplicationException( Sys.Exception cause )
		: base( cause.Message, cause )
	{ }
}
