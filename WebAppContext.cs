using System;
using System.Collections.Generic;
using WebApps.Dxf;
using Microsoft.JSInterop;
using WebApps.Components;
using System.Drawing;

namespace WebApps
{
    public class WebAppContext
    {
        public JsInteropHandler js { get; private set; }

        public Dictionary<string, WAShape> shapes { get; private set; }

        public List<string> sizeOrderedShapeKeys { get; private set; }

        public List<LayoutData> sheetLayout { get; set; }

        private List<int> lineIndices = new List<int>();

        public ShapeListDisplay shapeListDisplay { get; set; }

        public event Action OnShapesChange;

        public WebAppContext()
        {
            shapes = new Dictionary<string, WAShape>();
            sheetLayout = new List<LayoutData>();
            sizeOrderedShapeKeys = new List<string>();
        }

        public void BindJSRuntime(IJSRuntime jsRuntime)
        {
            js = new JsInteropHandler(jsRuntime);
            js.InitializeJS();
        }

        public void Pack(Point pSize, int pSpacing, bool pAllowFlip)
        {
            sheetLayout = ShapePacker.Pack(this, pSize, pSpacing, pAllowFlip);

            List<float> vertexBuffer = new List<float>();
            lineIndices = new List<int>();
            int topIndex = 0;

            foreach (LayoutData inst in sheetLayout)
            {
                vertexBuffer.AddRange(shapes[inst.key].GetVertexBuffer(pSize, inst.pos, inst.scale, inst.fliped));

                int[] li = shapes[inst.key].lineIndices;

                for (int i = 0; i < li.Length / 2; i++)
                {
                    lineIndices.Add(topIndex);
                    lineIndices.Add(li[i * 2 + 1]);

                    topIndex += lineIndices[lineIndices.Count - 1];
                }
            }

            js.LinkVertexBuffer(IdList.MainCanvasId, vertexBuffer.ToArray());
        }

        public void DrawPacker()
        {
            js.DrawCanvas(IdList.MainCanvasId, lineIndices.ToArray());
        }

        /*
        public void DrawPackerBitmap(PointF packerSize)
        {

            js.ClearCanvas(IdList.MainCanvasId);

            List<float> vertexBuffer = new List<float>();

            foreach (LayoutData inst in sheetLayout)
            {
                bool[,] bitmap = shapes[inst.key].GetBimap(inst.scale, inst.fliped);

                for (int row = 0; row < bitmap.GetLength(0); row++)
                {
                    for (int column = 0; column < bitmap.GetLength(1); column++)
                    {
                        if (bitmap[row, column])
                        {
                            vertexBuffer.Add((row + inst.pos.X) / (packerSize.X / 2) - 1);
                            vertexBuffer.Add((column + inst.pos.Y) / (packerSize.Y / 2) - 1);
                            vertexBuffer.Add(0);
                        }
                    }
                }
            }

            js.DrawBitmap(IdList.MainCanvasId, vertexBuffer.ToArray());
        }
        */

        public List<WAShape> GetOrderdShapes()
        {
            List<WAShape> oShapes = new List<WAShape>();

            foreach(string key in sizeOrderedShapeKeys)
            {
                oShapes.Add(shapes[key]);
            }

            return oShapes;
        }

        public void AddShape(string file)
        {
            string key = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();

            shapes.Add(key, new WAShape(DXFReader.ReadFile(file), key));

            if(sizeOrderedShapeKeys.Count == 0)
            {
                sizeOrderedShapeKeys.Add(key);
            }
            else
            {
                for (int i = 0; i < sizeOrderedShapeKeys.Count; i++)
                {
                    float area = shapes[sizeOrderedShapeKeys[i]].GetDisplayArea();

                    if (area <= shapes[key].GetDisplayArea())
                    {
                        sizeOrderedShapeKeys.Insert(i, key);
                        break;
                    }
                    else if (i == sizeOrderedShapeKeys.Count - 1)
                    {
                        sizeOrderedShapeKeys.Add(key);
                        break;
                    }
                }
            }

            ClearDeployment();
            HandleShapesChange();
        }

        public void RemoveShape(string key)
        {
            ClearDeployment();
            shapes.Remove(key);
            sizeOrderedShapeKeys.Remove(key);

            HandleShapesChange();
        }

        public void SetShapeQuantity(string key, int quant)
        {
            ClearDeployment();
            shapes[key].SetQuantity(quant);
        }

        public void SetShapeScale(string key, float scale)
        {
            ClearDeployment();
            shapes[key].SetDisplayScale(scale);
        }

        public void ClearDeployment()
        {
            sheetLayout.Clear();
            js.ClearCanvas(IdList.MainCanvasId);
            lineIndices.Clear();
        }

        private void HandleShapesChange() => OnShapesChange?.Invoke();
    }
}
