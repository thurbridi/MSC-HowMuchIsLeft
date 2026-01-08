![image01](https://github.com/user-attachments/assets/bf5d6f38-784d-4e5d-af41-448781226d7b)

# How Much Is Left
A mod for My Summer Car and My Winter Car that displays the contents of some items when you look at them. See currently supported items below.

## Dependencies
- [MSCLoader](https://github.com/piotrulos/MSCModLoader/releases)

## Mod Settings
Three settings for information detail are available through MSCLoader. These are:

- Show exact values (default): displays the exact amount following the units shown on the product label
- Show rough guess: shows the amount as one of three possible values (besides empty and full)
- Show educated guess: shows the amount as one of seven possible values (besides empty and full)


Countable items, like spark plugs, will always show exact values by default. If you'd like to see a guess instead, simply disable the override setting.

## Supported Items
- Two Stroke Fuel
- Motor Oil
- Brake Fluid
- Coolant
- Ground Coffee
- Grill Charcoal
- Spray Can
- Mosquito Spray
- Fuse Package
- R20 Battery Box
- Spark Plug Box
- Advert Pile
- Fire Extinguisher

### My Winter Car
All the items previously mentioned, plus
- Automatic Transmission Fluid
- Packaging Sheets
- Chargers Box
- Manuals Box
- Plastic Trays

## For Modders
As of version 1.3.0, this mod includes a simple API for other modders to add support for their own items, or to extend this mod with other vanilla items. 

For example, if you wanted to add support for beer cases, you could do something like this:

```c#
using HowMuchIsLeft;

// ...

private void Mod_OnLoad()
{
    // ...

    // Example: to show a text like "5 beer bottles remaining" when you look at a beer case,
    HowMuchIsLeftAPI.RegisterItem("beer case(itemx)", (item) =>
    {
        int drank_beers = item.GetComponent<PlayMakerFSM>().FsmVariables.FindFsmInt("DestroyedBottles").Value;
        int total_beers = 24;
        int remaining_beers = total_beers - drank_beers;
                
        // Handlers must call GenerateText to properly update the UI
        HowMuchIsLeftAPI.GenerateText(remaining_beers, total_beers, "beer bottle", true);
    });

    // ...
}
```

For more details, see [HowMuchIsLeftAPI.cs](HowMuchIsLeft/HowMuchIsLeftAPI.cs) in the source code. Some utility methods are also exposed in [Utils.cs](HowMuchIsLeft/Utils.cs).