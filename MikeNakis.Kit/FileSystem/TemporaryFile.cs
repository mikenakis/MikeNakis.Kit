namespace MikeNakis.Kit.FileSystem;

using static MikeNakis.Kit.GlobalStatics;

public class TemporaryFile : Sys.IDisposable
{
	readonly LifeGuard lifeGuard = LifeGuard.Create();
	readonly FilePath filePath;
	public FilePath FilePath => getFilePath();

	public TemporaryFile( string extension )
	{
		filePath = DotNetHelpers.GetApplicationTempDirectoryPath().GenerateUniqueFilePath( extension );
	}

	public void Dispose()
	{
		Assert( lifeGuard.IsAliveAssertion() );
		lifeGuard.Dispose();
		filePath.Delete();
	}

	FilePath getFilePath()
	{
		Assert( lifeGuard.IsAliveAssertion() );
		return filePath;
	}
}
