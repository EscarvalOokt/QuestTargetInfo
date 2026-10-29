using System;
using UnityEngine;

namespace QuestTargetInfo.UI
{
    internal static class QuestTargetWindowLayout
    {
        internal const float PreferredWidth = 360f;
        internal const float PreferredHeight = 640f;
        internal const float CollapsedHeight = 60f;
        private const float Gap = 5f;

        public static Rect Calculate(Rect owner, float screenWidth, float screenHeight, bool collapsed)
        {
            float marginX = Math.Min(Gap, Math.Max(0, screenWidth / 2f));
            float marginY = Math.Min(Gap, Math.Max(0, screenHeight / 2f));
            float width = Math.Min(PreferredWidth, Math.Max(0, screenWidth - marginX * 2));
            float height = Math.Min(
                collapsed ? CollapsedHeight : PreferredHeight,
                Math.Max(0, screenHeight - marginY * 2));

            float x = owner.xMax + Gap;
            if (x + width > screenWidth - marginX)
            {
                float left = owner.xMin - Gap - width;
                x = left >= marginX ? left : screenWidth - marginX - width;
            }

            float y = owner.yMax - height;
            x = Math.Clamp(x, marginX, Math.Max(marginX, screenWidth - marginX - width));
            y = Math.Clamp(y, marginY, Math.Max(marginY, screenHeight - marginY - height));
            return new Rect(x, y, width, height);
        }
    }
}