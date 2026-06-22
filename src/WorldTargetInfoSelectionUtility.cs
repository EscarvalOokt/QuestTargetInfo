using RimWorld;
using RimWorld.Planet;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoSelectionUtility
    {
        public static bool TryCreateRequest(out WorldTargetInfoRequest request)
        {
            request = null;

            PlanetTile originTile = Find.CurrentMap?.Tile ?? PlanetTile.Invalid;
            if(!originTile.Valid)
                return false;

            if(TryCreateRequestForSelectedObject(originTile, out request))
                return true;

            if(TryCreateRequestForSelectedTile(originTile, out request))
                return true;

            return false;
        }

        private static bool TryCreateRequestForSelectedObject(
            PlanetTile originTile,
            out WorldTargetInfoRequest request)
        {
            request = null;

            WorldObject selectedObject = Find.WorldSelector.SingleSelectedObject;
            if(selectedObject == null)
                return false;

            if(!selectedObject.Tile.Valid)
                return false;

            request = new WorldTargetInfoRequest(
                originTile,
                selectedObject.Tile,
                selectedObject.LabelCap,
                selectedObject,
                WorldTargetInfoSource.WorldInspectPane);

            return true;
        }

        private static bool TryCreateRequestForSelectedTile(
            PlanetTile originTile,
            out WorldTargetInfoRequest request)
        {
            request = null;

            if(Find.WorldSelector.NumSelectedObjects != 0)
                return false;

            PlanetTile selectedTile = Find.WorldSelector.SelectedTile;
            if(!selectedTile.Valid)
                return false;

            string label = GetSelectedTileLabel(selectedTile);

            request = new WorldTargetInfoRequest(
                originTile,
                selectedTile,
                label,
                null,
                WorldTargetInfoSource.WorldInspectPane);

            return true;
        }

        private static string GetSelectedTileLabel(PlanetTile tile)
        {
            if(ModsConfig.OdysseyActive && Find.World.landmarks[tile] != null)
                return Find.World.landmarks[tile].name;

            BiomeDef biome = Find.WorldGrid[tile].PrimaryBiome;
            if(biome != null)
                return biome.LabelCap;

            return "Tile".Translate();
        }
    }
}