public static class AliCloudSDK
{
    /**
     * 使用AK&SK初始化账号Client
     * @param accessKeyId
     * @param accessKeySecret
     * @return Client
     * @throws Exception
     */
    public static AlibabaCloud.OpenApiClient.Client CreateClient(string accessKeyId, string accessKeySecret)
    {
        AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
        {
            // 必填，您的 AccessKey ID
            AccessKeyId = accessKeyId,
            // 必填，您的 AccessKey Secret
            AccessKeySecret = accessKeySecret,
        };
        // Endpoint 请参考 https://api.aliyun.com/product/nlp-automl
        config.Endpoint = "nlp-automl.cn-hangzhou.aliyuncs.com";
        return new AlibabaCloud.OpenApiClient.Client(config);
    }

    /**
     * API 相关
     * @param path params
     * @return OpenApi.Params
     */
    public static AlibabaCloud.OpenApiClient.Models.Params CreateApiInfo()
    {
        AlibabaCloud.OpenApiClient.Models.Params params_ = new AlibabaCloud.OpenApiClient.Models.Params
        {
            // 接口名称
            Action = "RunPreTrainService",
            // 接口版本
            Version = "2019-11-11",
            // 接口协议
            Protocol = "HTTPS",
            // 接口 HTTP 方法
            Method = "POST",
            AuthType = "AK",
            Style = "RPC",
            // 接口 PATH
            Pathname = "/",
            // 接口请求体内容格式
            ReqBodyType = "formData",
            // 接口响应体内容格式
            BodyType = "json",
        };
        return params_;
    }
}
