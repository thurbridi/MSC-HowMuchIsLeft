using System.Collections.Generic;
using System;
using UnityEngine;

namespace HowMuchIsLeft
{
    namespace API
    {
        public static class ItemRegistry
        {
            private static Dictionary<string, Action<GameObject>> itemHandlers = new Dictionary<string, Action<GameObject>>();
            public static void RegisterItem(string itemName, Action<GameObject> handler)
            {
                itemHandlers[itemName] = handler;
            }

            public static bool TryGetItemHandler(string itemName, out Action<GameObject> handler)
            {
                return itemHandlers.TryGetValue(itemName, out handler);
            }
        }
    }
}
