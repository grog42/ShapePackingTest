using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Drawing;

namespace WebApps.Dxf
{
    public static class DXFWriter
    {
        private static Header PreLoadHeader(WebAppContext context)
        {
            Header header = new Header();

            ValuePairList insunits = new ValuePairList();
            insunits.Add(new ValuePair(9, "$INSUNITS"));
            insunits.Add(new ValuePair(70, "4"));

            header.AddVariable(insunits);

            float minX = float.PositiveInfinity;
            float minY = float.PositiveInfinity;
            float maxX = float.NegativeInfinity;
            float maxY = float.NegativeInfinity;

            foreach (LayoutData inst in context.sheetLayout)
            {
                DXFShape shape = context.shapes[inst.key].dxfBase;

                float unitScale = context.shapes[inst.key].getUnitScale();

                {
                    float x = float.Parse(shape.header.variableList["$EXTMIN"].Get(10).First()) * unitScale + inst.pos.X;
                    float y = float.Parse(shape.header.variableList["$EXTMIN"].Get(20).First()) * unitScale + inst.pos.Y;

                    minX = (x < minX) ? x : minX;
                    minY = (y < minY) ? y : minY;
                }

                {
                    float x = float.Parse(shape.header.variableList["$EXTMAX"].Get(10).First()) * unitScale + inst.pos.X;
                    float y = float.Parse(shape.header.variableList["$EXTMAX"].Get(20).First()) * unitScale + inst.pos.Y;

                    maxX = (x > maxX) ? x : maxX;
                    maxY = (y > maxY) ? y : maxY;
                }
            }

            ValuePairList extMin = new ValuePairList();
            extMin.Add(new ValuePair(9, "$EXTMIN"));
            extMin.Add(new ValuePair(10, minX.ToString()));
            extMin.Add(new ValuePair(20, minY.ToString()));

            header.AddVariable(extMin);

            ValuePairList extMax = new ValuePairList();
            extMax.Add(new ValuePair(9, "$EXTMAX"));
            extMax.Add(new ValuePair(10, maxX.ToString()));
            extMax.Add(new ValuePair(20, maxY.ToString()));

            header.AddVariable(extMax);

            return header;
        }
        public static void Write(WebAppContext context)
        {
            Header header = PreLoadHeader(context);
            Classes classes = new Classes();
            Tables tables = new Tables();
            Blocks blocks = new Blocks();
            Entities entities = new Entities();
            Objects objects = new Objects();

            Stopwatch stopwatch = new Stopwatch();

            stopwatch.Start();

            foreach (LayoutData inst in context.sheetLayout)
            {
                DXFShape shape = context.shapes[inst.key].dxfBase;

                foreach (string key in shape.header.variableList.Keys)
                {
                    if (!header.variableList.ContainsKey(key))
                    {
                        header.AddVariable(shape.header.variableList[key]);
                    }
                }

                if (shape.classes != null)
                {
                    foreach (string key in shape.classes.classList.Keys)
                    {
                        if (!classes.classList.ContainsKey(key))
                        {
                            classes.AddClass(shape.classes.classList[key]);
                        }
                    }
                }

                foreach (string key in shape.tables.tableList.Keys)
                {
                    if (!tables.tableList.ContainsKey(key))
                    {
                        tables.AddTable(shape.tables.tableList[key]);
                    }
                }

                if (shape.blocks != null)
                {
                    foreach (string key in shape.blocks.blockList.Keys)
                    {
                        if (!blocks.blockList.ContainsKey(key))
                        {
                            blocks.AddBlock(shape.blocks.blockList[key]);
                        }
                    }
                }

                foreach (ValuePairList enty in shape.entities.entityList)
                {
                    ValuePairList tempEnty = new ValuePairList();

                    float minX = float.Parse(shape.header.variableList["$EXTMIN"].Get(10).First());
                    float minY = float.Parse(shape.header.variableList["$EXTMIN"].Get(20).First());

                    float maxX = float.Parse(shape.header.variableList["$EXTMAX"].Get(10).First());
                    float maxY = float.Parse(shape.header.variableList["$EXTMAX"].Get(20).First());

                    float unitScale = context.shapes[inst.key].getUnitScale();

                    foreach (int key in enty.orderdKeys)
                    {
                        foreach (string value in enty.Get(key))
                        {
                            if ((9 < key && key < 19))
                            {
                                if (inst.fliped)
                                {
                                    tempEnty.Add(new ValuePair(key, (((maxX - float.Parse(value)) * unitScale) + inst.pos.X).ToString()));
                                }
                                else
                                {
                                    tempEnty.Add(new ValuePair(key, (((float.Parse(value) - minX) * unitScale) + inst.pos.X).ToString()));
                                }

                            }
                            else if (19 < key && key < 29)
                            {

                                if (inst.fliped)
                                {
                                    tempEnty.Add(new ValuePair(key, (((maxY - float.Parse(value)) * unitScale) + inst.pos.Y).ToString()));
                                }
                                else
                                {
                                    tempEnty.Add(new ValuePair(key, (((float.Parse(value) - minY) * unitScale) + inst.pos.Y).ToString()));
                                }

                            }
                            else
                            {
                                tempEnty.Add(new ValuePair(key, value));
                            }
                        }
                    }

                    entities.AddEntity(tempEnty);
                }

                if (shape.objects != null)
                {
                    foreach (string key in shape.objects.objectList.Keys)
                    {
                        if (!objects.objectList.ContainsKey(key))
                        {
                            objects.AddObject(shape.objects.objectList[key]);
                        }
                    }
                }
            }

            stopwatch.Stop();
            context.js.ConsoleLog("Building time taken: " + stopwatch.ElapsedMilliseconds.ToString());
            stopwatch.Restart();

            LinkedList<char> fileStr = new LinkedList<char>();

            if (header.variableList.Count > 0)
            {
                AppendLine(context, ref fileStr, "0\nSECTION\n");
                AppendLine(context, ref fileStr, "2\nHEADER\n");

                foreach (KeyValuePair<string, ValuePairList> keyVar in header.variableList)
                {
                    AppendLine(context, ref fileStr, keyVar.Value.ReadToFile());
                }

                AppendLine(context, ref fileStr, "0\nENDSEC\n");
            }

            if (classes.classList.Count > 0)
            {
                AppendLine(context, ref fileStr, "0\nSECTION\n");
                AppendLine(context, ref fileStr, "2\nCLASSES\n");

                foreach (KeyValuePair<string, ValuePairList> keyClass in classes.classList)
                {
                    AppendLine(context, ref fileStr, keyClass.Value.ReadToFile());
                }

                AppendLine(context, ref fileStr, "0\nENDSEC\n");
            }

            if (tables.tableList.Count > 0)
            {
                AppendLine(context, ref fileStr, "0\nSECTION\n");
                AppendLine(context, ref fileStr, "2\nTABLES\n");

                foreach (KeyValuePair<string, Table> keyTable in tables.tableList)
                {
                    AppendLine(context, ref fileStr, keyTable.Value.valuePairs.ReadToFile());

                    foreach (List<ValuePairList> entries in keyTable.Value.entries.Values)
                    {
                        foreach (ValuePairList entry in entries)
                        {
                            AppendLine(context, ref fileStr, entry.ReadToFile());
                        }
                    }

                    AppendLine(context, ref fileStr, "0\nENDTAB\n");
                }

                AppendLine(context, ref fileStr, "0\nENDSEC\n");
            }

            if (blocks.blockList.Count > 0)
            {
                AppendLine(context, ref fileStr, "0\nSECTION\n");
                AppendLine(context, ref fileStr, "2\nBLOCKS\n");

                foreach (ValuePairList block in blocks.blockList.Values)
                {
                    AppendLine(context, ref fileStr, block.ReadToFile());
                }

                AppendLine(context, ref fileStr, "0\nENDSEC\n");
            }

            if (entities.entityList.Count > 0)
            {
                AppendLine(context, ref fileStr, "0\nSECTION\n");
                AppendLine(context, ref fileStr, "2\nENTITIES\n");

                foreach (ValuePairList enty in entities.entityList)
                {
                    AppendLine(context, ref fileStr, enty.ReadToFile());
                }

                AppendLine(context, ref fileStr, "0\nENDSEC\n");
            }

            if (objects.objectList.Count > 0)
            {
                AppendLine(context, ref fileStr, WriteObjects(objects, GetTableHandles(tables)));
            }

            AppendLine(context, ref fileStr, "0\nEOF\n");

            context.js.AppendToStorage(IdList.WriterStorageKey, new string(fileStr.ToArray()));

            stopwatch.Stop();
            context.js.ConsoleLog("Writing time take: " + stopwatch.ElapsedMilliseconds.ToString());
        }

        private static void AppendLine(WebAppContext context, ref LinkedList<char> fileStr, string line)
        {
            char[] charArray = line.ToCharArray();

            for (int i = 0; i < charArray.Length; i++)
            {
                fileStr.AddLast(charArray[i]);
            }
        }

        private static string WriteObjects(Objects objects, List<string> extHandles)
        {
            string str = "";

            str += "0\nSECTION\n";
            str += "2\nOBJECTS\n";

            Queue<List<string>> objectLevel = new Queue<List<string>>();

            objectLevel.Enqueue(new List<string>());

            objectLevel.First().Add(objects.baseDictHandle);

            foreach (string handle in extHandles)
            {
                objectLevel.First().Add(handle);
            }

            do
            {
                List<string> handles = new List<string>();

                foreach (string handle in objectLevel.Dequeue())
                {
                    str += objects.objectList[handle].ReadToFile();

                    if (objects.objectList[handle].Get(350) != null)
                    {
                        handles.AddRange(objects.objectList[handle].Get(350));
                    }

                    if (objects.objectList[handle].Get(360) != null)
                    {
                        handles.AddRange(objects.objectList[handle].Get(360));
                    }
                }

                if (handles.Count > 0)
                {
                    objectLevel.Enqueue(handles);
                }

            } while (objectLevel.Count > 0);

            str += "0\nENDSEC\n";

            return str;
        }

        private static List<string> GetTableHandles(Tables tables)
        {
            List<string> handles = new List<string>();

            foreach (string key in tables.tableList.Keys)
            {
                foreach (List<ValuePairList> entrys in tables.tableList[key].entries.Values)
                {
                    foreach (ValuePairList pairs in entrys)
                    {
                        if (pairs.Get(361) != null)
                        {
                            handles.Add(pairs.Get(361).First());
                        }
                    }
                }
            }

            return handles;
        }
    }
}
