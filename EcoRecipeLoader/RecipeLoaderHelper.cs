﻿namespace EcoAssist;

internal static class RecipeLoaderHelper
{
    internal static CraftingTable GetTableType(string tableType)
    {
        switch (tableType)
        {
            case "AdvancedCarpentryTableItem":
                return CraftingTable.AdvancedCarpentryTable;
            case "AdvancedMasonryTableItem":
                return CraftingTable.AdvancedMasonryTable;
            case "AdvancedTailoringTableItem":
                return CraftingTable.AdvancedTailoringTable;
            case "AnvilItem":
                return CraftingTable.Anvil;
            case "ArrastraItem":
                return CraftingTable.Arrastra;
            case "AssemblyLineItem":
                return CraftingTable.AssemblyLine;
            case "AutomaticLoomItem":
                return CraftingTable.AutomaticLoom;
            case "BakeryOvenItem":
                return CraftingTable.BakeryOven;
            case "BlastFurnaceItem":
                return CraftingTable.BlastFurnace;
            case "BloomeryItem":
                return CraftingTable.Bloomery;
            case "ButcheryTableItem":
                return CraftingTable.ButcheryTable;
            case "CampfireItem":
                return CraftingTable.Campfire;
            case "CampsiteItem":
                return CraftingTable.Campsite;
            case "CapitolItem":
                return CraftingTable.Capitol;
            case "CarpentryTableItem":
                return CraftingTable.CarpentryTable;
            case "CastIronStoveItem":
                return CraftingTable.CastIronStove;
            case "CementKilnItem":
                return CraftingTable.CementKiln;
            case "ElectricLatheItem":
                return CraftingTable.ElectricLathe;
            case "ElectricMachinistTableItem":
                return CraftingTable.ElectricMachinistTable;
            case "ElectricPlanerItem":
                return CraftingTable.ElectricPlaner;
            case "ElectricStampingPressItem":
                return CraftingTable.ElectricStampingPress;
            case "ElectronicsAssemblyItem":
                return CraftingTable.ElectronicsAssembly;
            case "FarmersTableItem":
                return CraftingTable.FarmersTable;
            case "FisheryItem":
                return CraftingTable.Fishery;
            case "FrothFloatationCellItem":
                return CraftingTable.FrothFloatationCell;
            case "JawCrusherItem":
                return CraftingTable.JawCrusher;
            case "KilnItem":
                return CraftingTable.Kiln;
            case "KitchenItem":
                return CraftingTable.Kitchen;
            case "LaboratoryItem":
                return CraftingTable.Laboratory;
            case "LatheItem":
                return CraftingTable.Lathe;
            case "LoomItem":
                return CraftingTable.Loom;
            case "MachinistTableItem":
                return CraftingTable.MachinistTable;
            case "MasonryTableItem":
                return CraftingTable.MasonryTable;
            case "MillItem":
                return CraftingTable.Mill;
            case "OilRefineryItem":
                return CraftingTable.OilRefinery;
            case "PumpJackItem":
                return CraftingTable.PumpJack;
            case "ResearchTableItem":
                return CraftingTable.ResearchTable;
            case "RoboticAssemblyLineItem":
                return CraftingTable.RoboticAssemblyLine;
            case "RockerBoxItem":
                return CraftingTable.RockerBox;
            case "RollingMillItem":
                return CraftingTable.RollingMill;
            case "SawmillItem":
                return CraftingTable.Sawmill;
            case "ScreeningMachineItem":
                return CraftingTable.ScreeningMachine;
            case "ScrewPressItem":
                return CraftingTable.ScrewPress;
            case "SensorBasedBeltSorterItem":
                return CraftingTable.SensorBasedBeltSorter;
            case "ShaperItem":
                return CraftingTable.Shaper;
            case "SpinMelterItem":
                return CraftingTable.SpinMelter;
            case "StampMillItem":
                return CraftingTable.StampMill;
            case "StoveItem":
                return CraftingTable.Stove;
            case "TailoringTableItem":
                return CraftingTable.TailoringTable;
            case "ToolBenchItem":
                return CraftingTable.ToolBench;
            case "WainwrightTableItem":
                return CraftingTable.WainwrightTable;
            case "WorkbenchItem":
                return CraftingTable.Workbench;
        }
        return CraftingTable.Invalid;
    }
}