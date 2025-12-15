using System;

namespace Norsemen;

public static class NameGenerator
{
    private static readonly Random rng = new Random();
    
    private static readonly string[] MaleBaseNames =
    {
        "Ragnar", "Bjorn", "Erik", "Olaf", "Thor", "Leif", "Gunnar", "Ulf",
        "Sven", "Magnus", "Ivar", "Harald", "Knut", "Sigurd", "Finn", "Rolf",
        "Eirik", "Dag", "Nils", "Arne", "Bard", "Einar", "Hakon", "Ingvar",
        "Rollo", "Odin", "Loki", "Balder", "Tyr", "Heimdall", "Vidar", "Vali"
    };

    private static readonly string[] FemaleBaseNames =
    {
        "Astrid", "Freydis", "Gudrun", "Helga", "Ingrid", "Sigrid", "Thora",
        "Brunhild", "Solveig", "Ragnhild", "Asa", "Gunnhild", "Bergthora",
        "Valdis", "Thyra", "Liv", "Signe", "Inga", "Kirsten", "Runa",
        "Freyja", "Frigg", "Sif", "Idun", "Nanna", "Hel", "Skadi", "Eir"
    };

    public static string GenerateMaleName()
    {
        string baseName = MaleBaseNames[rng.Next(MaleBaseNames.Length)];
        return baseName;
    }

    public static string GenerateFemaleName()
    {
        string baseName = FemaleBaseNames[rng.Next(FemaleBaseNames.Length)];
        return baseName;
    }
}