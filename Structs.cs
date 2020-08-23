using System.Drawing;

namespace WebApps
{
    public struct LayoutData
    {
        public string key { get; }
        public PointF pos { get; }
        public bool fliped { get; }
        public float scale { get; }

        public LayoutData(string _Key, PointF _Pos, bool _Fliped, float _Scale)
        {
            key = _Key;
            pos = _Pos;
            fliped = _Fliped;
            scale = _Scale;
        }
    }
}
