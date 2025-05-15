namespace MikeNakis.Kit.Extensions;

public static class ListExtensions
{
	// We cannot declare this because it conflicts with IEnumerable.IsEmpty().
	// TODO: remove IEnumerable.IsEmpty(), because we already have .Any(), and enable this extension method.
	// public static bool IsEmpty<T>( this IList<T> self ) => self.Count == 0;
}
