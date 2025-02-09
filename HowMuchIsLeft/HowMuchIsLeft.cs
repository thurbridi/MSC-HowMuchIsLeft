using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MSCLoader;
using UnityEngine;

namespace HowMuchIsLeft
{
    public class HowMuchIsLeft : Mod
    {
        public override string ID => "HowMuchIsLeft"; // Your (unique) mod ID 
        public override string Name => "HowMuchIsLeft"; // Your mod name
        public override string Author => "casper-3"; // Name of the Author (your name)
        public override string Version => "1.1.0"; // Version
        public override string Description => "Displays the contents of some items. See settings to customize."; // Short description of your mod

        GameObject contentDescription;
        TextMesh foregroundText;
        TextMesh shadowText;

        private static SettingsCheckBoxGroup detailsExactSetting;
        private static SettingsCheckBoxGroup detailsRoughSetting;
        private static SettingsCheckBoxGroup detailsEducatedSetting;

        private static string text;

        private readonly int LayerItem = 19;


        private static void GenerateText(GameObject item, string fsmVar, float maxAmount, string unit, Func<float, float> converter)
        {
            float amount = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat(fsmVar).Value;
            
            if (detailsExactSetting.GetValue()) { 
                text = ExactValueText(converter(amount), unit);
                return;
            }

            if (detailsRoughSetting.GetValue())
                text = RoughGuessText(amount / maxAmount);

            if (detailsEducatedSetting.GetValue())
                text = EducatedGuessText(amount / maxAmount);
        }

        private static string ExactValueText(float amount, string unit)
        {
            return $"{amount:0.##} {unit}{(amount == 1f? "" : "s")} remaining";
        }
        
        private static string RoughGuessText(float value)
        {
            if (value == 1f)
                return "it's full";
            else if (value > .75f)
                return "it's almost full";
            else if (value > .25f)
                return "about half remaining";
            else
                return "there's still some left";
        }

        private static string EducatedGuessText(float value)
        {
            if (value == 1f)
                return "it's full";
            else if (value > .875f)
                return "nearly full";
            else if (value > .75f)
                return "more than 3/4 left";
            else if (value > .625f)
                return "less than 3/4 left";
            else if (value >= 0.375f)
                return "about half remaining";
            else if (value > .25f)
                return "more than 1/4 left";
            else if (value > .125f)
                return "less than 1/4 left";
            else
                return "it's almost empty";
        }

        private readonly Dictionary<string, Action<GameObject>> itemHandlerMap = new Dictionary<string, Action<GameObject>>
        {
            { "coolant(itemx)", (item) => GenerateText(item, "Fluid", 10f, "liter", (x) => x) },
            { "motor oil(itemx)", (item) => GenerateText(item, "Fluid", 4f, "liter", (x) => x) },
            { "brake fluid(itemx)", (item) => GenerateText(item, "Fluid", 1f, "liter", (x) => x)},
            { "two stroke fuel(itemx)", (item) => GenerateText(item, "Fluid", 5f, "liter", (x) => x)},
            { "ground coffee(itemx)", (item) => GenerateText(item, "Ground", 100f, "gram", (x) => x * 5f)},
            { "grill charcoal(itemx)", (item) => GenerateText(item, "Contents", 140f, "liter", (x) => x / 10f)},
            { "spray can(itemx)", (item) => GenerateText(item, "Fluid", 100f, "unit", (x) => x)},
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
