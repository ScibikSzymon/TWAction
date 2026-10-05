namespace ActionGenerator.MainAction;

internal static class ListExtensions
{
    internal static List<T> Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Shared.Next(n + 1);
            (list[n], list[k]) = (list[k], list[n]);
        }
        return list;
    }
}
