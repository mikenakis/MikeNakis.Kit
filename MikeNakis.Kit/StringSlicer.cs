namespace MikeNakis.Kit;

using static MikeNakis.Kit.GlobalStatics;
using SysCompiler = System.Runtime.CompilerServices;
using SysDiag = System.Diagnostics;

public class StringSlicer : Traversable<(int start, int end)>
{
	readonly string source;
	readonly int startOffset;
	readonly int endOffset;
	readonly char separator;

	internal StringSlicer( string source, int startOffset, int endOffset, char separator )
	{
		Assert( startOffset >= 0 );
		Assert( endOffset >= 0 );
		Assert( endOffset <= source.Length );
		this.source = source;
		this.startOffset = startOffset;
		this.endOffset = endOffset;
		this.separator = separator;
	}

	public Traverser GetTraverser() => new( this );
	Traverser<(int start, int end)> Traversable<(int start, int end)>.GetTraverser() => GetTraverser();

	[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
	public class Traverser : Traverser<(int start, int end)>
	{
		readonly StringSlicer stringSlicer;
		int start;
		int end;

		internal Traverser( StringSlicer stringSlicer )
		{
			this.stringSlicer = stringSlicer;
			start = stringSlicer.startOffset;
			next();
		}

		public bool HasCurrent => end <= stringSlicer.endOffset;

		[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
		public void MoveNext()
		{
			Assert( HasCurrent );
			start = end + 1;
			next();
		}

		[SysCompiler.MethodImpl( SysCompiler.MethodImplOptions.AggressiveInlining )]
		void next()
		{
			for( end = start; end < stringSlicer.endOffset; end++ )
				if( stringSlicer.source[end] == stringSlicer.separator )
					break;
		}

		public (int start, int end) Current
		{
			get
			{
				Assert( HasCurrent );
				return (start, end);
			}
		}

		public override string ToString() => KitHelpers.SafeToString( stringSlicer.source[start..end] );
		public void Dispose() { }

		public bool IsLastAssertion()
		{
			MoveNext();
			Assert( !HasCurrent );
			return true;
		}
	}
}
