namespace MikeNakis.Kit.Events;

using Sys = System;
using SysDiag = System.Diagnostics;

[SysDiag.DebuggerDisplay( "{ToString(),nq}" )]
public abstract class Subscription : Sys.IDisposable
{
	readonly LifeGuard lifeGuard = LifeGuard.Create();
	public override string ToString() => lifeGuard.ToStringFor( this );

	public virtual void Dispose()
	{
		lifeGuard.Dispose();
	}
}
