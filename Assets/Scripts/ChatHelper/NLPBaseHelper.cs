using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class NLPBaseHelper : MonoBehaviour
{
    /// <summary>
    /// 发送的信息list
    /// </summary> 
    public List<ChatContent> sendDataList = new();
    public UnityEvent<string> onReceiveMessage;

    /// <summary>
    /// 初始化模型配置
    /// </summary>
    public abstract void Init();
    public abstract void PostMessage(string message);

    // public abstract IEnumerator RequestMessage(Action<string> callback);

    /// <summary>
    /// 将发生的信息限制在maxHistory长度内
    /// </summary>
    /// <param name="maxHistory"></param>
    protected void CheckSendLength(int maxHistory)
    {
        if (maxHistory < 0)
            return;

        while (sendDataList.Count > maxHistory)
        {
            sendDataList.RemoveAt(1);
        }
    }
}

[Serializable]
public class ChatContent
{
    public string role;
    public string content;

    public ChatContent(string r, string c)
    {
        role = r;
        content = c;
    }

    public ChatContent() { }
}
