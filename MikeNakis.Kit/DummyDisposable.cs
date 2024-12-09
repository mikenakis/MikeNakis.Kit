namespace MikeNakis.Kit;

using System;

public sealed class DummyDisposable : IDisposable
{
	public static readonly DummyDisposable Instance = new();

	DummyDisposable()
	{ }

	public void Dispose()
	{ }
}
