namespace MikeNakis.Kit;

using static MikeNakis.Kit.GlobalStatics;
using Sys = System;

///<summary>Invokes a certain <see cref="Sys.Action"/> when disposed.</summary>
public sealed class MakeshiftDisposable : Sys.IDisposable
{
	readonly LifeGuard lifeGuard = LifeGuard.Create();
	public override string ToString() => $"{Id( this )} {lifeGuard.Status}";
	readonly Sys.Action procedure;

	public MakeshiftDisposable( Sys.Action procedure )
	{
		this.procedure = procedure;
	}

	public void Dispose()
	{
		Assert( lifeGuard.IsAliveAssertion() );
		procedure.Invoke();
		lifeGuard.Dispose();
	}
}
