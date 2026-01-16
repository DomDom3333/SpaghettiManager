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

    public enum BarcodeType
    {
        Unknown = 0,
        Ean = 1,
        Upc = 2,
        Other = 3,
    }
    
    public enum SpoolType : short
    {
        Unknown = 0,
        GenericPlastic = 1,
        Cardboard = 2,
        Reusable = 3,
        Refill = 4
    }
}
