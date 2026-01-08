using System;
using UnityEngine;

namespace HowMuchIsLeft
{
    /// <summary>
    /// Public API for registering items and generating content descriptions for the HowMuchIsLeft mod.
    /// </summary>
    public static class HowMuchIsLeftAPI
    {
        /// <summary>
        /// Registers an item and its handler for content description.
        /// </summary>
        /// <param name="itemName">The unique name of the item to register.</param>
        /// <param name="handler">
        /// A delegate that handles the item. The handler should call <see cref="GenerateText"/> to set the description.
        /// </param>
        public static void RegisterItem(string itemName, Action<GameObject> handler)
        {
            ItemRegistry.RegisterItem(itemName, handler);
        }

        /// <summary>
        /// Generates a content description text for an item based on its amount and user settings.
        /// </summary>
        /// <param name="amount">The current amount of the item.</param>
        /// <param name="maxAmount">The maximum possible amount of the item.</param>
        /// <param name="name">The singular name of the item, or the name of the unit (e.g., "liter", "battery").</param>
        /// <param name="isCountable">Indicates if the item is countable (true) or uncountable (false).</param>
        /// <param name="namePlural">The plural name of the item (optional). Used for special plural forms (e.g., 1 tooth, 2 teeth)</param>
        /// <param name="f">An optional function to transform the amount before generating the text. Useful if you want to match the amount from the game's logic with what is shown on the packaging for added realism.</param>
        public static void GenerateText(double amount, double maxAmount, string name, bool isCountable, string namePlural = null, Func<Double, Double> f = null)
        {
            HowMuchIsLeft.GenerateText(amount, maxAmount, name, isCountable, namePlural, f);
        }
    }
}
