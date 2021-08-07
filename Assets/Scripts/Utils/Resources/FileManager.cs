using System.Collections.Generic;
using System.IO;

namespace SoundPropagation{
    public class FileManager{
        public static void storePoint(Vector2 vector, string id){
            string path = "Assets/Resources/Files/" + id + ".txt";
            string content = vector.x + " " + vector.y;
            writeFile(path, content);
        }

        public static Vector2 readPoint(string id){
            string path = "Assets/Resources/Files/" + id + ".txt";
            string content = readFile(path);
            string[] splits = content.Split();
            return new Vector2(float.Parse(splits[0]), float.Parse(splits[1]));
        }

        public static string readFile(string path){
            StreamReader reader = new StreamReader(path);
            string content = reader.ReadToEnd();
            reader.Close();
            return content;
        }

        public static void writeFile(string path, string content){
            StreamWriter writer = new StreamWriter(path, false);
            writer.Write(content);
            writer.Close();
        }

        public static void writeFile(string path, List<string> content){
            StreamWriter writer = new StreamWriter(path, false);
            foreach(string str in content)
                writer.WriteLine(str);
            writer.Close();
        }

        public static void appendFile(string path, string content){
            StreamWriter writer = new StreamWriter(path, true);
            writer.WriteLine(content);
            writer.Close();
        }
    }
}
