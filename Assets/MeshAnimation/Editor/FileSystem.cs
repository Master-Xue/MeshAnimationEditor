using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class FileSystem  {

    public static string GetFileName(string path, string name)
    {
        int num = 1;

        string filePath = Path.Combine(path, name);
        string fileName = Path.GetFileNameWithoutExtension(filePath);
        string extension = Path.GetExtension(filePath);
        while (File.Exists(filePath))
        {
            string newFileName = string.Format("{0} {1}{2}", fileName, num, extension);//文件名+数字+后缀
            filePath = Path.Combine(path, newFileName);//保存新路径
            num++;
        }
        return filePath;
    }
}
