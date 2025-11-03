namespace MikeNakis.Kit;

using Sys = System;

public sealed class DummyDisposable : Sys.IDisposable
{
	public static readonly DummyDisposable Instance = new();

	DummyDisposable()
	{ }

	public void Dispose()
	{ }
}
