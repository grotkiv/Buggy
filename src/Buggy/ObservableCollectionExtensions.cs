namespace Buggy;

using System;
using System.Collections.ObjectModel;
using System.Linq;

public static class ObservableCollectionExtensions
{
    /// <summary>
    /// Removes all items matching <paramref name="condition"/> from <paramref name="coll"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="coll">The collection to modify.</param>
    /// <param name="condition">The condition to match.</param>
    /// <returns>The number of deleted items.</returns>
    /// <seealso href="https://stackoverflow.com/questions/5118513/removeall-for-observablecollections"/>
    public static int RemoveAll<T>(
        this ObservableCollection<T> coll, Func<T, bool> condition)
    {
        var itemsToRemove = coll.Where(condition).ToList();

        foreach (var itemToRemove in itemsToRemove)
        {
            coll.Remove(itemToRemove);
        }

        return itemsToRemove.Count;
    }
}
