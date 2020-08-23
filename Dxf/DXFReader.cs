using System.Collections.Generic;
using System.IO;

namespace WebApps.Dxf
{
    /// <summary>
    /// Trawls infomation from the file and returns it into a format which allows it to be accessed more quickly
    /// </summary>
    public static class DXFReader
    {
        public static DXFShape ReadFile(string file)
        {
            StringReader reader = new StringReader(file);

            Header header = null;
            Classes classes = null;
            Tables tables = null;
            Blocks blocks = null;
            Entities entities = null;
            Objects objects = null;

            int code;
            string value;

            do
            {
                code = int.Parse(reader.ReadLine().Trim());
                value = reader.ReadLine().Trim();

                if (code == 2)
                {
                    switch (value)
                    {
                        case "HEADER":
                            header = ReadHeader(ref reader);
                            break;

                        case "CLASSES":
                            classes = ReadClasses(ref reader);
                            break;

                        case "TABLES":
                            tables = ReadTables(ref reader);
                            break;

                        case "BLOCKS":
                            blocks = ReadBlocks(ref reader);
                            break;

                        case "ENTITIES":
                            entities = ReadEntities(ref reader);
                            break;

                        case "OBJECTS":
                            objects = ReadObjects(ref reader);
                            break;
                    }
                }

            } while (code != 0 || value != "EOF");

            return new DXFShape(header, classes, tables, blocks, entities, objects);
        }

        private static Header ReadHeader(ref StringReader reader)
        {
            Header header = new Header();
            int code;
            string value;
            ValuePairList pairs = new ValuePairList();

            do
            {
                code = int.Parse(reader.ReadLine().Trim());
                value = reader.ReadLine().Trim();

                if (code == 0 && value == "ENDSEC")
                {
                    header.AddVariable(pairs);
                    return header;
                }

                if (code == 9 && pairs.Count > 0)
                {
                    header.AddVariable(pairs);
                    pairs = new ValuePairList();
                }

                pairs.Add(new ValuePair(code, value));

            } while (true);
        }

        private static Classes ReadClasses(ref StringReader reader)
        {
            Classes classes = new Classes();
            int code;
            string value;

            ValuePairList pairs = new ValuePairList();

            do
            {
                code = int.Parse(reader.ReadLine().Trim());
                value = reader.ReadLine().Trim();

                if (code == 0 && value == "ENDSEC")
                {
                    classes.AddClass(pairs);
                    return classes;
                }

                if (code == 0 && pairs.Count > 0 && value == "CLASS")
                {
                    classes.AddClass(pairs);
                    pairs = new ValuePairList();
                }

                pairs.Add(new ValuePair(code, value));

            } while (true);
        }

        private static Tables ReadTables(ref StringReader reader)
        {
            Tables tables = new Tables();
            int code;
            string value;

            ValuePairList pairs = new ValuePairList();

            Table tempTable = null;

            do
            {
                code = int.Parse(reader.ReadLine().Trim());
                value = reader.ReadLine().Trim();

                if (value == "ENDSEC")
                {
                    return tables;
                }

                if (value == "ENDTAB")
                {
                    if (pairs.Count > 0)
                    {
                        tempTable = new Table(pairs);
                        pairs = new ValuePairList();
                        tables.AddTable(tempTable);
                    }
                }
                else if (code == 0 && pairs.Count > 0)
                {
                    tempTable = new Table(pairs);
                    pairs = new ValuePairList();

                    foreach (ValuePairList entryPairs in ReadEntrys(ref reader, value))
                    {

                        tempTable.AddEntry(entryPairs);

                    }

                    tables.AddTable(tempTable);
                }
                else
                {
                    pairs.Add(new ValuePair(code, value));
                }

            } while (true);
        }

        private static List<ValuePairList> ReadEntrys(ref StringReader reader, string tableName)
        {
            int code;
            string value;

            List<ValuePairList> pairsList = new List<ValuePairList>();

            ValuePairList pairs = new ValuePairList();

            pairs.Add(new ValuePair(0, tableName));

            do
            {
                code = int.Parse(reader.ReadLine().Trim());
                value = reader.ReadLine().Trim();

                if (code == 0)
                {
                    if (value == "ENDTAB")
                    {
                        pairsList.Add(pairs);
                        return pairsList;
                    }
                    else
                    {
                        pairsList.Add(pairs);
                        pairs = new ValuePairList();
                    }
                }

                pairs.Add(new ValuePair(code, value));

            } while (true);
        }

        private static Blocks ReadBlocks(ref StringReader reader)
        {
            Blocks blocks = new Blocks();
            int code;
            string value;

            ValuePairList pairs = new ValuePairList();

            do
            {
                code = int.Parse(reader.ReadLine().Trim());
                value = reader.ReadLine().Trim();

                if (code == 0 && value == "ENDSEC")
                {
                    blocks.AddBlock(pairs);
                    return blocks;
                }

                if (code == 0 && pairs.Count > 0)
                {
                    blocks.AddBlock(pairs);
                    pairs = new ValuePairList();
                }

                pairs.Add(new ValuePair(code, value));

            } while (true);
        }

        private static Entities ReadEntities(ref StringReader reader)
        {
            Entities entities = new Entities();
            int code;
            string value;

            ValuePairList pairs = new ValuePairList();

            do
            {
                code = int.Parse(reader.ReadLine().Trim());
                value = reader.ReadLine().Trim();

                if (code == 0 && value == "ENDSEC")
                {
                    entities.AddEntity(pairs);
                    return entities;
                }

                if (code == 0 && value != "ENDSEC" && pairs.Count > 0)
                {
                    entities.AddEntity(pairs);
                    pairs = new ValuePairList();
                }

                pairs.Add(new ValuePair(code, value));

            } while (true);
        }

        private static Objects ReadObjects(ref StringReader reader)
        {
            Objects objects = new Objects();
            int code;
            string value;

            ValuePairList pairs = new ValuePairList();

            do
            {
                code = int.Parse(reader.ReadLine().Trim());
                value = reader.ReadLine().Trim();

                if (code == 0 && value == "ENDSEC")
                {
                    objects.AddObject(pairs);
                    return objects;
                }

                if (code == 0 && pairs.Count > 0)
                {
                    objects.AddObject(pairs);
                    pairs = new ValuePairList();
                }

                pairs.Add(new ValuePair(code, value));

            } while (true);
        }
    }
}
