using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace StegoApp
{
    class UnreadList
    {

        // Key is path to image, Value is number of 
        // times image was overwritten 
        Dictionary<string, int> imageDict;

        public UnreadList(string filename)
        {

            var imagePaths = File.ReadAllLines(filename);
            imageDict = new Dictionary<string, int>();

            foreach (var imagePath in imagePaths)
            {

                if (! imageDict.ContainsKey(imagePath))
                    imageDict.Add(imagePath, 1);
                else
                    ++imageDict[imagePath];

            }

        }

        public IReadOnlyCollection<PathCountPair> Images
        {

            get
            {
                return imageDict.Select(pair => new PathCountPair(pair.Key, pair.Value)).ToList();
            }

        }

        public void Add(string path)
        {

            if (! imageDict.ContainsKey(path))
                imageDict.Add(path, 1);
            else
                ++imageDict[path];

        }

        public void Remove(string path)
        {

            imageDict.Remove(path);

        }
        public void Write(string filename)
        {

            using (StreamWriter file = new StreamWriter(filename))
            {
                foreach (var imagePath in imageDict.Keys)
                {

                    int count = imageDict[imagePath];

                    for(int i = 0; i < count; ++i)
                        file.WriteLine(imagePath);

                }
                
            }

        }

        public class PathCountPair
        {

            // path - image path
            // count - number of times image was overwritten
            public PathCountPair(string path, int count)
            {

                Path = path;
                Count = count;

            }

            public string Path
            {
                get;
            }

            public int Count
            {

                get;

            }

        }
         
    }
}
