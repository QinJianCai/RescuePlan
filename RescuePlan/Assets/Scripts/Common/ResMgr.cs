/************************************************************
** auth：qinjiancai
** date：2022-09-14 09:17:31
** desc：资源加载相关封装
** Ver：1.0
***********************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using UnityEditor;
using UnityEngine.SceneManagement;


public class ResMgr : UnitySingleton<ResMgr>
{
    /// <summary>
    /// 主包
    /// </summary>
    private AssetBundle _MainAb = null;
    /// <summary>
    /// 依赖包配置文件
    /// </summary>
    private AssetBundleManifest _Manifest = null;

    private Dictionary<string, AssetBundle> _AbDic = new Dictionary<string, AssetBundle>();
    private Dictionary<string, GameObject> _AssetDic = new Dictionary<string, GameObject>();
    private Dictionary<string, Object[]> _AltasDic = new Dictionary<string, Object[]>();

    AsyncOperation op = null;
    /// <summary>
    /// Ab包存放路径
    /// </summary>
    private string AbPath
    {
        get {
            return Application.streamingAssetsPath + "/";
        }
    }

    private string MainAbName
    {
        get
        {
            return "WinAB";
        }
    }

    /// <summary>
    /// 加载场景
    /// </summary>
    /// <param name="sceneId"></param>
    /// <param name="ac"></param>
    public void LoadScene(int sceneId, UnityAction<int> ac)
    {
        StartCoroutine(DoLoadScene(sceneId, ac));
    }

    IEnumerator DoLoadScene(int sceneId, UnityAction<int> ac)
    {
        op = SceneManager.LoadSceneAsync(sceneId, LoadSceneMode.Single);
        yield return op;

        if (op != null && op.isDone)
        {
            op = null;
            if (ac != null)
                ac(sceneId);
        }
    }

    /// <summary>
    /// 同步加载角色资源
    /// </summary>
    /// <param name="ResName"></param>
    /// <param name="ResType">emResType</param>
    /// <param name="cb"></param>
    public void LoadPrefab(string ResName, int ResType, UnityAction<GameObject> cb)
    {
#if UNITY_EDITOR
        Object obj = LoadRes(DefineRes.GetResRootPath(ResType), ResName + ".prefab");
        if (cb != null)
            cb(obj as GameObject);
#else
        Object obj = LoadRes(DefineRes.GetResAb(ResType), ResName);
        if (cb != null)
            cb(obj as GameObject);
#endif
    }

    /// <summary>
    /// 异步加载地图预设资源
    /// </summary>
    /// <param name="ResName"></param>
    /// <param name="ResType">emResType</param>
    /// <param name="cb"></param>
    public void LoadMapPrefab(string ResName, UnityAction<GameObject> cb)
    {
#if UNITY_EDITOR
        LoadResAsync<GameObject>(DefineRes.GetResRootPath((int)emResType.Map), ResName + ".prefab", cb);
#else
        LoadResAsync<GameObject>(ResName, ResName, cb);
#endif
    }

    /// <summary>
    /// 异步加载角色资源
    /// </summary>
    /// <param name="ResName"></param>
    /// <param name="cb"></param>
    public void LoadPrefabSync(string ResName, int ResType, UnityAction<GameObject> cb)
    {
#if UNITY_EDITOR
        LoadResAsync(DefineRes.GetResRootPath(ResType), ResName + ".prefab", cb);
#else
        LoadResAsync(DefineRes.GetResAb(ResType), ResName, cb);
#endif
    }

    /// <summary>
    /// 同步加载图集资源
    /// </summary>
    /// <param name="ResName"></param>
    /// <param name="ResType">emResType</param>
    /// <param name="cb"></param>
    public void LoadAltaElement(string AltaName, string elementName, UnityAction<Sprite> cb)
    {
        if (cb == null)
            return;

        bool IsSuccess = false;
        if (_AltasDic.ContainsKey(AltaName))
        {
            for (int i = 0; i < _AltasDic[AltaName].Length; i++)
            {
                if (_AltasDic[AltaName][i].name == elementName)
                {
                    cb( GameObject.Instantiate(_AltasDic[AltaName][i]) as Sprite);
                    IsSuccess = true;
                }
            }
        }
        else
        {
#if UNITY_EDITOR
            Object[] sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(DefineRes.AltaRootPath + AltaName + ".png");
            if (sprites != null)
            {
                _AltasDic[AltaName] = sprites;
                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i].name == elementName)
                    {
                        cb(GameObject.Instantiate(sprites[i]) as Sprite);
                        IsSuccess = true;
                    }
                }
            }
#else
            LoadAB(AltaName);
            if (_AbDic.ContainsKey(AltaName))
            {
                Object[] spritesObj = _AbDic[AltaName].LoadAllAssets();
                if (spritesObj != null && spritesObj.Length > 0)
                {
                    _AltasDic[AltaName] = spritesObj;
                    for (int i = 0; i < spritesObj.Length; i++)
                    {
                        if (spritesObj[i].name == elementName)
                        {
                            cb(GameObject.Instantiate(spritesObj[i]) as Sprite);
                            IsSuccess = true;
                        }
                    }
                }
            }
#endif  
        }

        if (!IsSuccess)
            cb(null);
    }


    /// <summary>
    /// 异步加载图集资源
    /// </summary>
    /// <param name="AltaName"></param>
    /// <param name="elementName"></param>
    /// <param name="cb"></param>
    public void LoadAltaElementSync(string AltaName, string elementName, UnityAction<Sprite> cb)
    {
        if (cb == null)
            return;

        StartCoroutine(DoLoadAltaElement(AltaName, elementName, cb));
    }
    /// <summary>
    /// 异步加载图集资源
    /// </summary>
    /// <param name="ResName"></param>
    /// <param name="ResType">emResType</param>
    /// <param name="cb"></param>
    IEnumerator DoLoadAltaElement(string AltaName, string elementName, UnityAction<Sprite> cb)
    {
        bool IsSuccess = false;
        if (_AltasDic.ContainsKey(AltaName))
        {
            for (int i = 0; i < _AltasDic[AltaName].Length; i++)
            {
                if (_AltasDic[AltaName][i].name == elementName)
                {
                    cb(GameObject.Instantiate(_AltasDic[AltaName][i]) as Sprite);
                    IsSuccess = true;
                }
            }
        }
        else
        {
#if UNITY_EDITOR
            Object[] sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(DefineRes.AltaRootPath + AltaName + ".png");
            yield return sprites;
            if (sprites != null)
            {
                _AltasDic[AltaName] = sprites;
                for (int i = 0; i < sprites.Length; i++)
                {
                    if (sprites[i].name == elementName)
                    {
                        cb(GameObject.Instantiate(sprites[i]) as Sprite);
                        IsSuccess = true;
                    }
                }
            }
#else
            //加载AB包
            LoadAB(AltaName);
            if (_AbDic.ContainsKey(AltaName))
            {
                AssetBundleRequest abr = _AbDic[AltaName].LoadAllAssetsAsync();
                yield return abr;

                if (abr.allAssets != null && abr.allAssets.Length > 0)
                {

                    _AltasDic[AltaName] = abr.allAssets;
                    for (int i = 0; i < abr.allAssets.Length; i++)
                    {
                        if (abr.allAssets[i].name == elementName)
                        {
                            cb(GameObject.Instantiate(abr.allAssets[i]) as Sprite);
                            IsSuccess = true;
                        }
                    }
                }
            }
#endif
        }

        if (!IsSuccess)
            cb(null);

        yield return null;
    }

    /// <summary>
    /// 加载音效资源
    /// </summary>
    /// <param name="ResName"></param>
    /// <param name="cb"></param>
    public void LoadSound(string ResName, UnityAction<AudioClip> cb)
    {
#if UNITY_EDITOR
        LoadResAsync<AudioClip>(DefineRes.GetResRootPath((int)emResType.Sound), ResName, cb);
#else
        LoadResAsync<AudioClip>(DefineRes.GetResAb((int)emResType.Sound), ResName,cb);
#endif
    }


    /// <summary>
    /// 加载AB包
    /// </summary>
    /// <param name="AbName"></param>
    public void LoadAB(string AbName)
    {
        if (_AbDic.ContainsKey(AbName))
        {
            return;
        }

        if (_MainAb == null)
        {
            _MainAb = AssetBundle.LoadFromFile(AbPath + MainAbName);
            _Manifest = _MainAb.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        }

        AssetBundle ab = null;
        string[] dependence = _Manifest.GetAllDependencies(AbName);
        for (int i = 0; i < dependence.Length; i++)
        {
            if (!_AbDic.ContainsKey(dependence[i]))
            {
                ab = AssetBundle.LoadFromFile(AbPath + dependence[i]);
                _AbDic.Add(dependence[i], ab);
            }
        }

        if (!_AbDic.ContainsKey(AbName))
        { 
            ab = AssetBundle.LoadFromFile(AbPath + AbName);
            _AbDic.Add(AbName, ab);
        }
    }

    /// <summary>
    /// 加载本地资源，指定类型
    /// </summary>
    /// <param name="ResPath"></param>
    /// <param name="ResName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    private Object LoadLocalRes(string ResPath, string ResName, System.Type type)
    {
#if UNITY_EDITOR
        string path = ResPath + ResName;
        if (_AssetDic.ContainsKey(path))
            return GameObject.Instantiate(_AssetDic[path]);
        else
        {
            Object target = UnityEditor.AssetDatabase.LoadAssetAtPath(path, type);
            if (target != null)
            {
                _AssetDic.Add(path, (target as GameObject));
                return GameObject.Instantiate(target);
            }
            else
                return null;
        }
#endif
        return null;
    }

    /// <summary>
    /// 加载本地资源，泛型指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ResPath"></param>
    /// <param name="ResName"></param>
    /// <returns></returns>
    private T LoadLocalRes<T>(string ResPath, string ResName) where T:Object
    {
#if UNITY_EDITOR
        string path = ResPath + ResName;
        if (_AssetDic.ContainsKey(path))
            return GameObject.Instantiate(_AssetDic[path]) as T;
        else
        {
            T target = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
            if (target != null)
            {
                _AssetDic.Add(path, (target as GameObject));
                return GameObject.Instantiate(target) as T;
            }
            else
                return null;
        }
#endif
        return null;
    }


    /// <summary>
    /// 同步加载，不指定类型
    /// </summary>
    /// <param name="ResPath">资源路径或AB资源名</param>
    /// <param name="ResName"></param>
    /// <returns></returns>
    public Object LoadRes(string ResPath, string ResName)
    {
#if UNITY_EDITOR
        Object objRes = LoadLocalRes<Object>(ResPath, ResName);
        return objRes;
#endif
        //加载AB包
        LoadAB(ResPath);

        Object obj = _AbDic[ResPath].LoadAsset(ResName);
        if (obj is GameObject)
        {
            return GameObject.Instantiate(obj);
        }
        else
            return obj;
    }

    /// <summary>
    /// 同步加载，指定类型
    /// </summary>
    /// <param name="ResPath">资源路径或AB资源名</param>
    /// <param name="ResName"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public Object LoadRes(string ResPath, string ResName, System.Type type)
    {
#if UNITY_EDITOR
        Object objRes = LoadLocalRes(ResPath, ResName, type);
        return objRes;
#endif
        //加载AB包
        LoadAB(ResPath);

        Object obj = _AbDic[ResPath].LoadAsset(ResName, type);
        if (obj is GameObject)
        {
            return GameObject.Instantiate(obj);
        }
        else
            return obj;
    }

    /// <summary>
    /// 同步加载，泛型指定类型
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ResPath">资源路径或AB资源名</param>
    /// <param name="ResName"></param>
    /// <returns></returns>
    public T LoadRes<T>(string ResPath, string ResName) where T:Object
    {
#if UNITY_EDITOR
        T objRes = LoadLocalRes<T>(ResPath, ResName);
        return objRes;
#endif
        //加载AB包
        LoadAB(ResPath);

        T obj = _AbDic[ResPath].LoadAsset<T>(ResName);
        if (obj is GameObject)
        {
            return GameObject.Instantiate(obj);
        }
        else
            return obj;
    }

    /// <summary>
    /// 异步加载资源，不指定类型
    /// </summary>
    /// <param name="ResPath">资源路径或AB资源名</param>
    /// <param name="ResName"></param>
    /// <param name="CB"></param>
    public void LoadResAsync(string ResPath, string ResName, UnityAction<Object> CB)
    {
        StartCoroutine(DoLoadResAsync(ResPath, ResName, CB));
    }

    private IEnumerator DoLoadResAsync(string ResPath, string ResName, UnityAction<Object> CB)
    {
#if UNITY_EDITOR
        Object objRes = LoadLocalRes<Object>(ResPath, ResName);
        yield return objRes;

        CB(objRes);
#else
        //加载AB包
        LoadAB(ResPath);

        AssetBundleRequest abr = _AbDic[ResPath].LoadAssetAsync(ResName);
        yield return abr;

        if (abr.asset is GameObject)
        {
            CB(GameObject.Instantiate(abr.asset));
        }
        else
            CB(abr.asset);
#endif
    }

    /// <summary>
    /// 异步加载资源，指定类型
    /// </summary>
    /// <param name="ResPath">异步加载资源，指定类型</param>
    /// <param name="ResName"></param>
    /// <param name="CB"></param>
    public void LoadResAsync(string ResPath, string ResName, System.Type type, UnityAction<Object> CB)
    {
        StartCoroutine(DoLoadResAsync(ResPath, ResName, type, CB));
    }

    private IEnumerator DoLoadResAsync(string ResPath, string ResName, System.Type type, UnityAction<Object> CB)
    {
#if UNITY_EDITOR
        Object objRes = LoadLocalRes(ResPath, ResName, type);
        yield return objRes;

        CB(objRes);
#else
        //加载AB包
        LoadAB(ResPath);

        AssetBundleRequest abr = _AbDic[ResPath].LoadAssetAsync(ResName, type);
        yield return abr;

        if (abr.asset is GameObject)
        {
            CB(GameObject.Instantiate(abr.asset));
        }
        else
            CB(abr.asset);
#endif
    }

    /// <summary>
    /// 异步加载资源，泛型指定类型
    /// </summary>
    /// <param name="ResPath">异步加载资源，指定类型</param>
    /// <param name="ResName"></param>
    /// <param name="CB"></param>
    public void LoadResAsync<T>(string ResPath, string ResName, UnityAction<T> CB) where T:Object
    {
        StartCoroutine(DoLoadResAsync(ResPath, ResName, CB));
    }

    private IEnumerator DoLoadResAsync<T>(string ResPath, string ResName, UnityAction<T> CB) where T : Object
    {
#if UNITY_EDITOR
        T objRes = LoadLocalRes<T>(ResPath, ResName);
        yield return objRes;

        CB(objRes);
#else
        //加载AB包
        LoadAB(ResPath);

        AssetBundleRequest abr = _AbDic[ResPath].LoadAssetAsync<T>(ResName);
        yield return abr;

        if (abr.asset is GameObject)
        {
            CB(GameObject.Instantiate(abr.asset) as T);
        }
        else
            CB(abr.asset as T);
#endif
    }

    /// <summary>
    /// 卸载单个资源
    /// </summary>
    /// <param name="ResName"></param>
    public void UnLoad(string ResName)
    {
        if (_AbDic.ContainsKey(ResName))
        {
            _AbDic[ResName].Unload(false);
            _AbDic.Remove(ResName);
        }

        if (_AssetDic.ContainsKey(ResName))
            _AssetDic.Remove(ResName);
    }

    public void UnLoadAllAB()
    {
        AssetBundle.UnloadAllAssetBundles(false);
        _AssetDic.Clear();
        _AbDic.Clear();
        _MainAb = null;
        _Manifest = null;
    }
}

