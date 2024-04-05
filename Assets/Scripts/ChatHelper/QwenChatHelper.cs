using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

#region 请求构造体
[Serializable]
public class QwenRequest
{
    public string model;
    public QwenInput input;
    public QwenParameters parameters;
}

[Serializable]
public class QwenInput
{
    public List<ChatContent> messages;
}

[Serializable]
public class QwenParameters
{
    public float top_p;//取值范围为（0,1.0)，取值越大，生成的随机性越高；取值越低，生成的随机性越低。
    public float top_k;//取值越大，生成的随机性越高；取值越小，生成的确定性越高。
    public float repetition_penalty;//可以降低模型生成的重复度。1.0表示不做惩罚。
    public float temperature;//用于控制随机性和多样性的程度。取值范围：[0, 2)
}
#endregion

public class QwenChatHelper : NLPBaseHelper
{
    public QwenChatConfig qwenConfig;

    // private void OnValidate()
    // {
    //     ResetDataList();
    // }

    public override void Init()
    {
        ResetDataList();
    }

    void ResetDataList()
    {
        sendDataList.Clear();
        sendDataList.Add(new ChatContent { role = "system", content = qwenConfig.prompt });
    }

    public override void PostMessage(string message)
    {
        sendDataList.Add(new ChatContent { role = "user", content = message });
        CheckSendLength(qwenConfig.maxHistory);
        StartCoroutine(RequestMessage());
    }

    private IEnumerator RequestMessage()
    {
        using (UnityWebRequest request = new UnityWebRequest(qwenConfig.url, "POST"))
        {
            QwenRequest qwenRequest = new()
            {
                model = qwenConfig.qwenModel,
                input = new QwenInput
                {
                    messages = sendDataList
                },
                parameters = new QwenParameters
                {
                    top_p = 0.8f,
                    top_k = 50f,
                    repetition_penalty = 1.1f,
                    temperature = 0.85f,
                }
            };

            string requestJson = JsonConvert.SerializeObject(qwenRequest);
            Debug.Log(requestJson);
            byte[] data = System.Text.Encoding.UTF8.GetBytes(requestJson);

            request.uploadHandler = new UploadHandlerRaw(data);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            // request.SetRequestHeader("Accept", "text/event-stream");
            request.SetRequestHeader("Authorization", $"Bearer {qwenConfig.api_key}");
            // request.SetRequestHeader("X-DashScope-SSE", "enable");

            // "正在处理中..." 提示

            yield return request.SendWebRequest();

            if (request.responseCode == 200)
            {
                string _msg = request.downloadHandler.text;
                HandlingRespond(_msg);
            }
            else
            {
                Debug.LogError("请求失败: " + request.responseCode);
            }
            // string _msg = request.downloadHandler.text;
        }
    }

    /// <summary>
    /// 处理返回的信息
    /// </summary>
    /// <param name="requestMsg"></param>
    /// <param name="callback"></param> <summary>
    private void HandlingRespond(string requestMsg)
    {
        Debug.Log(requestMsg);

        JObject jsonObj = JObject.Parse(requestMsg);
        string resp = (string)jsonObj["output"]["text"];
        int tokens = (int)jsonObj["usage"]["total_tokens"];

        Debug.Log("请求成功: " + resp);
        Debug.Log($"本次消耗token数: {tokens}");

        sendDataList.Add(new ChatContent { role = "assistant", content = resp });
        onReceiveMessage.Invoke(resp);
    }

}

