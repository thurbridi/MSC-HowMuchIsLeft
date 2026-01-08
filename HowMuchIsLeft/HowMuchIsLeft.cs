using System;
using MSCLoader;
using UnityEngine;

namespace HowMuchIsLeft
{
    public class HowMuchIsLeft : Mod
    {
        public override string ID => "HowMuchIsLeft"; // Your (unique) mod ID 
        public override string Name => "HowMuchIsLeft"; // Your mod name
        public override string Author => "casper-3"; // Name of the Author (your name)
        public override string Version => "1.3.0"; // Version
        public override string Description => "Displays the contents of some items. See settings to customize."; // Short description of your mod
        public override Game SupportedGames => Game.MySummerCar | Game.MyWinterCar;

        
        static SettingsCheckBoxGroup detailsExactSetting;
        static SettingsCheckBoxGroup detailsRoughSetting;
        static SettingsCheckBoxGroup detailsEducatedSetting;

        static SettingsCheckBox alwaysExactCountablesSetting;

        static string text;
        static RaycastHit raycastHit;

        private readonly int layerItem = 19;

        internal static void GenerateText(double amount, double maxAmount, string name, bool isCountable, string namePlural = null, Func<Double, Double> f = null)
        {
            bool forceExact = isCountable & alwaysExactCountablesSetting.GetValue();
            if (forceExact | detailsExactSetting.GetValue()) {
                var fAmount = f != null ? f(amount) : amount;

                text = Utils.ExactValueText(fAmount, Utils.Pluralize(fAmount, name, namePlural));
                return;
            }

            var amountNormalized = amount / maxAmount;

            if (detailsRoughSetting.GetValue())
                text = Utils.RoughGuessText(amountNormalized);

            if (detailsEducatedSetting.GetValue())
                text = Utils.EducatedGuessText(amountNormalized);
        }

        private static void HandleFireExtinguisher(GameObject item, string fsmVar, float maxAmount, string name, string namePlural = null, Func<double, double> f = null)
        {
            PlayMakerFSM[] fsms;
            HutongGames.PlayMaker.FsmFloat amount;
            
            fsms = item.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM fsm in fsms)
            {
                amount = fsm.FsmVariables.FindFsmFloat(fsmVar);
                if (amount != null) {
                    GenerateText(amount.Value, maxAmount, name, isCountable: false, namePlural, f);
                    return;
                }
            }
        }

        private void RegisterCommonItems()
        {
            ItemRegistry.RegisterItem("coolant(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 10f, "liter"));
            ItemRegistry.RegisterItem("motor oil(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 4f, "liter"));
            ItemRegistry.RegisterItem("brake fluid(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 1f, "liter"));
            ItemRegistry.RegisterItem("two stroke fuel(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 5f, "liter"));
            ItemRegistry.RegisterItem("ground coffee(itemx)", (item) => Utils.HandleUncountableItem(item, "Ground", 100f, "gram", f: (x) => x * 5f));
            ItemRegistry.RegisterItem("grill charcoal(itemx)", (item) => Utils.HandleUncountableItem(item, "Contents", 140f, "liter", f: (x) => x / 10f));
            ItemRegistry.RegisterItem("spray can(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 100f, "unit"));
            ItemRegistry.RegisterItem("mosquito spray(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 100f, "unit"));

            ItemRegistry.RegisterItem("fuse package(Clone)", (item) => Utils.HandleCountableItem(item, "Quantity", 5, "fuse"));
            ItemRegistry.RegisterItem("r20 battery box(Clone)", (item) => Utils.HandleCountableItem(item, "Quantity", 4, "battery", namePlural: "batteries"));
            ItemRegistry.RegisterItem("spark plug box(Clone)", (item) => Utils.HandleCountableItem(item, "Quantity", 4, "spark plug"));

            ItemRegistry.RegisterItem("fire extinguisher(itemx)", (item) => HandleFireExtinguisher(item, "Fluid", 100f, "unit"));
        }

        private void RegisterMySummerCarItems()
        {
            RegisterCommonItems();
            ItemRegistry.RegisterItem("teimo advert pile(itemx)", (item) => Utils.HandleCountableItem(item, "Sheets", 30, "sheet"));
        }

        private void RegisterMyWinterCarItems()
        {
            RegisterCommonItems();
            ItemRegistry.RegisterItem("automatic transmission fluid(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 1f, "liter"));

            ItemRegistry.RegisterItem("advert pile(itemx)", (item) => Utils.HandleCountableItem(item, "Sheets", 30, "sheet"));
            ItemRegistry.RegisterItem("chargers box(Clone)", (item) => Utils.HandleCountableItem(item, "Items", 40, "charger"));
            ItemRegistry.RegisterItem("packaging sheets(Clone)", (item) => Utils.HandleCountableItem(item, "Items", 20, "sheet"));
            ItemRegistry.RegisterItem("manuals box(Clone)", (item) => Utils.HandleCountableItem(item, "Items", 80, "manual"));
            ItemRegistry.RegisterItem("plastic trays(Clone)", (item) => Utils.HandleCountableItem(item, "Items", 20, "tray"));
            // TODO: Packages box, look into the different logic 
        }

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
            // Called once, when mod is loading after game is fully 
            if (ModLoader.CurrentGame == Game.MySummerCar)
                RegisterMySummerCarItems();
            else if (ModLoader.CurrentGame == Game.MyWinterCar)
                RegisterMyWinterCarItems();
        }

        private void Mod_Update()
        {
            text = "";
            raycastHit = UnifiedRaycast.GetRaycastHit();

            if (
                raycastHit.collider == null || 
                raycastHit.transform.gameObject.layer != layerItem)
            {
                ItemContentDescription.SetText(text);
                return;
            };

            GameObject item = raycastHit.transform.gameObject;

            if (ItemRegistry.TryGetItemHandler(item.name, out Action<GameObject> handler))
                handler(item);

            ItemContentDescription.SetText(text);
        }
    }
}
