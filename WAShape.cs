using System;
using WebApps.Dxf;
using WebApps.Components;
using System.Drawing;
using System.Collections.Generic;

namespace WebApps
{
    //Shape: A class for storing the vertices and usefull metrics of a shape
    public class WAShape
    {
        public PointF size { get; }
        public DXFShape dxfBase { get; }

        public string key { get; }

        public float displayScale { get; private set; } = 1;
        public int quantity { get; private set; } = 1;

        public ShapeInterface display { get; private set; }

        public float[] vertexBuffer { get; private set; }

        public int[] lineIndices { get; private set; }

        //The vertices that represent the shape are aligned so that the lowest x and y value are equal to zero.
        public WAShape(DXFShape dxfShape, string _Key)
        {
            key = _Key;
            dxfBase = dxfShape;
            dxfBase.SetRenderData();
            vertexBuffer = dxfBase.vertexBuffer.ToArray();
            lineIndices = dxfBase.lineIndices.ToArray();
            size = dxfBase.GetSize();
        }

        public float GetDisplayArea()
        {
            return size.X * displayScale * size.Y;
        }

        public PointF GetDisplaySize()
        {
            return new PointF(size.X * displayScale, size.Y * displayScale);
        }

        public float[] GetVertexBuffer(PointF canvasSize, PointF pos, float scale = 1, bool fliped = false)
        {
            float[] drawingBuffer = new float[vertexBuffer.Length];

            for (int i = 0; i < vertexBuffer.Length / 3; i++)
            {
                if (fliped)
                {
                    drawingBuffer[i * 3] = ((vertexBuffer[i * 3] * -scale) + pos.X + (size.X * scale)) / (canvasSize.X / 2) - 1;
                    drawingBuffer[i * 3 + 1] = ((vertexBuffer[i * 3 + 1] * -scale) + pos.Y + (size.Y * scale)) / (canvasSize.Y / 2) - 1;
                    drawingBuffer[i * 3 + 2] = 0;
                }
                else
                {
                    drawingBuffer[i * 3] = (vertexBuffer[i * 3] * scale + pos.X) / (canvasSize.X / 2) - 1;
                    drawingBuffer[i * 3 + 1] = (vertexBuffer[i * 3 + 1] * scale + pos.Y) / (canvasSize.Y / 2) - 1;
                    drawingBuffer[i * 3 + 2] = 0;
                }
            }

            return drawingBuffer;
        }

        public void LinkCanvas(WebAppContext context, string canvasId, PointF canvasSize, PointF pos, float scale = 1, bool fliped = false)
        {
            context.js.LinkVertexBuffer(canvasId, GetVertexBuffer(canvasSize, pos, scale, fliped));
        }

        public bool[,] GetHitmap(int spacing, float scale = 1, bool fliped = false)
        {

            bool[,] bitmap = this.GetBitmap(scale, fliped);
            int bitmapWidth = bitmap.GetLength(0);
            int bitmapHeight = bitmap.GetLength(1);

            Queue<Point> activePixles = new Queue<Point>();

            for (int i = 0; i < bitmapWidth; i++)
            {
                activePixles.Enqueue(new Point(i, -1));
                activePixles.Enqueue(new Point(i, bitmapHeight));
            }

            for (int i = 0; i < bitmapHeight; i++)
            {
                activePixles.Enqueue(new Point(-1, i));
                activePixles.Enqueue(new Point(bitmapWidth, i));
            }

            bool[,] backgroundBitMap = BuildBitmap(bitmapWidth, bitmapHeight, true);

            do
            {
                Point TP = activePixles.Dequeue();

                Point[] points = { new Point(TP.X, TP.Y + 1),
                                   new Point(TP.X + 1, TP.Y),
                                   new Point(TP.X, TP.Y - 1),
                                   new Point(TP.X - 1, TP.Y)};

                foreach (Point P in points)
                {
                    if (0 <= P.X && 0 <= P.Y && P.X < bitmapWidth && P.Y < bitmapHeight)
                    {
                        if (!bitmap[P.X, P.Y] && backgroundBitMap[P.X, P.Y])
                        {
                            backgroundBitMap[P.X, P.Y] = false;
                            activePixles.Enqueue(P);
                        }
                    }
                }

            } while (activePixles.Count > 0);

            bool[,] spacedBitmap = new bool[bitmapWidth + spacing + spacing, bitmapHeight + spacing + spacing];

            for (int column = 0; column < bitmapWidth; column++)
            {
                for (int row = 0; row < bitmapHeight; row++)
                {
                    if (backgroundBitMap[column, row])
                    {
                        for (int x = -spacing; x <= spacing; x++)
                        {
                            for (int y = -spacing; y <= spacing; y++)
                            {
                                spacedBitmap[column + spacing + x, row + spacing + y] = true;
                            }
                        }
                    }
                }
            }

            return spacedBitmap;
        }

        public bool[,] GetBitmap(float scale = 1, bool fliped = false)
        {
            scale *= displayScale;

            int bitmapWidth = (int)Math.Round(size.X * scale) + 1,
                bitmapHeight = (int)Math.Round(size.Y * scale) + 1;

            bool[,] bitmap = new bool[bitmapWidth, bitmapHeight];

            for(int i = 0; i < lineIndices.Length / 2; i++)
            {
                for(int j = lineIndices[i * 2]; j < lineIndices[i * 2 + 1] - 1 + lineIndices[i * 2]; j++)
                {
                    int ptr = j * 3;
                    float x1 = (!fliped) ? vertexBuffer[ptr] : size.X - vertexBuffer[ptr];
                    float y1 = (!fliped) ? vertexBuffer[ptr + 1] : size.Y - vertexBuffer[ptr + 1];
                    float x2 = (!fliped) ? vertexBuffer[ptr + 3] : size.X - vertexBuffer[ptr + 3];
                    float y2 = (!fliped) ? vertexBuffer[ptr + 4] : size.Y - vertexBuffer[ptr + 4];


                    float distX = x2 - x1 * scale,
                          distY = y2 - y1 * scale,
                          dist = (Math.Abs(distX) > Math.Abs(distY)) ? Math.Abs(distX) : Math.Abs(distY),
                          x = distX / dist,
                          y = distY / dist;

                    for (float t = 0; t < dist; t++)
                    {
                        bitmap[(int)Math.Round(x1 + (x * t)), (int)Math.Round(y1 + (y * t))] = true;
                    }
                }
            }

            return bitmap;
        }

        public void SetDisplayScale(float scale)
        {
            displayScale = scale;
        }

        public void SetQuantity(int quant)
        {
            quantity = quant;
        }

        public float getUnitScale()
        {
            return dxfBase.unitLength * display.scale;
        }

        public void LinkInterface(ShapeInterface _Display)
        {
            display = _Display;
        }

        private static bool[,] BuildBitmap(int width, int height, bool startValue)
        {
            bool[,] shapeBitMap = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    shapeBitMap[x, y] = startValue;
                }
            }

            return shapeBitMap;
        }
    }
}
