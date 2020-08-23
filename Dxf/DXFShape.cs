using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace WebApps.Dxf
{
    /// <summary>
    /// An object containing the indexed info of a dxf file 
    /// </summary>
    public class DXFShape
    {
        public Header header { get; private set; }
        public Classes classes { get; private set; }
        public Tables tables { get; private set; }
        public Blocks blocks { get; private set; }
        public Entities entities { get; private set; }
        public Objects objects { get; private set; }

        /// <summary>
        /// Multiplication value for the dxf files original units to millimeters
        /// </summary>
        public float unitLength { get; private set; } = 1;

        /// <summary>
        /// Buffer of floats representing vertices(x, y, z) used to render the shape
        /// </summary>
        public List<float> vertexBuffer { get; private set; }

        /// <summary>
        /// Collection of vertex index and length pairs which exspress the location and length of vertices which make up a line
        /// </summary>
        public List<int> lineIndices { get; private set; }

        /// <summary>
        /// Top-left most point
        /// </summary>
        private PointF maxVert;

        /// <summary>
        /// Bottom-right most point
        /// </summary>
        private PointF minVert;

        public DXFShape(Header _Header, Classes _Classes, Tables _Tables, Blocks _Blocks, Entities _Entities, Objects _Objects)
        {
            header = _Header;
            classes = _Classes;
            tables = _Tables;
            blocks = _Blocks;
            entities = _Entities;
            objects = _Objects;

            if (header.variableList.ContainsKey("$INSUNITS"))
            {
                switch (int.Parse(header.variableList["$INSUNITS"].Get(70).First()))
                {
                    //Inch
                    case 1:
                        unitLength = 25.4f;
                        break;
                    //Foot
                    case 2:
                        unitLength = 304.8f;
                        break;
                    //Mile
                    case 3:
                        unitLength = 1.609e+6f;
                        break;
                    //CM
                    case 5:
                        unitLength = 10;
                        break;
                    //M
                    case 6:
                        unitLength = 100;
                        break;
                    //KM
                    case 7:
                        unitLength = 1000;
                        break;
                    //Microinches
                    case 8:
                        unitLength = 0.00002539998f;
                        break;
                    //Mils
                    case 9:
                        unitLength = 0.0254f;
                        break;
                    //Yards
                    case 10:
                        unitLength = 914.4f;
                        break;
                    //Angstroms
                    case 11:
                        unitLength = 1e-7f;
                        break;
                    //Nanometers
                    case 12:
                        unitLength = 1e-6f;
                        break;
                    //Microns
                    case 13:
                        unitLength = 0.001f;
                        break;
                    //Decimeters
                    case 14:
                        unitLength = 100;
                        break;
                    //Hectometers
                    case 16:
                        unitLength = 100000;
                        break;
                    //Gigameters
                    case 17:
                        unitLength = 1e+12f;
                        break;
                    //Astronomical units
                    case 18:
                        unitLength = 1.495978707e+14f;
                        break;
                    //Light years
                    case 19:
                        unitLength = 9.461e+18f;
                        break;
                    //Parsecs
                    case 20:
                        unitLength = 3.086e+19f;
                        break;
                }
            }
            maxVert = new PointF(float.Parse(header.variableList["$EXTMAX"].Get(10).First()),
                                 float.Parse(header.variableList["$EXTMAX"].Get(20).First()));

            minVert = new PointF(float.Parse(header.variableList["$EXTMIN"].Get(10).First()),
                                 float.Parse(header.variableList["$EXTMIN"].Get(20).First()));

            SetRenderData();
        }

        /// <summary>
        /// Generates a buffer containing the verticies which can be used to render the shape and an index of the vertices which make up the entities within the shape
        /// </summary>
        public void SetRenderData()
        {
            List<PointF> polyPoints = new List<PointF>();
            vertexBuffer = new List<float>();
            lineIndices = new List<int>();

            foreach (ValuePairList enty in entities.entityList)
            {
                switch (enty.Get(0).First())
                {
                    case "3DFACE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "3DSOLID":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "ACAD_PROXY_ENTITY":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "ARC":
                        {
                            float centerX = ScaleX(float.Parse(enty.Get(10).First())),
                            centerY = ScaleY(float.Parse(enty.Get(20).First())),
                            radius = float.Parse(enty.Get(40).First()) * unitLength,
                            startAngle = float.Parse(enty.Get(50).First()),
                            endAngle = float.Parse(enty.Get(51).First());

                            lineIndices.Add(vertexBuffer.Count / 3);

                            vertexBuffer.AddRange(GetArcVertices(centerX, centerY, radius, startAngle, endAngle));

                            lineIndices.Add(vertexBuffer.Count / 3 - lineIndices[lineIndices.Count-1]);
                        }                  

                        break;

                    case "ATTDEF":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "ATTRIB":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "BODY":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "CIRCLE":
                        {
                            float centerX = ScaleX(float.Parse(enty.Get(10).First())),
                            centerY = ScaleY(float.Parse(enty.Get(20).First())),
                            radius = float.Parse(enty.Get(40).First()) * unitLength;

                            lineIndices.Add(vertexBuffer.Count / 3);

                            vertexBuffer.AddRange(GetArcVertices(centerX, centerY, radius, 0, 360));

                            lineIndices.Add(vertexBuffer.Count / 3 - lineIndices[lineIndices.Count - 1]);
                        }

                        break;

                    case "DIMENSION":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "ELLIPSE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "HATCH":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "HELIX":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "IMAGE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "INSERT":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "LEADER":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "LIGHT":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "LINE":

                        lineIndices.Add(vertexBuffer.Count / 3);

                        vertexBuffer.Add(ScaleX(float.Parse(enty.Get(10).First())));
                        vertexBuffer.Add(ScaleY(float.Parse(enty.Get(20).First())));
                        vertexBuffer.Add(0);
                        vertexBuffer.Add(ScaleX(float.Parse(enty.Get(11).First())));
                        vertexBuffer.Add(ScaleY(float.Parse(enty.Get(21).First())));
                        vertexBuffer.Add(0);


                        lineIndices.Add(vertexBuffer.Count / 3 - lineIndices[lineIndices.Count - 1]);
                        break;

                    case "LWPOLYLINE":
                        {
                            lineIndices.Add(vertexBuffer.Count / 3);

                            for (int i = 0; i < int.Parse(enty.Get(90).First()); i++)
                            {
                                vertexBuffer.Add(ScaleX(float.Parse(enty.Get(10).First())));
                                vertexBuffer.Add(ScaleY(float.Parse(enty.Get(20).First())));
                                vertexBuffer.Add(0);
                            }

                            lineIndices.Add(vertexBuffer.Count / 3 - lineIndices[lineIndices.Count - 1]);
                        }
                        break;

                    case "MESH":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "MLINE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "MLEADERSTYLE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "MLEADER":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "MTEXT":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "OLEFRAME":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "OLE2FRAME":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "POINT":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "POLYLINE":
                        polyPoints.Clear();
                        break;

                    case "RAY":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "REGION":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "SECTION":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "SEQEND":
                        if (polyPoints.Count > 0)
                        {
                            lineIndices.Add(vertexBuffer.Count / 3);

                            foreach (PointF p in polyPoints)
                            {
                                vertexBuffer.Add(ScaleX(p.X));
                                vertexBuffer.Add(ScaleY(p.Y));
                                vertexBuffer.Add(0);
                            }

                            lineIndices.Add(vertexBuffer.Count / 3 - lineIndices[lineIndices.Count - 1]);

                            polyPoints.Clear();
                        }
                        break;

                    case "SHAPE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "SOLID":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "SPLINE":

                        lineIndices.Add(vertexBuffer.Count / 3);

                        vertexBuffer.AddRange(BuildSPLine(enty));

                        lineIndices.Add(vertexBuffer.Count / 3 - lineIndices[lineIndices.Count - 1]);
                        break;

                    case "SUN":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "SURFACE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "TABLE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "TEXT":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "TOLERANCE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "TRACE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "UNDERLAY":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "VERTEX":
                        {
                            float x = float.Parse(enty.Get(10).First());
                            float y = float.Parse(enty.Get(20).First());
                            polyPoints.Add(new PointF(x, y));
                        }
                        break;

                    case "VIEWPORT":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "WIPEOUT":
                        throw new ArgumentException("Entity not supported");
                        break;

                    case "XLINE":
                        throw new ArgumentException("Entity not supported");
                        break;

                    default:
                        throw new ArgumentException("File contains invalid entity");

                }
            }
        }

        public PointF GetSize()
        {
            return new PointF(ScaleX(maxVert.X), ScaleY(maxVert.Y));
        }

        private List<float> GetArcVertices(float centerX, float centerY, float radius, float startAngle, float endAngle)
        {

            List<float> vertexBuffer = new List<float>();

            float angle = (startAngle > endAngle) ? startAngle - endAngle : endAngle - startAngle,
            R = MathF.PI / 180;

            int sampleNumber = (int)Math.Round(radius * (Math.Abs(angle) * R));

            float step = (angle) / sampleNumber;

            for (int i = 0; i <= sampleNumber; i++)
            {
                float a = startAngle + step * i;

                vertexBuffer.Add(centerX + radius * MathF.Cos(a * R));
                vertexBuffer.Add(centerY + radius * MathF.Sin(a * R));
                vertexBuffer.Add(0);
            }

            return vertexBuffer;
        }

        private List<float> BuildSPLine(ValuePairList enty)
        {
            List<float> vertexBuffer = new List<float>();

            int cpNum = int.Parse(enty.Get(73).First());
            int knotNum = int.Parse(enty.Get(72).First());
            int degrees = int.Parse(enty.Get(71).First());

            List<float> cpX = new List<float>();
            List<float> cpY = new List<float>();
            List<float> knots = new List<float>();
            List<float> cpW = new List<float>();

            float lineLength = 0;

            for (int i = 0; i < cpNum; i++)
            {
                cpX.Add(float.Parse(enty.Get(10).ElementAt(i)));
                cpY.Add(float.Parse(enty.Get(20).ElementAt(i)));

                if (0 < i)
                {
                    lineLength += MathF.Sqrt(Math.Abs((cpX[i] - cpX[i - 1]) * 2 + (cpY[i] - cpY[i - 1])) * 2);
                }

                if (enty.Get(41) != null && i < enty.Get(41).Count())
                {
                    cpW.Add(float.Parse(enty.Get(41).ElementAt(i)));
                }
                else
                {
                    cpW.Add(1);
                }
            }

            for (int i = 0; i < knotNum; i++)
            {
                knots.Add(float.Parse(enty.Get(40).ElementAt(i)));
            }

            for (float t = 0; t <= 1; t += 1 / (lineLength))
            {
                vertexBuffer.AddRange(NURBSFunction(cpX, cpY, cpW, degrees, knots, t));
            }

            return vertexBuffer;
        }

        private float[] NURBSFunction(List<float> cpX, List<float> cpY, List<float> cpW, int degree, List<float> knots, float t)
        {
            float x = 0, y = 0, ratWeight = 0;

            for (int i = 0; i < cpX.Count; i++)
            {
                ratWeight += Nip(i, degree, knots, t) * cpW[i];
            }

            for (int i = 0; i < cpX.Count; i++)
            {
                float temp = Nip(i, degree, knots, t);
                x += cpX[i] * cpW[i] * temp / ratWeight;
                y += cpY[i] * cpW[i] * temp / ratWeight;
            }

            return new [] {x, y, 0};
        }

        private float Nip(int i, int degree, List<float> U, float u)
        {
            float[] N = new float[degree + 1];
            float saved, temp;

            int m = U.Count - 1;

            if ((i == 0 && u == U[0]) || (i == (m - degree - 1) && u == U[m]))
                return 1;

            if (u < U[i] || u >= U[i + degree + 1])
                return 0;

            for (int j = 0; j <= degree; j++)
            {
                if (u >= U[i + j] && u < U[i + j + 1])
                    N[j] = 1f;
                else
                    N[j] = 0f;
            }

            for (int k = 1; k <= degree; k++)
            {
                if (N[0] == 0)
                    saved = 0f;
                else
                    saved = ((u - U[i]) * N[0]) / (U[i + k] - U[i]);

                for (int j = 0; j < degree - k + 1; j++)
                {
                    float Uleft = U[i + j + 1];
                    float Uright = U[i + j + k + 1];

                    if (N[j + 1] == 0)
                    {
                        N[j] = saved;
                        saved = 0f;
                    }
                    else
                    {
                        temp = N[j + 1] / (Uright - Uleft);
                        N[j] = saved + (Uright - u) * temp;
                        saved = (u - Uleft) * temp;
                    }
                }
            }
            return N[0];
        }

        private float ScaleX(float x)
        {
            return (x - minVert.X) * unitLength;
        }

        private float ScaleY(float y)
        {
            return (y - minVert.Y) * unitLength;
        }
    }
}
