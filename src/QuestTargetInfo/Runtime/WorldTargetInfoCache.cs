using System.Diagnostics;
using QuestTargetInfo.Info;
using QuestTargetInfo.Info.ViewModels;

namespace QuestTargetInfo.Runtime
{
    internal sealed class WorldTargetInfoCache
    {
        private readonly TimedModelCache<WorldTargetGeometryKey,
            (object Target, string Label, WorldTargetInfoSource Source, int Settings), WorldTargetRouteInfo,
            WorldTargetInfoModel> _cache = new();

        private bool _hasSelection;
        private WorldTargetGeometryKey _selectionGeometry;
        private WorldTargetInfoSource _selectionSource;
        private object _selectionTarget;

        public WorldTargetInfoModel Get(WorldTargetInfoContext context, out bool selectionChanged)
        {
            WorldTargetInfoRequest request = context.Request;
            selectionChanged = !_hasSelection ||
                               !_selectionGeometry.HasSameEndpoints(context.GeometryKey) ||
                               !ReferenceEquals(_selectionTarget, request.TargetObject) ||
                               _selectionSource != request.Source;

            _hasSelection = true;
            _selectionGeometry = context.GeometryKey;
            _selectionTarget = request.TargetObject;
            _selectionSource = request.Source;

            double now = (double)Stopwatch.GetTimestamp() / Stopwatch.Frequency;
            return _cache.Get(
                context.GeometryKey,
                (request.TargetObject, request.TargetLabel, request.Source, QuestTargetInfoMod.Settings.Version),
                now,
                () => WorldTargetRouteCalculator.Calculate(request),
                route => WorldTargetInfoModelBuilder.Build(request, route, context.Map));
        }

        public void Clear()
        {
            _cache.Clear();
            _hasSelection = false;
            _selectionGeometry = default;
            _selectionTarget = null;
            _selectionSource = default;
        }
    }
}