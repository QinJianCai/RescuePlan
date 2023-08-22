/********************************************************************************
** auth�� qinjiancai
** date�� 2022/5/27 16:02:00
** desc�� ����IO֧��
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
    /// ����һ���ı��ļ�
    /// </summary>
    /// <param name="fileName">�ļ�·��</param>
    /// <param name="content">�ļ�����</param>
    public void CreateFile(string fileName, string content)
    {
        StreamWriter streamWriter = File.CreateText(fileName);
        streamWriter.Write(content);
        streamWriter.Close();
    }

    /// <summary>
    /// ����һ���ļ���
    /// </summary>
    /// <param name="fileName"></param>
    public void CreateDirectory(string fileName)
    {
        if (IsDirectoryExit(fileName))
            return;

        Directory.CreateDirectory(fileName);
    }

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="fileName">�ļ���</param>
    /// <param name="pContent">��������</param>
    public void SetData(string fileName, object pContent)
    {
        string toSaveData = SerializeData(pContent);
        toSaveData = RijndaelEncryptData(toSaveData, "RP_DATA_SAVE_CMD_XXXXXXXXXX");

        StreamWriter writer = File.CreateText(fileName);
        writer.Write(toSaveData);
        writer.Close();
    }

    /// <summary>
    /// ��ȡ����
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
    /// �������л�
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
    /// �����л�����
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
    /// rijin����
    /// </summary>
    /// <param name="content"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public string RijndaelEncryptData(string content, string key)
    {
        //��Կ
        byte[] keyArry = UTF8Encoding.UTF8.GetBytes(key);
        //����������
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
    /// ���ݽ���
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
