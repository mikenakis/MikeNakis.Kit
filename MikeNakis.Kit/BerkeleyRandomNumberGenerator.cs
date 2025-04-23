namespace MikeNakis.Kit;

using SysCompiler = System.Runtime.CompilerServices;

/// Extremely simple, and therefore fast, Linear Congruential Random Number Generator.
/// In a debug build, this class is a bit slower than `System.Random` with a seed.
/// In a release build, this class is slightly faster than `System.Random` with a seed.
/// Furthermore, this class exposes a `SetSeed()` method, (which `System.Random` does not,) so the seed can be changed
/// without having to re-instantiate the class.
///
/// Based on https://rosettacode.org/wiki/Linear_congruential_generator#C# (BSD flavor) which is actually lame,
/// (see LameBerkeleyRandomNumberGenerator,) so I had to modify it to store the internal seed in a `ulong` and to return
/// the top 32 bits of the seed. This seems to produce very good results.
public sealed class BerkeleyRandomNumberGenerator
{
	ulong state;

	public BerkeleyRandomNumberGenerator( int seed )
	{
		SetSeed( seed );
	}

	public void SetSeed( int seed ) => state = unchecked((ulong)seed);

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	uint nextUint()
	{
		state = unchecked(1103515245 * state + 12345);
		return (uint)(state >> 32);
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	double nextDouble() => nextUint() * (1.0 / uint.MaxValue);

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	uint nextUint( uint maxInclusive ) => (uint)(nextDouble() * maxInclusive);

	public int Next() => unchecked((int)(nextUint() & int.MaxValue));

	public double NextDouble() => nextDouble();

	public int Next( int maxInclusive )
	{
		Assert( maxInclusive >= 0 );
		return (int)nextUint( (uint)maxInclusive );
	}

	public int Next( int minInclusive, int maxInclusive )
	{
		Assert( minInclusive <= maxInclusive );
		return unchecked((int)((uint)minInclusive + nextUint( unchecked((uint)(maxInclusive - minInclusive)) )));
	}
}
