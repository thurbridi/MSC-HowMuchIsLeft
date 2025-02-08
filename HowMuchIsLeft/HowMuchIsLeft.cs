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
        public override string Version => "1.0"; // Version
        public override string Description => ""; // Short description of your mod

        GameObject contentDescription;
        TextMesh foregroundText;
        TextMesh shadowText;

        static string text;

        private readonly int LayerItem = 19;

        static void HandleFluidItem(GameObject item)
        {
            float fluid = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Fluid").Value;
            text = $"{fluid:0.00} liters remaining";
        }

        static void HandleCoffeeItem(GameObject item)
        {
            float content = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Ground").Value;
            content = content * 5f; // product label says 500g
            text = $"{content:0.} grams remaining";
        }

        static void HandleCharcoalItem(GameObject item)
        {
            float content = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Contents").Value;
            content = content / 10f; // product label says 14L or 1.7Kg
            text = $"{content:0.00} liters remaining";
        }

        static readonly Dictionary<string, Action<GameObject>> itemHandlersMap = new Dictionary<string, Action<GameObject>>
        {
            { "coolant(itemx)", HandleFluidItem },
            { "motor oil(itemx)", HandleFluidItem },
            { "brake fluid(itemx)", HandleFluidItem },
            { "two stroke fuel(itemx)", HandleFluidItem },
            { "ground coffee(itemx)", HandleCoffeeItem },
            { "grill charcoal(itemx)", HandleCharcoalItem },
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

            if (itemHandlersMap.TryGetValue(item.name, out Action<GameObject> handler))
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
