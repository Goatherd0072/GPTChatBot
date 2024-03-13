using UnityEngine;

[CreateAssetMenu(fileName = "SparkChatConfig", menuName = "NLPConfig/SparkChatConfig")]
public class SparkChatConfig : NLPConfig
{
    SparkChatConfig()
    {
        prompt = "使用中文回答,并移除回答中的Markdown标示";
        modelName = "星火";
    }

    /// <summary>
    /// 星火大模型
    /// https://www.xfyun.cn/doc/spark/Web.html#_1-%E6%8E%A5%E5%8F%A3%E8%AF%B4%E6%98%8E
    /// </summary>

    // 应用APPID（必须为webapi类型应用，并开通星火认知大模型授权）
    public string x_appid;

    // 接口密钥（webapi类型应用开通星火认知大模型后，控制台--我的应用---星火认知大模型---相应服务的apikey）
    public string api_secret;

    // 接口密钥（webapi类型应用开通星火认知大模型后，控制台--我的应用---星火认知大模型---相应服务的apisecret）
    public string api_key;

    /// <summary>
    /// api URL
    /// </summary>
    public string xfUrl = "wss://spark-api.xf-yun.com/v3.5/chat";
    public string xfDomain = "generalv3.5";

    void OnValidate()
    {

    }
}
