using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class EmotionAnalyzeAli : MonoBehaviour
{
    public AliEmotionAnalyzeConfig config;
    class EmotionParam
    {
        public float score;
        public string key;
    }

    public (string emotion, string allEmotionJson) GetEmotion(string context)
    {
        if (config == null)
        {
            Debug.LogError("请先配置阿里云情绪识别配置文件");
            return ("未识别到阿里云情绪识别配置文件", "未识别到阿里云情绪识别配置文件");
        }

        // 请确保代码运行环境设置了环境变量 ALIBABA_CLOUD_ACCESS_KEY_ID 和 ALIBABA_CLOUD_ACCESS_KEY_SECRET。
        // 工程代码泄露可能会导致 AccessKey 泄露，并威胁账号下所有资源的安全性。以下代码示例使用环境变量获取 AccessKey 的方式进行调用，仅供参考，建议使用更安全的 STS 方式，更多鉴权访问方式请参见：https://help.aliyun.com/document_detail/378671.html
        AlibabaCloud.OpenApiClient.Client client = AliCloudSDK.CreateClient(config.alibabaKey, config.alibabaSecret);
        AlibabaCloud.OpenApiClient.Models.Params params_ = AliCloudSDK.CreateApiInfo();
        // body params
        Dictionary<string, object> body = new Dictionary<string, object>() { };
        body["ServiceName"] = "DeepEmotion";
        body["ServiceVersion"] = "V1";
        body["PredictContent"] = "{\"input\":{\"content\": \"" + context + "\"}}";
        body["Product"] = null;

        // runtime options
        AlibabaCloud.TeaUtil.Models.RuntimeOptions runtime = new();
        AlibabaCloud.OpenApiClient.Models.OpenApiRequest request = new()
        {
            Body = body,
        };
        // 复制代码运行请自行打印 API 的返回值
        // 返回值为 Map 类型，可从 Map 中获得三类数据：响应体 body、响应头 headers、HTTP 返回的状态码 statusCode。
        var response = client.CallApi(params_, request, runtime);

        string output = (response["body"] as Dictionary<string, object>)["PredictResult"].ToString();
        // Debug.Log(output);
        JObject outputJobj = JObject.Parse(output);

        if ((int)outputJobj["code"] == 0)
        {
            string outputJson = outputJobj["output"]["sentiment"].ToString();
            // Debug.Log(outputJson);
            List<EmotionParam> emotionScores = JsonConvert.DeserializeObject<List<EmotionParam>>(outputJson);
            emotionScores.Sort((a, b) => b.score.CompareTo(a.score));
            return (emotionScores[0].key, outputJson);
        }
        else
        {
            Debug.LogError($"情绪识别请求失败: 错误代码{outputJobj["code"]}");
            return ($"情绪识别请求失败: 错误代码{outputJobj["code"]}", $"情绪识别请求失败: 错误代码{outputJobj["code"]}");
        }


        // 遍历response字典，并log出来
        // foreach (var item in response)
        // {
        //     Debug.Log(item.Key + " : \n" + item.Value);
        // }
    }


}