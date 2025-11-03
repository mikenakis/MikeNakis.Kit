namespace MikeNakis.Kit;

using SysCompiler = System.Runtime.CompilerServices;

/// Extremely simple, and therefore fast, Linear Congruential Random Number Generator.
/// In a debug build, this class is a bit slower than `System.Random` with a seed.
/// In a release build, this class is slightly faster than `System.Random` with a seed.
/// Furthermore, this class exposes a `State` property, (which `System.Random` does not,) so the seed can be changed
/// without having to re-instantiate the class.
///
/// Based on https://rosettacode.org/wiki/Linear_congruential_generator#C# (BSD flavor) which is actually lame,
/// (see LameBerkeleyRandomNumberGenerator,) so I had to modify it to store the state in a `ulong` and to return
/// the upper 32 bits as the result. This seems to produce much better results.
///
/// The multiplier used to be 1103515245 but I changed it to a much larger prime number found in https://en.wikipedia.org/wiki/List_of_prime_numbers
public sealed class BerkeleyRandomNumberGenerator
{
	public ulong State { get; set; }

	public BerkeleyRandomNumberGenerator( ulong state )
	{
		State = state;
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	uint nextUint()
	{
		State = unchecked(3331113965338635107 * State + 12345);
		return (uint)(State >> 32);
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	double nextDouble()
	{
		uint randomUint = nextUint();
		double result = randomUint / (double)uint.MaxValue; // / (double)uint.MaxValue; // ;
		Assert( result is >= 0.0 and <= 1.0 );
		return result;
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	uint nextUint( uint maxInclusive )
	{
		double randomDouble = nextDouble();
		uint result = (uint)(randomDouble * maxInclusive);
		Assert( result >= 0 && result <= maxInclusive );
		return result;
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	public int Next() => unchecked((int)(nextUint() & int.MaxValue));

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	public double NextDouble() => nextDouble();

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	public int Next( int maxInclusive )
	{
		Assert( maxInclusive >= 0 );
		return (int)nextUint( (uint)maxInclusive );
	}

	[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
	public int Next( int minInclusive, int maxInclusive )
	{
		Assert( minInclusive <= maxInclusive );
		uint range = unchecked((uint)(maxInclusive - minInclusive));
		return unchecked((int)((uint)minInclusive + nextUint( range )));
	}
}
