namespace MikeNakis.Kit;

/// <summary>An equivalent to the `Unit` class of the Scala language.</summary>
public sealed class Unit
{
	public static readonly Unit Instance = new();

	Unit()
	{ }
}
