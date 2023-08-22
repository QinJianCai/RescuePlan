/************************************************************
** auth：qinjiancai
** date：2022-06-15 16:33:12
** desc：计时器工具功能
** Ver：1.0
***********************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void CompleteEvent();
public delegate void UpdateEvent(float t);

public class Timer : MonoBehaviour
{
    UpdateEvent updateEvent;
    CompleteEvent onCompleted;
    bool isLog = true;              //是否打印消息
    float time;                     // 计时时间/
    float timeStart;                // 开始计时时间/
    float offsetTime;               // 计时偏差/
    bool isStartTimer;              // 是否开始计时/
    bool isDestory = true;          // 计时结束后是否销毁/
    bool isEnd = false;             // 计时是否结束/
    bool isIgnoreTimeScale = true;  // 是否忽略时间速率
    bool isRepeate;                 //是否重复
    float now;                      //当前时间 正计时
    float downNow;                  //倒计时
    bool isCountDown = false;       //是否是倒计时

    bool isCompleted = false;
    ///是否使用游戏的真实时间 不依赖游戏的时间速度
    float TimeNow
    {
        get { return isIgnoreTimeScale ? Time.realtimeSinceStartup : Time.time; }
    }

    /// <summary>
    /// 创建计时器:名字  根据名字可以创建多个计时器对象
    /// </summary>
    public static Timer createTimer(string gobjName = "Timer")
    {
        GameObject g = new GameObject(gobjName);
        Timer timer = g.AddComponent<Timer>();
        return timer;
    }

    private void OnDestroy()
    {
        if (!isCompleted)
        {
            if (onCompleted != null)
                onCompleted();
        }
    }

    /// <summary>
    /// 开始计时
    /// </summary>
    /// <param name="time">目标时间</param>
    /// <param name="isCountDown">是否是倒计时</param>
    /// <param name="onCompletedCB">完成回调函数</param>
    /// <param name="updateCB">计时器进程回调函数</param>
    /// <param name="isIgnoreTimeScale">是否忽略时间倍数</param>
    /// <param name="isRepeate">是否重复</param>
    /// <param name="isDestory">完成后是否销毁</param>
    public void startTimer(float time, bool isCountDown = false,
        CompleteEvent onCompletedCB = null, UpdateEvent updateCB = null,
        bool isIgnoreTimeScale = true, bool isRepeate = false, bool isDestory = true,
        float offsetTime = 0)
    {
        this.time = time;
        this.isIgnoreTimeScale = isIgnoreTimeScale;
        this.isRepeate = isRepeate;
        this.isDestory = isDestory;
        this.offsetTime = offsetTime;
        this.isEnd = false;
        this.isStartTimer = true;
        this.isCountDown = isCountDown;
        timeStart = TimeNow;

        if (onCompletedCB != null)
            onCompleted = onCompletedCB;
        if (updateCB != null)
            updateEvent = updateCB;
    }

    void Update()
    {
        if (isStartTimer)
        {
            now = TimeNow - offsetTime - timeStart;
            downNow = time - now;
            if (updateEvent != null)
            {
                if (isCountDown)
                {
                    updateEvent(downNow);
                }
                else
                {
                    updateEvent(now);
                }
            }
            if (now > time)
            {
                isCompleted = true;
                if (onCompleted != null)
                    onCompleted();
                if (!isRepeate)
                    destory();
                else
                    reStartTimer();
            }
        }
    }

    /// <summary>
    /// 获取剩余时间
    /// </summary>
    /// <returns></returns>
    public float GetTimeNow()
    {
        return Mathf.Clamp(time - now, 0, time);
    }

    /// <summary>
    /// 计时结束
    /// </summary>
    public void destory()
    {
        isStartTimer = false;
        isEnd = true;
        if (isDestory)
            Destroy(gameObject);
    }

    float _pauseTime;
    /// <summary>
    /// 暂停计时
    /// </summary>
    public void pauseTimer()
    {
        if (isEnd)
        {
            if (isLog) Debug.LogWarning("Timer is end！");
        }
        else
        {
            if (isStartTimer)
            {
                isStartTimer = false;
                _pauseTime = TimeNow;
            }
        }
    }

    /// <summary>
    /// 继续计时
    /// </summary>
    public void connitueTimer()
    {
        if (isEnd)
        {
            if (isLog) Debug.LogWarning("Timer is end！Restart the Timer Please！");
        }
        else
        {
            if (!isStartTimer)
            {
                offsetTime += (TimeNow - _pauseTime);
                isStartTimer = true;
            }
        }
    }

    /// <summary>
    /// 重新计时
    /// </summary>
    public void reStartTimer()
    {
        timeStart = TimeNow;
        offsetTime = 0;
    }

    /// <summary>
    /// 更改目标时间
    /// </summary>
    /// <param name="time_"></param>
    public void changeTargetTime(float nTargetTime)
    {
        time = nTargetTime;
        timeStart = TimeNow;
    }


    /// <summary>
    /// 游戏暂停调用
    /// </summary>
    /// <param name="isPause_"></param>
    void OnApplicationPause(bool isPause)
    {
        if (isPause)
        {
            pauseTimer();
        }
        else
        {
            connitueTimer();
        }
    }
}
