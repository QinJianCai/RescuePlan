/************************************************************
** auth：qinjiancai
** date：2023-04-05 11:34:46
** desc：资源路径配置
** Ver：1.0
***********************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefineRes : Singleton<DefineRes>
{
    public static bool IsUseLuaEnv = true;

    /// <summary>
    /// ResourceRootPath
    /// </summary>
    public static string CharacterPrefabRootPath = "Assets/AssetPackage/Prefabs/Character/";
    public static string BuildingPrefabRootPath = "Assets/AssetPackage/Prefabs/Building/";
    public static string EffectPrefabRootPath = "Assets/AssetPackage/Prefabs/Effect/";
    public static string GUIPrefabRootPath = "Assets/AssetPackage/Prefabs/GUI/";
    public static string MapPrefabRootPath = "Assets/AssetPackage/Prefabs/Map/";
    public static string WeaponPrefabRootPath = "Assets/AssetPackage/Prefabs/Weapon/";
    public static string AltaRootPath = "Assets/AssetPackage/GUI/Alta/";
    public static string SoundRootPath = "Assets/AssetPackage/Sounds/";
    public static string MissileRootPath = "Assets/AssetPackage/Prefabs/Missiles/";

    public static string AbCharacter = "character";
    public static string AbBuilding = "building";
    public static string AbEffect = "effect";
    public static string AbGUI = "gui";
    public static string AbWeapon = "weapon";
    public static string AbSound = "sound";
    public static string AbMissile = "missile";

    /// <summary>
    /// 获取资源AB包,注意：图集与地图不能各自打一个ab包，一个地图一个ab包，一个图集一个ab包
    /// </summary>
    /// <param name="ResType"></param>
    /// <returns></returns>
    public static string GetResAb(int ResType)
    {
        switch (ResType)
        {
            case (int)emResType.Character:
                return AbCharacter;
            case (int)emResType.Building:
                return AbBuilding;
            case (int)emResType.Effect:
                return AbEffect;
            case (int)emResType.GUI:
                return AbGUI;
            case (int)emResType.Weapon:
                return AbWeapon;
            case (int)emResType.Sound:
                return AbSound;
            case (int)emResType.Missile:
                return AbMissile;

            default:
                return null;
        }

    }

    /// <summary>
    /// 获取本地资源路径
    /// </summary>
    /// <param name="ResType"></param>
    /// <returns></returns>
    public static string GetResRootPath(int ResType)
    {
        switch (ResType)
        {
            case (int)emResType.Character:
                return CharacterPrefabRootPath;
            case (int)emResType.Building:
                return BuildingPrefabRootPath;
            case (int)emResType.Effect:
                return EffectPrefabRootPath;
            case (int)emResType.GUI:
                return GUIPrefabRootPath;
            case (int)emResType.Map:
                return MapPrefabRootPath;
            case (int)emResType.Weapon:
                return WeaponPrefabRootPath;
            case (int)emResType.Alta:
                return AltaRootPath;
            case (int)emResType.Sound:
                return SoundRootPath;
            case (int)emResType.Missile:
                return MissileRootPath;

            default:
                return null;
        }

    }
}

public enum ENTITY_TYPE
{
    Player,
    Npc,
    City,
    Sect,
    Pick,
    Grave,
    GearEvent,
    Missile,
}

public enum emResType
{
    Character,
    Building,
    Effect,
    GUI,
    Map,
    Weapon,
    Alta,
    Sound,
    Missile,
}

