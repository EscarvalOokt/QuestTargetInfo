using System;
using RimWorld.Planet;

namespace QuestTargetInfo
{
    internal readonly struct WorldTargetInfoRequestKey : IEquatable<WorldTargetInfoRequestKey>
    {
        private readonly PlanetTile _originTile;
        private readonly PlanetTile _targetTile;
        private readonly WorldObject _targetObject;
        private readonly string _targetLabel;
        private readonly WorldTargetInfoSource _source;

        private WorldTargetInfoRequestKey(
            PlanetTile originTile,
            PlanetTile targetTile,
            WorldObject targetObject,
            string targetLabel,
            WorldTargetInfoSource source)
        {
            _originTile = originTile;
            _targetTile = targetTile;
            _targetObject = targetObject;
            _targetLabel = targetLabel;
            _source = source;
        }

        public static WorldTargetInfoRequestKey From(
            WorldTargetInfoRequest request)
        {
            if(request == null)
            {
                return new WorldTargetInfoRequestKey(
                    PlanetTile.Invalid,
                    PlanetTile.Invalid,
                    null,
                    null,
                    default);
            }

            return new WorldTargetInfoRequestKey(
                request.OriginTile,
                request.TargetTile,
                request.TargetObject,
                request.TargetLabel,
                request.Source);
        }

        public bool Equals(WorldTargetInfoRequestKey other)
        {
            return _originTile == other._originTile
                && _targetTile == other._targetTile
                && ReferenceEquals(_targetObject, other._targetObject)
                && string.Equals(_targetLabel, other._targetLabel, StringComparison.Ordinal)
                && _source == other._source;
        }

        public override bool Equals(object obj)
        {
            return obj is WorldTargetInfoRequestKey other
                && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + _originTile.GetHashCode();
                hash = hash * 31 + _targetTile.GetHashCode();
                hash = hash * 31 + (_targetObject != null ? _targetObject.GetHashCode() : 0);
                hash = hash * 31 + (_targetLabel != null ? StringComparer.Ordinal.GetHashCode(_targetLabel) : 0);
                hash = hash * 31 + (int)_source;
                return hash;
            }
        }

        public static bool operator ==(
            WorldTargetInfoRequestKey left,
            WorldTargetInfoRequestKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            WorldTargetInfoRequestKey left,
            WorldTargetInfoRequestKey right)
        {
            return !left.Equals(right);
        }
    }
}