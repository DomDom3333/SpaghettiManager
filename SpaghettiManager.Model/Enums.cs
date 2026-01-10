using System;

namespace SpaghettiManager.Model;

public static class Enums
{
    public enum MaterialFamily
    {
        Unknown = 0,
        Pla,
        PetCopolyester,   // PET, PETG, PCTG, CPE, nGen
        Styrenics,        // ABS, ASA, HIPS, PC-ABS
        Polycarbonate,    // PC
        Polyamide,        // PA (Nylon)
        FlexibleTpe,      // TPU, TPE, TPC
        Polypropylene,    // PP
        Acrylic,          // PMMA
        Acetal,           // POM
        Paek,             // PEEK, PEKK
        Pei,              // PEI (Ultem)
        Sulfone,          // PPS, PPSU
        WaterSoluble,     // PVA, BVOH
    }

    [Flags]
    public enum AdditiveMaterial
    {
        None = 0,
        CarbonFiber = 1 << 0,
        GlassFiber = 1 << 1,
        AramidFiber = 1 << 2,
        BasaltFiber = 1 << 3,
        MetalFilled = 1 << 4,
        Wood = 1 << 5,
        Ceramic = 1 << 6,
        Stone = 1 << 7,
        MagneticIron = 1 << 8,
        Phosphorescent = 1 << 9,
        Conductive = 1 << 10,
        EsdSafe = 1 << 11,
        Graphene = 1 << 12,
        CarbonNanotube = 1 << 13,
        Glitter = 1 << 14,
    }

    public enum Opacity
    {
        Unknown = 0,
        Opaque,
        Translucent,
        Transparent,
    }

    public enum Finish
    {
        Unknown = 0,
        Matte,
        Glossy,
        Silk,
        Sparkle,
        Textured,
    }

    [Flags]
    public enum MaterialCategory
    {
        Unknown = 0,
        Standard = 1 << 0,
        Engineering = 1 << 1,
        HighTemperature = 1 << 2,
        Flexible = 1 << 3,
        Support = 1 << 4,
        Composite = 1 << 5,
    }

    public enum Hygroscopicity
    {
        Unknown = 0,
        Low,
        Medium,
        High,
        VeryHigh,
    }

    public enum NozzleAbrasiveness
    {
        Unknown = 0,
        NonAbrasive,
        ModeratelyAbrasive,
        HighlyAbrasive,
    }

    public enum SpoolMaterial
    {
        Unknown = 0,
        Plastic,
        Cardboard,
        Metal,
    }

    public enum CarrierKind
    {
        Unknown = 0,
        Spool,
        SpoollessCoil,
        MasterSpoolRefill, // “refill” meant for a reusable master spool
    }

    public enum FilamentDiameter
    {
        Unknown = 0,
        Mm175,
        Mm285,
    }

    public enum FilamentForm
    {
        Unknown = 0,
        Round,
        Oval,
        FilledComposite, // still “round” physically, but helps UI/filtering
    }

    public enum WindingDirection
    {
        Unknown = 0,
        Clockwise,
        CounterClockwise,
    }

    public enum InventoryStatus
    {
        Unknown = 0,
        Sealed,
        Opened,
        InUse,
        Empty,
        Discarded,
    }
}
