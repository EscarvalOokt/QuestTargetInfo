using System;
using System.Collections.Generic;

namespace QuestTargetInfo.Runtime
{
    // The clock is supplied by the caller so refresh behavior can be tested without a game.
    internal sealed class TimedModelCache<TGeometryKey, TModelKey, TGeometry, TModel>
    {
        internal const double RefreshIntervalSeconds = 0.5;
        private TGeometry _geometry;
        private TGeometryKey _geometryKey;

        private bool _hasGeometry;
        private bool _hasModel;
        private double _lastRefresh;
        private TModel _model;
        private TModelKey _modelKey;

        public TModel Get(
            TGeometryKey geometryKey,
            TModelKey modelKey,
            double now,
            Func<TGeometry> calculateGeometry,
            Func<TGeometry, TModel> buildModel)
        {
            bool geometryChanged =
                !_hasGeometry || !EqualityComparer<TGeometryKey>.Default.Equals(_geometryKey, geometryKey);

            if (geometryChanged)
            {
                _geometry = calculateGeometry();
                _geometryKey = geometryKey;
                _hasGeometry = true;
                _hasModel = false;
            }

            if (!_hasModel ||
                !EqualityComparer<TModelKey>.Default.Equals(_modelKey, modelKey) ||
                now < _lastRefresh ||
                now - _lastRefresh >= RefreshIntervalSeconds)
            {
                _model = buildModel(_geometry);
                _modelKey = modelKey;
                _lastRefresh = now;
                _hasModel = true;
            }

            return _model;
        }

        public void Clear()
        {
            _hasGeometry = false;
            _hasModel = false;
            _geometryKey = default;
            _modelKey = default;
            _geometry = default;
            _model = default;
            _lastRefresh = 0;
        }
    }
}