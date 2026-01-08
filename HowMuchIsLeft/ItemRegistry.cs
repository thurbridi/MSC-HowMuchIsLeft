using System.Collections.Generic;
using System;
using UnityEngine;

namespace HowMuchIsLeft
{
    internal static class ItemRegistry
    {
        private static Dictionary<string, Action<GameObject>> itemHandlers = new Dictionary<string, Action<GameObject>>();
        internal static void RegisterItem(string itemName, Action<GameObject> handler)
        {
            itemHandlers[itemName] = handler;
        }

        internal static bool TryGetItemHandler(string itemName, out Action<GameObject> handler)
        {
            return itemHandlers.TryGetValue(itemName, out handler);
        }
    }
}
