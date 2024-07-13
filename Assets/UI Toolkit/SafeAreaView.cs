using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DefaultNamespace
{
    public class SafeAreaView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<SafeAreaView>
        {
        }

        public SafeAreaView()
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            try
            {
                var safeArea = Screen.safeArea;

                var leftTop =
                    RuntimePanelUtils.ScreenToPanel(panel, new Vector2(safeArea.xMin, Screen.height - safeArea.yMax));

                var rightBottom = RuntimePanelUtils.ScreenToPanel(panel,
                    new Vector2(Screen.width - safeArea.xMax, safeArea.yMin));

                style.paddingLeft = leftTop.x;
                style.paddingTop = leftTop.y;
                style.paddingRight = rightBottom.x;
                style.paddingBottom = rightBottom.y;
            }
            catch (InvalidCastException)
            {
            }
        }
    }
}