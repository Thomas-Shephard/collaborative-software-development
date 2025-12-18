using System.Collections.ObjectModel;

namespace Jahoot.Display.Extensions;

public static class ObservableCollectionExtensions
{
    public static void UpdateCollection<T>(this ObservableCollection<T> collection, IEnumerable<T> newItems)
    {
        collection.Clear();
        foreach (var item in newItems)
        {
            collection.Add(item);
        }
    }
}
