using System.Collections.Generic;
using System.Drawing;
using System;

namespace WebApps
{
    public static class ShapePacker
    {
        private class RLEShape
        {
            public int[][] runLengths;
            public Point size;

            public RLEShape(int[][] _RunLengths, int width, int height)
            {
                runLengths = _RunLengths;
                size = new Point(width, height);
            }
        }
        public static List<LayoutData> Pack(WebAppContext context, Point sheetMetrics, int spacing, bool allowFlip)
        {
            bool[,] sheetHitmap = new bool[sheetMetrics.X, sheetMetrics.Y];
            RLEShape rleSheet = null;

            List<LayoutData> displayTokens = new List<LayoutData>();

            foreach (string key in context.sizeOrderedShapeKeys)
            {

                WAShape shape = context.shapes[key];
                for (int j = 0; j < shape.quantity; j++)
                {
                    Point? pos = null;
                    bool fliped = false;
                    RLEShape rleShape = RunLengthEncode(shape.GetHitmap(spacing, shape.displayScale));

                    context.js.ConsoleLog(rleShape.size.X + ":" + shape.size.X);
                    context.js.ConsoleLog(rleShape.size.Y + ":" + shape.size.Y);

                    if (rleSheet == null)
                    {
                        rleSheet = RunLengthEncode(sheetHitmap);
                    }

                    pos = Place(rleSheet, pos, rleShape, sheetMetrics, false);

                    if (allowFlip)
                    {
                        Point? p = Place(rleSheet, pos, rleShape, sheetMetrics, true);

                        if (p.HasValue && (!pos.HasValue || CalcPosValue(p.Value) < CalcPosValue(pos.Value)))
                        {
                            pos = p;
                            fliped = true;
                        }
                    }

                    if (pos.HasValue) { 
                    
                        for(int row = 0; row < rleShape.runLengths.Length; row++)
                        {
                            for(int column = 0; column < rleShape.runLengths[row].Length / 2; column++)
                            {
                                for(int x = rleShape.runLengths[row][column * 2]; x <= rleShape.runLengths[row][column * 2 + 1]; x++)
                                {
                                    if (fliped)
                                    {
                                        sheetHitmap[rleShape.size.X - x + pos.Value.X, rleShape.size.Y - row + pos.Value.Y] = true;
                                    }
                                    else
                                    {
                                        sheetHitmap[x + pos.Value.X, row + pos.Value.Y] = true;                                     
                                    }
                                }
                            }
                        }

                        rleSheet = null;
                   
                        displayTokens.Add(new LayoutData(shape.key, new PointF(pos.Value.X + spacing, pos.Value.Y + spacing), fliped, shape.displayScale));
                    }
                }
            }

            return displayTokens;
        }

        /// <summary>
        /// Tests points on the sheet to find an optimal position for the shape.
        /// The collisions are done using run length encoded data insted of directly using the bitmaps representing the shapes.
        /// </summary>
        /// <param name="rleSheet"></param>
        /// <param name="pos"></param>
        /// <param name="rleShape"></param>
        /// <param name="sheetMetrics"></param>
        /// <param name="fliped"></param>
        /// <returns>Point which is null if a position could not be found or the point most valubale point</returns>
        private static Point? Place(RLEShape rleSheet, Point? pos, RLEShape rleShape, Point sheetMetrics, bool fliped)
        {
            float posValue = CalcPosValue(sheetMetrics);

            if (pos.HasValue)
            {
                posValue = CalcPosValue(pos.Value);
            }

            for (int row = 0; row < rleSheet.size.Y - rleShape.size.Y; row++)
            {

                if (!Collide(rleSheet, 0, row, rleShape, fliped))
                {
                    return new Point(0, row);
                }

                int rlNumber = rleSheet.runLengths[row].Length/2;

                for (int column = 0; column < rlNumber; column++)
                {
                    int x = rleSheet.runLengths[row][column * 2 + 1] + 1;
                    int newValue = CalcPosValue(new Point(x, row));

                    if (newValue < posValue && x < rleSheet.size.X - rleShape.size.X && !Collide(rleSheet, x, row, rleShape, fliped))
                    {
                        posValue = newValue;
                        pos = new Point(x, row);
                    }
                }

                for (int x = LastPointInRow(rleSheet.runLengths[row]); x < rleSheet.size.X - rleShape.size.X; x++)
                {
                    int newValue = CalcPosValue(new Point(x, row));

                    if (newValue < posValue && x < rleSheet.size.X - rleShape.size.X && !Collide(rleSheet, x, row, rleShape, fliped))
                    {
                        posValue = newValue;
                        pos = new Point(x, row);
                    }
                }
            }

            return pos;
        }

        private static int LastPointInRow(int[] row)
        {
            if(row.Length == 0)
            {
                return 0;
            }

            return row[row.Length - 1];
        }

        /// <summary>
        /// Calculates the placement value of the point (lower is better)
        /// </summary>
        /// <param name="point"></param>
        /// <param name="sheetMetrics"></param>
        /// <returns></returns>
        private static int CalcPosValue(Point point)
        {
            return point.X + point.Y;
        }

        private static RLEShape RunLengthEncode(bool[,] bitmap)
        {
            int[][] rle = new int[bitmap.GetLength(1)][];

            for(int row = 0; row < bitmap.GetLength(1); row++)
            {
                int start = -1;

                List<int> blocks = new List<int>();

                for (int x = 0; x < bitmap.GetLength(0); x++)
                {
                    if (bitmap[x, row])
                    {
                        if (start < 0)
                        {
                            start = x;
                        }
                    }
                    else if (start > -1)
                    {
                        blocks.Add(start);
                        blocks.Add(x - 1);
                        start = -1;
                    }
                }

                if (start > -1)
                {
                    blocks.Add(start);
                    blocks.Add(bitmap.GetLength(0));
                }

                rle[row] = blocks.ToArray();
            }

            return new RLEShape(rle, bitmap.GetLength(0), bitmap.GetLength(1));
        }
        private static bool Collide(RLEShape rleSheet, int posX, int posY, RLEShape rleShape, bool fliped)
        {
            for (int row = 0; row < rleShape.size.Y; row++)
            {

                int[] shapeRow = rleShape.runLengths[row];
                int[] sheetRow = rleSheet.runLengths[row + posY];

                if (fliped)
                {
                    shapeRow = rleShape.runLengths[rleShape.size.Y - row - 1];
                }

                for(int i = 0; i < shapeRow.Length / 2; i++)
                {
                    int start = shapeRow[i * 2];
                    int end = shapeRow[i * 2 + 1];

                    if (fliped)
                    {
                        end = rleShape.size.X - start;
                        start = rleShape.size.X - end;
                    }

                    for(int j = 0; j < sheetRow.Length/2; j++)
                    {
                        if (Overlap(start + posX, end + posX, sheetRow[j * 2], sheetRow[j * 2 + 1]))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool Overlap(int A1, int A2, int B1, int B2)
        {
            if ((A1 <= B1 && B1 <= A2) || (A1 <= B2 && B2 <= A2) || (B1 <= A1 && A1 <= B2) || (B1 <= A2 && A2 <= B2))
            {
                return true;
            }
            return false;
        }
    }
}
