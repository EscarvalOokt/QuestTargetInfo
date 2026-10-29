using QuestTargetInfo.Info;
using QuestTargetInfo.Runtime;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace QuestTargetInfo
{
    internal static class WorldTargetInfoSelectionUtility
    {
        public static bool TryCreateRequest(out WorldTargetInfoRequest request)
        {
            bool success = TryCreateContext(out WorldTargetInfoContext context);
            request = context?.Request;
            return success;
        }

        public static bool TryCreateContext(out WorldTargetInfoContext context)
        {
            context = null;
            if (!WorldTargetInfoContext.TryGetEnvironment(out World world, out Map map))
                return false;

            WorldObject selectedObject = Find.WorldSelector.SingleSelectedObject;
            if (selectedObject != null)
            {
                if (selectedObject.Destroyed || !world.worldObjects.Contains(selectedObject))
                    return false;

                return WorldTargetInfoContext.TryCreate(
                    world,
                    map,
                    selectedObject.Tile,
                    selectedObject.LabelCap,
                    selectedObject,
                    WorldTargetInfoSource.WorldInspectPane,
                    out context);
            }

            if (Find.WorldSelector.NumSelectedObjects != 0)
                return false;

            PlanetTile tile = Find.WorldSelector.SelectedTile;
            // Validate the layer and bounds before asking for a biome or landmark label.
            if (!WorldTargetInfoContext.TryCreate(
                    world,
                    map,
                    tile,
                    null,
                    null,
                    WorldTargetInfoSource.WorldInspectPane,
                    out _))
                return false;

            string label = GetSelectedTileLabel(tile);
            return WorldTargetInfoContext.TryCreate(
                world,
                map,
                tile,
                label,
                null,
                WorldTargetInfoSource.WorldInspectPane,
                out context);
        }

        private static string GetSelectedTileLabel(PlanetTile tile)
        {
            if (ModsConfig.OdysseyActive && Find.World.landmarks[tile] != null)
                return Find.World.landmarks[tile].name;

            BiomeDef biome = Find.WorldGrid[tile].PrimaryBiome;
            return biome != null ? biome.LabelCap.ToString() : "Tile".Translate().ToString();
        }
    }
}