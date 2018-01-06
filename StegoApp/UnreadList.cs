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
        // times message is written to image
        Dictionary<string, int> imageList;

        public UnreadList(string filename)
        {

            var imagePaths = File.ReadAllLines(filename);
            imageList = new Dictionary<string, int>();

            foreach (var imagePath in imagePaths)
            {

                if (! imageList.ContainsKey(imagePath))
                    imageList.Add(imagePath, 1);
                else
                    ++imageList[imagePath];

            }

        }

        public IReadOnlyDictionary<string, int> Images
        {

            get
            {
                return imageList;
            }

        }

        public void Add(string path)
        {

            if (! imageList.ContainsKey(path))
                imageList.Add(path, 1);
            else
                ++imageList[path];

        }

        public void Write(string filename)
        {

            using (StreamWriter file = new StreamWriter(filename))
            {
                foreach (var imagePath in imageList.Keys)
                {

                    int count = imageList[imagePath];

                    for(int i = 0; i < count; ++i)
                        file.WriteLine(imagePath);

                }
                
            }

        }
         
    }
}
