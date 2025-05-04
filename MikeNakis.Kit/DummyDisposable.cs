namespace MikeNakis.Kit;

public sealed class DummyDisposable : Sys.IDisposable
{
	public static readonly DummyDisposable Instance = new();

	DummyDisposable()
	{ }

	public void Dispose()
	{ }
}
