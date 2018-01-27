using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace StegoApp
{
    public class UnreadList
    {

        Dictionary<string, Record> unreadList = new Dictionary<string, Record>();

        public UnreadList(string filename) => ParseCsv(filename);
        
        void ParseCsv(string filename)
        {

            var csvLines = File.ReadAllLines(filename);

            foreach(var line in csvLines)
            {

                var record = ParseCsvLine(line);
                unreadList.Add(record.Path, record);

            }

        }

        Record ParseCsvLine(string line)
        {

            var values = line.Split(new char[] { ',' }, 3);

            if (values.Length < 3)
                throw new FileFormatException("Invalid unread file");

            var record = new Record(values[0], values[1], 
                values[2] == "overwrite" ? true : false);
           
            return record;

        }

        public IReadOnlyDictionary<string, Record> Messages
        {

            get => unreadList;

        }

        public void Add(string path, string hash)
        {

            if (!unreadList.ContainsKey(path))
                unreadList.Add(path, new Record(path, hash, false));
            else
                unreadList[path] = new Record(path, hash, true);

        }

        public void Remove(string path) => unreadList.Remove(path);

        public void Write(string filename)
        {

            var lines = new List<string>();

            foreach(var record in unreadList)
                lines.Add(GetLine(record.Value));

            File.WriteAllLines(filename, lines.ToArray());

        }

        string GetLine(Record record) => $"{record.Path},{record.Hash}," +
            $"{(record.Overwrite == true ? "overwrite" : "")}";
        
        public class Record
        {

            // path - image path
            // hash - image hash
            // overwrite - Did this message overwrote some message
            public Record(string path, string hash, bool overwrite)
            {

                Path = path;
                Hash = hash;
                Overwrite = overwrite;

            }

            public string Path
            {

                get;

            }

            public string Hash
            {

                get;

            }

            public bool Overwrite
            {

                get;

            }
   
        }
         
    }
}
