/********************************************************************************
** auth： qinjiancai
** date： 2022/5/24 16:02:00
** desc： 配置文件加载管理器
** Ver.:  V1.0.0
*********************************************************************************/
using System;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif


public class FilesManager : Singleton<FilesManager>
{

    enum AssetsSource
    {
        eResources = 1,             // 使用Resources里面的资源；
        eStreamingAssets,          // 
        ePersistentDataPath,
        eTemporaryCachePath,
    }

    private AssetsSource m_AssetSource = AssetsSource.eResources;
    private readonly string dataPath = Application.dataPath;
    private readonly string streamingAssetsPath = Application.streamingAssetsPath;
    private readonly string persistentDataPath = Application.persistentDataPath;
    private readonly string temporaryCachePath = Application.temporaryCachePath;

    public void OnInit()
    {
#if UNITY_EDITOR
        m_AssetSource = AssetsSource.eResources;

#elif UNITY_ANDROID || UNITY_IPHONE

    m_AssetSource = AssetsSource.ePersistentDataPath;

#endif
    }

    #region GetFileContent
    public byte[] GetFileContent(string strPath, string strExt = "bytes", bool isEditor = false)
    {
        return GetContent(strPath, strExt, isEditor);
    }

    byte[] GetContent(string strPath, string strExt, bool isEditor = false)
    {
        strPath = strPath.Trim();
        if (string.IsNullOrEmpty(strPath)) return null;

        strPath = strPath.ToLower();
        strExt = strExt.ToLower();

        byte[] result = null;

        switch (m_AssetSource)
        {
            case AssetsSource.eResources:
                {
#if UNITY_EDITOR
                    TextAsset textAsset = null;
                    if (isEditor) textAsset = AssetDatabase.LoadMainAssetAtPath(strPath) as TextAsset;
                    else textAsset = Resources.Load(strPath, typeof(TextAsset)) as TextAsset;
#else
                var textAsset = Resources.Load(strPath, typeof(TextAsset)) as TextAsset;
#endif
                    if (textAsset != null)
                    {
                        result = textAsset.bytes.Clone() as byte[];
                        textAsset = null;
                    }
                    else
                        Debug.LogError(
                            string.Format("[GetFileContent] error : load resouce failed, path is {0}", strPath));
                    break;
                }
            case AssetsSource.eStreamingAssets:
                {
                    var strFilePath = string.Format("{0}/{1}.{2}", streamingAssetsPath, strPath, strExt);
                    result = ReadFileBytes(strFilePath);
                    break;
                }
            case AssetsSource.ePersistentDataPath:
                {
                    var strFilePath = string.Format("{0}/{1}.{2}", persistentDataPath, strPath, strExt);
                    result = ReadFileBytes(strFilePath);
                    break;
                }
            case AssetsSource.eTemporaryCachePath:
                {
                    var strFilePath = string.Format("{0}/{1}.{2}", temporaryCachePath, strPath, strExt);
                    result = ReadFileBytes(strFilePath);
                    break;
                }
            default:
                break;
        }

        return result;
    }

    private byte[] ReadFileBytes(string strFilePath)
    {
        System.IO.FileInfo fileInfo = new System.IO.FileInfo(strFilePath);
        if (!fileInfo.Exists) return null;

        FileStream stream = new FileStream(strFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        byte[] fileBuffer = new byte[stream.Length];
        stream.Read(fileBuffer, 0, Convert.ToInt32(stream.Length));
        stream.Close();
        stream = null;

        return fileBuffer;
    }

    #endregion


    #region get resources path
    // 传入参数 fileName:是Resources下的路径; ext fileName中是否有带扩展名，有的话请填上；如".txt"
    // android 返回 persistentDataPath 的路径；
    public string GetResourcesPath(string fileName, string ext = "")
    {
        string path = "";
        if (fileName.Length > 0)
        {
#if UNITY_ANDROID
        var filePath = string.Format("{0}/{1}", Application.persistentDataPath, fileName);
        var txtAsset = Resources.Load(fileName.Replace(ext, string.Empty), typeof(TextAsset)) as TextAsset;

        if (txtAsset != null)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            var folder = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);

            var fileStream = new FileStream(filePath, FileMode.Create);
            fileStream.Write(txtAsset.bytes, 0, txtAsset.bytes.Length);
            fileStream.Flush();
            fileStream.Close();
        }
        else
        {
            Debug.Log("Error:TextAsset is null! fileName:" + fileName);
        }

        return filePath;
#else
            path = Application.dataPath + "/Resources/" + fileName;
#endif
        }
        return path;
    }
    #endregion
}

