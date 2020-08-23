using System.Collections.Generic;
using System.Linq;

namespace WebApps.Dxf
{
    public struct ValuePair
    {
        public int key { get; }
        public string value { get; }

        public ValuePair(int _Key, string _Value)
        {
            key = _Key;
            value = _Value;
        }
    }
    public class ValuePairList
    {
        private Dictionary<int, List<string>> valuePairs { get; }

        public List<int> orderdKeys { private set; get; }

        public int Count = 0;

        public ValuePairList()
        {
            valuePairs = new Dictionary<int, List<string>>();
            orderdKeys = new List<int>();
        }

        public void Add(ValuePair pair)
        {
            orderdKeys.Add(pair.key);

            if (valuePairs.ContainsKey(pair.key))
            {
                valuePairs[pair.key].Add(pair.value);
            }
            else
            {
                valuePairs.Add(pair.key, new List<string>());
                valuePairs[pair.key].Add(pair.value);
                Count++;
            }
        }

        public List<string> Get(int i)
        {
            if (valuePairs.ContainsKey(i))
            {
                return valuePairs[i];
            }

            return null;
        }

        public bool Contains(int i)
        {
            if (valuePairs.ContainsKey(i))
            {
                return true;
            }
            return false;
        }

        public string ReadToFile()
        {
            string str = "";

            Dictionary<int, IEnumerator<string>> enumPairs = new Dictionary<int, IEnumerator<string>>();

            foreach (int key in valuePairs.Keys)
            {
                enumPairs.Add(key, valuePairs[key].GetEnumerator());
                enumPairs[key].Reset();
            }

            foreach (int key in orderdKeys)
            {
                enumPairs[key].MoveNext();
                str += key + "\n" + enumPairs[key].Current + "\n";
            }

            return str;
        }
    }

    public class Header
    {
        public Dictionary<string, ValuePairList> variableList { get; }

        public Header()
        {
            variableList = new Dictionary<string, ValuePairList>();
        }

        public void AddVariable(ValuePairList variable)
        {
            variableList.Add(variable.Get(9).First(), variable);
        }
    }

    public class Classes
    {
        public Dictionary<string, ValuePairList> classList { get; }

        public Classes()
        {
            classList = new Dictionary<string, ValuePairList>();
        }

        public void AddClass(ValuePairList dXFClass)
        {
            classList.Add(dXFClass.Get(1).First(), dXFClass);
        }
    }

    public class Tables
    {
        public Dictionary<string, Table> tableList { get; }

        public Tables()
        {
            tableList = new Dictionary<string, Table>();
        }

        public void AddTable(Table table)
        {
            tableList.Add(table.valuePairs.Get(2).First(), table);
        }
    }

    public class Table
    {
        public ValuePairList valuePairs { get; }
        public Dictionary<string, List<ValuePairList>> entries { get; }

        public Table(ValuePairList _Pairs)
        {
            entries = new Dictionary<string, List<ValuePairList>>();
            valuePairs = _Pairs;
        }

        public void AddEntry(ValuePairList entry)
        {
            string key;

            if (entry.Contains(2)) 
            {
                key = entry.Get(2).First();
            }
            else
            {
                key = entry.Get(3).First();
            }

            if (entries.ContainsKey(key))
            {
                entries[key].Add(entry);
            }
            else
            {
                entries.Add(key, new List<ValuePairList>());
                entries[key].Add(entry);
            }
        }
    }

    public class Blocks
    {
        public Dictionary<string, ValuePairList> blockList { get; }

        public Blocks()
        {
            blockList = new Dictionary<string, ValuePairList>();
        }

        public void AddBlock(ValuePairList block)
        {
            blockList.Add(block.Get(5).First(), block);
        }
    }

    public class Entities
    {
        public List<ValuePairList> entityList { get; }

        public Entities()
        {
            entityList = new List<ValuePairList>();
        }

        public void AddEntity(ValuePairList entity)
        {
            entityList.Add(entity);
        }
    }

    public class Objects
    {
        public string baseDictHandle { get; private set; }
        public Dictionary<string, ValuePairList> objectList { get; }

        public Objects()
        {
            objectList = new Dictionary<string, ValuePairList>();
        }

        public void AddObject(ValuePairList obj)
        {
            if (objectList.Count == 0)
            {
                baseDictHandle = obj.Get(5).First();
            }

            objectList.Add(obj.Get(5).First(), obj);
        }
    }
}
