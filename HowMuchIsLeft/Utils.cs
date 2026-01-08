using System;
using UnityEngine;

namespace HowMuchIsLeft
{
    /// <summary>
    /// Provides utility methods for item handling and content description generation.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Returns the singular or plural form of a name based on the amount.
        /// </summary>
        /// <param name="amount">The amount to evaluate for pluralization.</param>
        /// <param name="singular">The singular form of the name.</param>
        /// <param name="plural">The plural form of the name (optional). If not provided, 's' is appended to the singular form.</param>
        /// <returns>The appropriate singular or plural form based on the amount.</returns>
        public static string Pluralize(double amount, string singular, string plural = null)
        {
            if (amount == 1)
                return singular;

            if (plural != null)
                return plural;

            return $"{singular}s";
        }

        /// <summary>
        /// Handles uncountable items by extracting their amount and generating a content description.
        /// </summary>
        /// <param name="item">The GameObject representing the item.</param>
        /// <param name="fsmVar">The name of the FSM float variable representing the amount.</param>
        /// <param name="maxAmount">The maximum possible amount of the item.</param>
        /// <param name="name">The singular name of the item or unit.</param>
        /// <param name="namePlural">The plural name of the item or unit (optional).</param>
        /// <param name="f">An optional function to transform the amount before generating the text.</param>
        public static void HandleUncountableItem(GameObject item, string fsmVar, float maxAmount, string name, string namePlural = null, Func<double, double> f = null)
        {
            float amount = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat(fsmVar).Value;
            HowMuchIsLeft.GenerateText(amount, maxAmount, name, isCountable: false, namePlural, f);
        }

        /// <summary>
        /// Handles countable items by extracting their quantity and generating a content description.
        /// </summary>
        /// <param name="item">The GameObject representing the item.</param>
        /// <param name="fsmVar">The name of the FSM int variable representing the quantity.</param>
        /// <param name="maxAmount">The maximum possible quantity of the item.</param>
        /// <param name="name">The singular name of the item or unit.</param>
        /// <param name="namePlural">The plural name of the item or unit (optional).</param>
        /// <param name="f">An optional function to transform the amount before generating the text.</param>
        public static void HandleCountableItem(GameObject item, string fsmVar, int maxAmount, string name, string namePlural = null, Func<double, double> f = null)
        {
            int amount = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmInt(fsmVar).Value;
            HowMuchIsLeft.GenerateText(amount, maxAmount, name, isCountable: true, namePlural, f);
        }

        internal static string ExactValueText(double amount, string name)
        {
            return $"{amount:0.##} {name} remaining";
        }

        internal static string RoughGuessText(double value)
        {
            if (value == 1.0)
                return "it's full";
            else if (value > .75)
                return "it's almost full";
            else if (value > .25)
                return "about half remaining";
            else
                return "there's still some left";
        }

        internal static string EducatedGuessText(double value)
        {
            if (value == 1.0)
                return "it's full";
            else if (value > .875)
                return "nearly full";
            else if (value > .75)
                return "more than 3/4 left";
            else if (value > .625)
                return "less than 3/4 left";
            else if (value >= 0.375)
                return "about half remaining";
            else if (value > .25)
                return "more than 1/4 left";
            else if (value > .125)
                return "less than 1/4 left";
            else
                return "it's almost empty";
        }
    }
}
