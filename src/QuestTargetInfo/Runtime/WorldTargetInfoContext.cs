using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using QuestTargetInfo.Info;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace QuestTargetInfo.Runtime
{
    internal sealed class WorldTargetInfoContext
    {
        private WorldTargetInfoContext(
            World world,
            Map map,
            WorldTargetInfoRequest request,
            PlanetLayer originLayer,
            PlanetLayer targetLayer)
        {
            Map = map;
            Request = request;
            GeometryKey = new WorldTargetGeometryKey(
                world,
                map,
                originLayer,
                targetLayer,
                request.OriginTile.tileId,
                request.TargetTile.tileId,
                WorldTargetTopologySnapshot.Capture(world));
        }

        public Map Map { get; }
        public WorldTargetInfoRequest Request { get; }
        public WorldTargetGeometryKey GeometryKey { get; }

        public static bool TryGetEnvironment(out World world, out Map map)
        {
            world = null;
            map = null;
            Game game = Current.Game;
            if (Current.ProgramState != ProgramState.Playing || game == null)
                return false;

            world = game.World;
            if (world?.grid == null || game.currentMapIndex < 0 || game.currentMapIndex >= game.Maps.Count)
                return false;

            map = game.CurrentMap;
            return map != null && TryGetLayer(world, map.Tile, out _);
        }

        public static bool TryCreate(
            World world,
            Map map,
            PlanetTile targetTile,
            string targetLabel,
            WorldObject targetObject,
            WorldTargetInfoSource source,
            out WorldTargetInfoContext context)
        {
            context = null;
            if (world == null ||
                map == null ||
                !TryGetLayer(world, map.Tile, out PlanetLayer originLayer) ||
                !TryGetLayer(world, targetTile, out PlanetLayer targetLayer))
                return false;

            if (targetObject != null && (targetObject.Destroyed || !world.worldObjects.Contains(targetObject)))
                return false;

            var request = new WorldTargetInfoRequest(map.Tile, targetTile, targetLabel, targetObject, source);
            context = new WorldTargetInfoContext(world, map, request, originLayer, targetLayer);
            return true;
        }

        public static bool TryCreateForQuest(Quest quest, out WorldTargetInfoContext context)
        {
            context = null;
            if (quest == null || !TryGetEnvironment(out World world, out Map map))
                return false;

            // Enumerate once and preserve the established single-world-target behavior.
            using IEnumerator<GlobalTargetInfo> targets = quest.QuestLookTargets.GetEnumerator();
            if (!targets.MoveNext())
                return false;

            GlobalTargetInfo target = targets.Current;
            if (targets.MoveNext() || !target.IsValid || !target.IsWorldTarget)
                return false;

            WorldObject targetObject = target.HasWorldObject ? target.WorldObject : null;
            PlanetTile tile = targetObject?.Tile ?? target.Tile;
            if (!TryGetLayer(world, tile, out _))
                return false;

            string label = targetObject != null ? targetObject.LabelCap : target.Label;
            if (label == "Location".Translate())
                return false;

            return TryCreate(world, map, tile, label, targetObject, WorldTargetInfoSource.Quest, out context);
        }

        private static bool TryGetLayer(World world, PlanetTile tile, out PlanetLayer layer)
        {
            layer = null;
            if (!tile.Valid || world?.grid == null)
                return false;

            // PlanetTile.Valid only checks tileId. Removed layers must be rejected before UI reads.
            try
            {
                layer = tile.Layer;
                return layer != null &&
                       world.grid.PlanetLayers.TryGetValue(layer.LayerID, out PlanetLayer registered) &&
                       ReferenceEquals(layer, registered) &&
                       tile.tileId < layer.TilesCount;
            }
            catch (KeyNotFoundException)
            {
                return false;
            }
        }
    }

    // References and raw IDs avoid PlanetTile equality/hash resolving a different current world.
    internal readonly struct WorldTargetGeometryKey(
        object world,
        object map,
        object originLayer,
        object targetLayer,
        int originTile,
        int targetTile,
        WorldTargetTopologySnapshot topology = null) : IEquatable<WorldTargetGeometryKey>
    {
        private readonly object _world = world;
        private readonly object _map = map;
        private readonly object _originLayer = originLayer;
        private readonly object _targetLayer = targetLayer;
        private readonly int _originTile = originTile;
        private readonly int _targetTile = targetTile;
        private readonly WorldTargetTopologySnapshot _topology = topology;

        public bool Equals(WorldTargetGeometryKey other)
        {
            return HasSameEndpoints(other) && Equals(_topology, other._topology);
        }

        public bool HasSameEndpoints(WorldTargetGeometryKey other)
        {
            return ReferenceEquals(_world, other._world) &&
                   ReferenceEquals(_map, other._map) &&
                   ReferenceEquals(_originLayer, other._originLayer) &&
                   ReferenceEquals(_targetLayer, other._targetLayer) &&
                   _originTile == other._originTile &&
                   _targetTile == other._targetTile;
        }

        public override bool Equals(object obj) => obj is WorldTargetGeometryKey other && Equals(other);

        public override int GetHashCode() =>
            HashCode.Combine(
                _world == null ? 0 : RuntimeHelpers.GetHashCode(_world),
                _map == null ? 0 : RuntimeHelpers.GetHashCode(_map),
                _originLayer == null ? 0 : RuntimeHelpers.GetHashCode(_originLayer),
                _targetLayer == null ? 0 : RuntimeHelpers.GetHashCode(_targetLayer),
                _originTile,
                _targetTile,
                _topology);
    }

    internal sealed class WorldTargetTopologySnapshot : IEquatable<WorldTargetTopologySnapshot>
    {
        private readonly (object Layer, object Target, int Tiles, bool Reachable, float Cost)[] _entries;

        internal WorldTargetTopologySnapshot(
            params (object Layer, object Target, int Tiles, bool Reachable, float Cost)[] entries)
        {
            _entries = entries;
        }

        public bool Equals(WorldTargetTopologySnapshot other)
        {
            if (other == null || _entries.Length != other._entries.Length)
                return false;
            for (var i = 0; i < _entries.Length; i++)
            {
                (object Layer, object Target, int Tiles, bool Reachable, float Cost) a = _entries[i];
                (object Layer, object Target, int Tiles, bool Reachable, float Cost) b = other._entries[i];
                if (!ReferenceEquals(a.Layer, b.Layer) ||
                    !ReferenceEquals(a.Target, b.Target) ||
                    a.Tiles != b.Tiles ||
                    a.Reachable != b.Reachable ||
                    !a.Cost.Equals(b.Cost))
                    return false;
            }

            return true;
        }

        public static WorldTargetTopologySnapshot Capture(World world)
        {
            var entries = new List<(object, object, int, bool, float)>();
            foreach (PlanetLayer layer in world.grid.PlanetLayers.Values.OrderBy(layer => layer.LayerID))
            {
                entries.Add((layer, null, layer.TilesCount, layer.CanReachLayer, 0));
                foreach (KeyValuePair<PlanetLayer, PlanetLayerConnection> connection in
                         layer.Connections.OrderBy(pair => pair.Key.LayerID))
                    entries.Add((layer, connection.Key, 0, false, connection.Value.fuelCost));
            }

            return new WorldTargetTopologySnapshot(entries.ToArray());
        }

        public override bool Equals(object obj) => obj is WorldTargetTopologySnapshot other && Equals(other);

        public override int GetHashCode() => _entries.Length;
    }
}