/********************************************************************************
** auth： qinjiancai
** date： 2022/5/27 16:02:00
** desc： 数据IO支持
** Ver.:  V1.0.0
*********************************************************************************/
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;
using Newtonsoft.Json;

public class DataIOHelper : Singleton<DataIOHelper>
{
    public bool IsFileExit(string fileName)
    {
        return File.Exists(fileName);
    }

    public bool IsDirectoryExit(string path)
    {
        return Directory.Exists(path);
    }

    /// <summary>
    /// 创建一个文本文件
    /// </summary>
    /// <param name="fileName">文件路径</param>
    /// <param name="content">文件内容</param>
    public void CreateFile(string fileName, string content)
    {
        StreamWriter streamWriter = File.CreateText(fileName);
        streamWriter.Write(content);
        streamWriter.Close();
    }

    /// <summary>
    /// 创建一个文件夹
    /// </summary>
    /// <param name="fileName"></param>
    public void CreateDirectory(string fileName)
    {
        if (IsDirectoryExit(fileName))
            return;

        Directory.CreateDirectory(fileName);
    }

    /// <summary>
    /// 存入数据
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="pContent">数据内容</param>
    public void SetData(string fileName, object pContent)
    {
        string toSaveData = SerializeData(pContent);
        toSaveData = RijndaelEncryptData(toSaveData, "RP_DATA_SAVE_CMD_XXXXXXXXXX");

        StreamWriter writer = File.CreateText(fileName);
        writer.Write(toSaveData);
        writer.Close();
    }

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public object GetData(string fileName, Type type)
    {
        if (IsFileExit(fileName) == false)
        {
            return null;
        }

        StreamReader reader = File.OpenText(fileName);
        string data = reader.ReadToEnd();
        data = RijndaelDecryptData(data, "RP_DATA_SAVE_CMD_XXXXXXXXXX");
        reader.Close();

        return DeserializeData(data, type);
    }

    /// <summary>
    /// 数据序列化
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    private string SerializeData(object obj)
    {
        string result = string.Empty;
        result = JsonConvert.SerializeObject(obj);
        return result;
    }

    /// <summary>
    /// 反序列化数据
    /// </summary>
    /// <param name="content"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private object DeserializeData(string content, Type type)
    {
        object obj = null;
        obj = JsonConvert.DeserializeObject(content, type);
        return obj;
    }

    /// <summary>
    /// rijin加密
    /// </summary>
    /// <param name="content"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public string RijndaelEncryptData(string content, string key)
    {
        //秘钥
        byte[] keyArry = UTF8Encoding.UTF8.GetBytes(key);
        //待加密数据
        byte[] dataArry = UTF8Encoding.UTF8.GetBytes(content);

        RijndaelManaged rijndael = new RijndaelManaged();
        rijndael.Key = keyArry;
        rijndael.Mode = CipherMode.ECB;
        rijndael.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTran = rijndael.CreateEncryptor();

        byte[] result = cTran.TransformFinalBlock(dataArry, 0, dataArry.Length);
        return Convert.ToBase64String(result, 0, result.Length);
    }

    /// <summary>
    /// 数据解密
    /// </summary>
    /// <param name="content"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public string RijndaelDecryptData(string content, string key)
    {
        byte[] keyArry = UTF8Encoding.UTF8.GetBytes(key);
        byte[] dataArry = Convert.FromBase64String(content);

        RijndaelManaged rijndael = new RijndaelManaged();
        rijndael.Key = keyArry;
        rijndael.Mode = CipherMode.ECB;
        rijndael.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTran = rijndael.CreateDecryptor();

        byte[] result = cTran.TransformFinalBlock(dataArry, 0, dataArry.Length);
        return UTF8Encoding.UTF8.GetString(result);
    }
}
