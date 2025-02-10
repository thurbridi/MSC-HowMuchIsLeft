using System;
using System.Collections.Generic;
using MSCLoader;
using UnityEngine;

namespace HowMuchIsLeft
{
    public class HowMuchIsLeft : Mod
    {
        public override string ID => "HowMuchIsLeft"; // Your (unique) mod ID 
        public override string Name => "HowMuchIsLeft"; // Your mod name
        public override string Author => "casper-3"; // Name of the Author (your name)
        public override string Version => "1.2.0"; // Version
        public override string Description => "Displays the contents of some items. See settings to customize."; // Short description of your mod

        GameObject contentDescription;
        TextMesh foregroundText;
        TextMesh shadowText;

        private static SettingsCheckBoxGroup detailsExactSetting;
        private static SettingsCheckBoxGroup detailsRoughSetting;
        private static SettingsCheckBoxGroup detailsEducatedSetting;

        private static SettingsCheckBox alwaysExactCountablesSetting;

        private static string text;

        private readonly int LayerItem = 19;

        private static void GenerateText(double amount, double maxAmount, string name, string namePlural = null, Func<Double, Double> f = null, bool forceExact = false)
        {
            if (forceExact | detailsExactSetting.GetValue()) {
                var fAmount = f != null ? f(amount) : amount;

                text = ExactValueText(fAmount, Pluralize(fAmount, name, namePlural));
                return;
            }

            var amountNormalized = amount / maxAmount;

            if (detailsRoughSetting.GetValue())
            {
                text = RoughGuessText(amountNormalized);
                return;
            }

            if (detailsEducatedSetting.GetValue())
                text = EducatedGuessText(amountNormalized);
        }

        private static void HandleUncountableItem(GameObject item, string fsmVar, float maxAmount, string name, string namePlural = null, Func<double, double> f = null)
        {
            float amount = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat(fsmVar).Value;
            GenerateText(amount, maxAmount, name, namePlural, f);
        }

        private static void HandleCountableItem(GameObject item, string fsmVar, int maxAmount, string name, string namePlural = null, Func<double, double> f = null)
        {
            int amount = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmInt(fsmVar).Value;
            GenerateText(amount, maxAmount, name, namePlural, f, alwaysExactCountablesSetting.GetValue());
        }

        private static void FireExtinguisherHandler(GameObject item, string fsmVar, float maxAmount, string name, string namePlural = null, Func<double, double> f = null)
        {
            PlayMakerFSM[] fsms;
            HutongGames.PlayMaker.FsmFloat amount;
            
            fsms = item.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM fsm in fsms)
            {
                amount = fsm.FsmVariables.FindFsmFloat(fsmVar);
                if (amount != null) {
                    GenerateText(amount.Value, maxAmount, name, namePlural, f);
                    return;
                }
            }
        }

        private static string Pluralize(double amount, string singular, string plural = null)
        {
            if (amount == 1)
                return singular;

            if (plural != null)
                return plural;

            return $"{singular}s";
        }

        private static string ExactValueText(double amount, string name)
        {
            return $"{amount:0.##} {name} remaining";
        }

        private static string RoughGuessText(double value)
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

        private static string EducatedGuessText(double value)
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

        private readonly Dictionary<string, Action<GameObject>> itemHandlerMap = new Dictionary<string, Action<GameObject>>
        {
            { "coolant(itemx)", (item) => HandleUncountableItem(item, "Fluid", 10f, "liter") },
            { "motor oil(itemx)", (item) => HandleUncountableItem(item, "Fluid", 4f, "liter") },
            { "brake fluid(itemx)", (item) => HandleUncountableItem(item, "Fluid", 1f, "liter") },
            { "two stroke fuel(itemx)", (item) => HandleUncountableItem(item, "Fluid", 5f, "liter")},
            { "ground coffee(itemx)", (item) => HandleUncountableItem(item, "Ground", 100f, "gram", f: (x) => x * 5f)},
            { "grill charcoal(itemx)", (item) => HandleUncountableItem(item, "Contents", 140f, "liter", f: (x) => x / 10f)},
            { "spray can(itemx)", (item) => HandleUncountableItem(item, "Fluid", 100f, "unit") },
            { "mosquito spray(itemx)", (item) => HandleUncountableItem(item, "Fluid", 100f, "unit") },

            { "fuse package(Clone)", (item) => HandleCountableItem(item, "Quantity", 5, "fuse") },
            { "r20 battery box(Clone)", (item) => HandleCountableItem(item, "Quantity", 4, "battery", namePlural: "batteries" )},
            { "spark plug box(Clone)", (item) => HandleCountableItem(item, "Quantity", 4, "spark plug") },
            { "teimo advert pile(itemx)", (item) => HandleCountableItem(item, "Sheets", 30, "sheet") },
            
            { "fire extinguisher(itemx)", (item) => FireExtinguisherHandler(item, "Fluid", 100f, "unit") },

        };

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.Update, Mod_Update);
            SetupFunction(Setup.ModSettings, Mod_Settings);
        }

        private void Mod_Settings()
        {
            // All settings should be created here. 
            // DO NOT put anything that isn't settings or keybinds in here!
            Settings.AddHeader("Information detail");
            detailsExactSetting = Settings.AddCheckBoxGroup("detailsExactSetting", "Show exact values", true, "group_content_details");
            detailsRoughSetting = Settings.AddCheckBoxGroup("detailsRoughSetting", "Show rough guess", false, "group_content_details");
            detailsEducatedSetting = Settings.AddCheckBoxGroup("detailsEducatedSetting", "Show educated guess", false, "group_content_details");

            Settings.AddText("Overrides");
            alwaysExactCountablesSetting = Settings.AddCheckBox("alwaysExactCountablesSetting", "Always show exact value for countables", true);
        }

        private void Mod_OnLoad()
        {
            // Called once, when mod is loading after game is fully loaded
            InitializeDescription();
        }

        private void Mod_Update()
        {
            // Update is called once per frame
            text = "";

            RaycastHit raycastHit = UnifiedRaycast.GetRaycastHit();

            if (
                raycastHit.collider == null || 
                raycastHit.transform.gameObject.layer != LayerItem)
            {
                UpdateDescription(text);
                return;
            };

            GameObject item = raycastHit.transform.gameObject;

            if (itemHandlerMap.TryGetValue(item.name, out Action<GameObject> handler))
                handler(item);

            UpdateDescription(text);
        }

        private void InitializeDescription()
        {
            GameObject partName = GameObject.Find("GUI/Indicators/Partname");

            contentDescription = GameObject.Instantiate(partName);

            GameObject.Destroy(contentDescription.GetComponent<PlayMakerFSM>());

            contentDescription.name = "ContentDescription";
            contentDescription.transform.parent = partName.transform.parent;
            contentDescription.transform.localPosition = new Vector3(0.0f, -0.21f, 0.0f);

            foregroundText = contentDescription.GetComponent<TextMesh>();
            shadowText = contentDescription.transform.GetChild(0).GetComponent<TextMesh>();

            foregroundText.characterSize = 0.05f;
            shadowText.characterSize = 0.05f;
        }

        private void UpdateDescription(string text)
        {
            foregroundText.text = text;
            shadowText.text = text;
        }
    }
}
