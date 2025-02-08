using System.Linq;
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

        private const int LayerItem = 19;
        private readonly string[] fluid_items = new string[]
        {
            "coolant(itemx)",
            "motor oil(itemx)",
            "brake fluid(itemx)",
            "two stroke fuel(itemx)",
        };

        public override void ModSetup()
        {
            SetupFunction(Setup.OnLoad, Mod_OnLoad);
            SetupFunction(Setup.OnGUI, Mod_OnGUI);
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
        private void Mod_OnGUI()
        {
            // Draw unity OnGUI() here
        }
        private void Mod_Update()
        {
            // Update is called once per frame
            string text = "";

            RaycastHit raycastHit = UnifiedRaycast.GetRaycastHit();

            if (
                raycastHit.collider == null || 
                raycastHit.transform.gameObject.layer != LayerItem || 
                !fluid_items.Contains(raycastHit.transform.gameObject.name))
            {
                UpdateDescription(text);
                return;
            };

            GameObject item = raycastHit.transform.gameObject;

            float fluid = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmFloat("Fluid").Value;
            text = $"{fluid:0.00} liters remaining";

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
