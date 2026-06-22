using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace RopeTangle
{
    [AddComponentMenu("UI/Effects/Gradient")]
    public class UIGradient : BaseMeshEffect
    {
        public Color m_color1 = Color.white;
        public Color m_color2 = Color.white;
        [Range(-180f, 180f)]
        public float m_angle = 0f;
        public bool m_ignoreRatio = true;
        public override void ModifyMesh(VertexHelper vh)
        {
            if (enabled)
            {
                Rect rect = graphic.rectTransform.rect;
                Vector2 dir = UIGradientUtils.RotationDir(m_angle);
                if (!m_ignoreRatio)
                    dir = UIGradientUtils.CompensateAspectRatio(rect, dir);
                UIGradientUtils.Matrix2x3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);
                UIVertex vertex = default(UIVertex);
                for (int i = 0; i < vh.currentVertCount; i++)
                {
                    vh.PopulateUIVertex(ref vertex, i);
                    Vector2 localPosition = localPositionMatrix * vertex.position;
                    vertex.color *= Color.Lerp(m_color2, m_color1, localPosition.y);
                    vh.SetUIVertex(vertex, i);
                }
            }
        }
        protected override void OnEnable()
        {
        }
        protected override void OnDisable()
        {
        }
        public void SetVerticesDirty(Color _1, Color _2)
        {
            m_color1 = _1;
            m_color2 = _2;
            graphic.SetVerticesDirty();
        }
        //public void SetVerticesDirty(ConfigTheme _theme){
        //    m_color1 = _theme.colorTop;
        //    m_color2 = _theme.colorBottom;
        //    m_angle = _theme.angle;
        //    graphic.SetVerticesDirty();
        //}
    }
}