/********************************************************************************
** auth： qinjiancai
** date： 2022/5/23 16:02:00
** desc： 事件消息分发机制
** Ver.:  V1.0.0
*********************************************************************************/
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public class UtilEvent : Singleton<UtilEvent>
{
    Dictionary<string, Delegate> m_dictDel = new Dictionary<string, Delegate>();

    void OnCheckAddListener(string eventType, Delegate adddel)
    {
        if (!m_dictDel.ContainsKey(eventType))
        {
            m_dictDel.Add(eventType, null);
        }

        Delegate d = m_dictDel[eventType];
        if (d != null && d.GetType() != adddel.GetType())
        {
            Debug.LogError(string.Format("添加事件错误,当前事件{0}, 要添加的事件{1}", d.GetType(), adddel.GetType()));
        }
    }

    bool OnCheckRemoveListener(string eventType, Delegate remdel)
    {
        if (m_dictDel.ContainsKey(eventType) != true)
            return false;

        Delegate d = m_dictDel[eventType];
        if (d != null && d.GetType() != remdel.GetType())
        {
            Debug.LogError(string.Format("移除事件错误，当前事件{0}, 要移除的事件{1}", d.GetType(), remdel.GetType()));
            return false;
        }
        else
            return true;
    }

    void OnListernerRemoved(string eventType)
    {
        if (m_dictDel.ContainsKey(eventType) && m_dictDel[eventType] == null)
            m_dictDel.Remove(eventType);
    }
    public void AddEventListener(string eventType, Action Handler)
    {
        OnCheckAddListener(eventType, Handler);
        m_dictDel[eventType] = (Action)m_dictDel[eventType] + Handler;

    }
    public void AddEventListener<T>(string eventType, Action<T> Handler)
    {
        OnCheckAddListener(eventType, Handler);
        m_dictDel[eventType] = (Action<T>)m_dictDel[eventType] + Handler;
    }

    public void AddEventListener<T, U>(string eventType, Action<T, U> Handler)
    {
        OnCheckAddListener(eventType, Handler);
        m_dictDel[eventType] = (Action<T, U>)m_dictDel[eventType] + Handler;
    }

    public void AddEventListener<T, U, V>(string eventType, Action<T, U, V> Handler)
    {
        OnCheckAddListener(eventType, Handler);
        m_dictDel[eventType] = (Action<T, U, V>)m_dictDel[eventType] + Handler;
    }

    public void AddEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> Handler)
    {
        OnCheckAddListener(eventType, Handler);
        m_dictDel[eventType] = (Action<T, U, V, W>)m_dictDel[eventType] + Handler;
    }

    public void AddEventListener<T, U, V, W, H>(string eventType, Action<T, U, V, W, H> Handler)
    {
        OnCheckAddListener(eventType, Handler);
        m_dictDel[eventType] = (Action<T, U, V, W, H>)m_dictDel[eventType] + Handler;
    }

    public void RemoveEventListener(string eventType, Action Handler)
    {
        if (OnCheckRemoveListener(eventType, Handler))
        {
            m_dictDel[eventType] = (Action)m_dictDel[eventType] - Handler;
            OnListernerRemoved(eventType);
        }
    }

    public void RemoveEventListener<T>(string eventType, Action<T> Handler)
    {
        if (OnCheckRemoveListener(eventType, Handler))
        {
            m_dictDel[eventType] = (Action<T>)m_dictDel[eventType] - Handler;
            OnListernerRemoved(eventType);
        }
    }

    public void RemoveEventListener<T, U>(string eventType, Action<T, U> Handler)
    {
        if (OnCheckRemoveListener(eventType, Handler))
        {
            m_dictDel[eventType] = (Action<T, U>)m_dictDel[eventType] - Handler;
            OnListernerRemoved(eventType);
        }
    }

    public void RemoveEventListener<T, U, V>(string eventType, Action<T, U, V> Handler)
    {
        if (OnCheckRemoveListener(eventType, Handler))
        {
            m_dictDel[eventType] = (Action<T, U, V>)m_dictDel[eventType] - Handler;
            OnListernerRemoved(eventType);
        }
    }

    public void RemoveEventListener<T, U, V, W>(string eventType, Action<T, U, V, W> Handler)
    {
        if (OnCheckRemoveListener(eventType, Handler))
        {
            m_dictDel[eventType] = (Action<T, U, V, W>)m_dictDel[eventType] - Handler;
            OnListernerRemoved(eventType);
        }
    }

    public void RemoveEventListener<T, U, V, W, H>(string eventType, Action<T, U, V, W, H> Handler)
    {
        if (OnCheckRemoveListener(eventType, Handler))
        {
            m_dictDel[eventType] = (Action<T, U, V, W, H>)m_dictDel[eventType] - Handler;
            OnListernerRemoved(eventType);
        }
    }

    public void TriggerEvent(string eventType)
    {
        Delegate d;
        if (!m_dictDel.TryGetValue(eventType, out d))
            return;

        var callback = d.GetInvocationList();
        for (int i = 0; i < callback.Length; i++)
        {
            Action ac = callback[i] as Action;
            if (ac != null)
                ac();
        }
    }

    public void TriggerEvent<T>(string eventType, T arg1)
    {
        Delegate d;
        if (!m_dictDel.TryGetValue(eventType, out d))
            return;

        var callback = d.GetInvocationList();
        for (int i = 0; i < callback.Length; i++)
        {
            Action<T> ac = callback[i] as Action<T>;
            if (ac != null)
            {
                ac(arg1);
            }
            else
            {
                Debug.LogError("Trigger Event Error, eventType:" + eventType);
            }
        }
    }

    public void TriggerEvent<T, U>(string eventType, T arg1, U arg2)
    {
        Delegate d;
        if (!m_dictDel.TryGetValue(eventType, out d))
            return;

        var callback = d.GetInvocationList();
        for (int i = 0; i < callback.Length; i++)
        {
            Action<T, U> ac = callback[i] as Action<T, U>;
            if (ac != null)
            {
                ac(arg1, arg2);
            }
            else
            {
                Debug.LogError("Trigger Event Error, eventType:" + eventType);
            }
        }
    }

    public void TriggerEvent<T, U, V>(string eventType, T arg1, U arg2, V arg3)
    {
        Delegate d;
        if (!m_dictDel.TryGetValue(eventType, out d))
            return;

        var callback = d.GetInvocationList();
        for (int i = 0; i < callback.Length; i++)
        {
            Action<T, U, V> ac = callback[i] as Action<T, U, V>;
            if (ac != null)
            {
                ac(arg1, arg2, arg3);
            }
            else
            {
                Debug.LogError("Trigger Event Error, eventType:" + eventType);
            }
        }
    }

    public void TriggerEvent<T, U, V, W>(string eventType, T arg1, U arg2, V arg3, W arg4)
    {
        Delegate d;
        if (!m_dictDel.TryGetValue(eventType, out d))
            return;

        var callback = d.GetInvocationList();
        for (int i = 0; i < callback.Length; i++)
        {
            Action<T, U, V, W> ac = callback[i] as Action<T, U, V, W>;
            if (ac != null)
            {
                ac(arg1, arg2, arg3, arg4);
            }
            else
            {
                Debug.LogError("Trigger Event Error, eventType:" + eventType);
            }
        }
    }

    public void TriggerEvent<T, U, V, W, H>(string eventType, T arg1, U arg2, V arg3, W arg4, H arg5)
    {
        Delegate d;
        if (!m_dictDel.TryGetValue(eventType, out d))
            return;

        var callback = d.GetInvocationList();
        for (int i = 0; i < callback.Length; i++)
        {
            Action<T, U, V, W, H> ac = callback[i] as Action<T, U, V, W, H>;
            if (ac != null)
            {
                ac(arg1, arg2, arg3, arg4, arg5);
            }
            else
            {
                Debug.LogError("Trigger Event Error, eventType:" + eventType);
            }
        }
    }
}