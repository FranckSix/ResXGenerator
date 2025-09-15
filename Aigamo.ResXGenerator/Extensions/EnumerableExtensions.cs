namespace Aigamo.ResXGenerator.Extensions;
public static class EnumerableExtensions
{
	public static void ForEach<T>(this IEnumerable<T> col, Action<T> action)
	{
		foreach (var i in col) action(i);
	}

	public static void ForEach<T1, T2, T3>(this IEnumerable<ValueTuple<T1, T2, T3>> col, Action<T1, T2, T3> action)
	{
		foreach (var i in col) action(i.Item1, i.Item2, i.Item3);
	}
}
