using System;
using HutongGames.PlayMaker;
using MSCLoader;
using UnityEngine;

namespace HowMuchIsLeft
{
    public class HowMuchIsLeft : Mod
    {
        public override string ID => "HowMuchIsLeft"; // Your (unique) mod ID 
        public override string Name => "HowMuchIsLeft"; // Your mod name
        public override string Author => "casper-3"; // Name of the Author (your name)
        public override string Version => "1.3.2"; // Version
        public override string Description => "Displays the contents of some items. See settings to customize."; // Short description of your mod
        public override Game SupportedGames => Game.MySummerCar | Game.MyWinterCar;


        private static SettingsCheckBoxGroup detailsExactSetting, detailsRoughSetting, detailsEducatedSetting;
        private static SettingsDropDownList itemUnitsSetting;
        private static SettingsCheckBox alwaysExactCountablesSetting;
        private static string text;

        private ItemContentDescription itemContentDescription;
        private RaycastHit raycastHit;

        private readonly int layerItem = 19;

        internal static void GenerateText(double amount, double maxAmount, string name, bool isCountable, string namePlural = null, Func<Double, Double> f = null)
        {
            var fAmount = f != null ? f(amount) : amount;
            var amountNormalized = amount / maxAmount;

            if (detailsExactSetting.GetValue())
            {
                if (!isCountable && itemUnitsSetting.GetSelectedItemName() == "Percentage")
                    text = Utils.ExactValueText(amountNormalized * 100, "%");
                else
                    text = Utils.ExactValueText(fAmount, Utils.Pluralize(fAmount, name, namePlural));
                return;
            }

            bool forceExact = isCountable && alwaysExactCountablesSetting.GetValue();
            if (detailsRoughSetting.GetValue())
            {
                if (forceExact)
                {
                    text = Utils.ExactValueText(fAmount, Utils.Pluralize(fAmount, name, namePlural));
                    return;
                }
                text = Utils.RoughGuessText(amountNormalized);
                return;
            }
            if (detailsEducatedSetting.GetValue())
            {
                if (forceExact)
                {
                    text = Utils.ExactValueText(fAmount, Utils.Pluralize(fAmount, name, namePlural));
                    return;
                }
                text = Utils.EducatedGuessText(amountNormalized);
                return;
            }
        }

        private void HandleFireExtinguisher(GameObject item, string fsmVar, float maxAmount, string name, string namePlural = null, Func<double, double> f = null)
        {
            PlayMakerFSM[] fsms;
            HutongGames.PlayMaker.FsmFloat amount;

            fsms = item.GetComponents<PlayMakerFSM>();
            foreach (PlayMakerFSM fsm in fsms)
            {
                amount = fsm.FsmVariables.FindFsmFloat(fsmVar);
                if (amount != null)
                {
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
            ItemRegistry.RegisterItem("spray can(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 100f, "%", namePlural: "%"));
            ItemRegistry.RegisterItem("mosquito spray(itemx)", (item) => Utils.HandleUncountableItem(item, "Fluid", 100f, "%", namePlural: "%"));

            ItemRegistry.RegisterItem("fuse package(Clone)", (item) => Utils.HandleCountableItem(item, "Quantity", 5, "fuse"));
            ItemRegistry.RegisterItem("r20 battery box(Clone)", (item) => Utils.HandleCountableItem(item, "Quantity", 4, "battery", namePlural: "batteries"));
            ItemRegistry.RegisterItem("spark plug box(Clone)", (item) => Utils.HandleCountableItem(item, "Quantity", 4, "spark plug"));
        }

        private void RegisterMySummerCarItems()
        {
            RegisterCommonItems();
            ItemRegistry.RegisterItem("teimo advert pile(itemx)", (item) => Utils.HandleCountableItem(item, "Sheets", 30, "sheet"));
            ItemRegistry.RegisterItem("fire extinguisher(itemx)", (item) => HandleFireExtinguisher(item, "Fluid", 100f, name: "%", namePlural: "%"));
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
            ItemRegistry.RegisterItem("Fire Extinguisher(VINXX)", (item) => Utils.HandleUncountableItem(item, "Fluid", 100f, name: "%", namePlural: "%"));
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
            detailsExactSetting = Settings.AddCheckBoxGroup("detailsExactSetting", "Show exact values", value: true, "group_content_details");
            itemUnitsSetting = Settings.AddDropDownList("itemUnitsSetting", "Show exact value in:", new string[] { "Metric from item type", "Percentage" });

            detailsRoughSetting = Settings.AddCheckBoxGroup("detailsRoughSetting", "Show rough guess", value: false, "group_content_details");
            detailsEducatedSetting = Settings.AddCheckBoxGroup("detailsEducatedSetting", "Show educated guess", value: false, "group_content_details");


            Settings.AddText("<b>Overrides</b>");
            alwaysExactCountablesSetting = Settings.AddCheckBox("alwaysExactCountablesSetting", "Always show exact value for countables", value: true);
        }

        private void Mod_OnLoad()
        {
            // Called once, when mod is loading after game is fully 
            itemContentDescription = ItemContentDescription.Instance();

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
                itemContentDescription.SetText(text);
                return;
            }
            ;

            GameObject item = raycastHit.transform.gameObject;

            if (ItemRegistry.TryGetItemHandler(item.name, out Action<GameObject> handler))
                handler(item);

            itemContentDescription.SetText(text);
        }
    }
}
