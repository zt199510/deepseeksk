using DeepseekRAG;
using DeepseekRAG.Model;
using Google.Protobuf.WellKnownTypes;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.DocumentStorage.DevTools;
using Microsoft.KernelMemory.FileSystem.DevTools;
using Microsoft.KernelMemory.MemoryStorage.DevTools;
using System;

public class KmsConstantcs
{
    // 常量定义
    public const string KmsIdTag = "kmsid";
    public const string KmsIndex = "kms";
    public const string KmsSearchNull = "知识库未搜索到相关内容";

}

public class KMService
{
    public MemoryServerless _memory;

    // 创建并配置MemoryServerless实例
    public MemoryServerless CreateMemoryByApp()
    {
        // 配置搜索客户端
        var searchClientConfig = new SearchClientConfig
        {
            MaxAskPromptSize = 2048, // 提问最大token数
            MaxMatchesCount = 3, // 向量匹配数
            AnswerTokens = 2048, // 回答最大token数
            EmptyAnswer = KmsConstantcs.KmsSearchNull
        };

        // 获取HTTP客户端
        var chatHttpClient = OpenAIHttpClientHandlerUtil.GetHttpClient("http://abc.ztgametv.cn:10086/");
        var embeddingHttpClient = OpenAIHttpClientHandlerUtil.GetHttpClient("http://abc.ztgametv.cn:10086/");

        // 构建KernelMemory实例
        var memoryBuild = new KernelMemoryBuilder()
            .WithSearchClientConfig(searchClientConfig)
            .WithOpenAITextGeneration(new OpenAIConfig
            {
                APIKey = "sk-uxaC405uujdT3CSS828dC75fF89b49B09dFa9a76B13667E3",
                TextModel = "gpt-4o-mini" // 使用正确的参数名和模型ID
            }, null, chatHttpClient)
            .WithOpenAITextEmbeddingGeneration(new OpenAIConfig
            {
                APIKey = "sk-z557jeuF1sNO4Gn6A697895c04A74c76BfFd0e624d218299",
                EmbeddingModel = "text-embedding-3-small" // 使用正确的参数名
            }, null, false, embeddingHttpClient)
            .WithSimpleFileStorage(new SimpleFileStorageConfig
            {
                StorageType = FileSystemTypes.Disk,
                Directory = AppDomain.CurrentDomain.BaseDirectory + "test1" // 指定存储路径
            })
            .WithSimpleVectorDb(new SimpleVectorDbConfig
            {
                StorageType = FileSystemTypes.Disk,
                Directory = AppDomain.CurrentDomain.BaseDirectory + "test2" // 可选，指定向量存储路径
            });

        // 构建MemoryServerless实例
        _memory = memoryBuild.Build<MemoryServerless>();

        return _memory;
    }


    public async Task<List<RelevantSource>> GetRelevantSourceList(string msg,string KmsIdList)
    {
        var result = new List<RelevantSource>();
        var kmsIdList = KmsIdList.Split(",");
        if (!kmsIdList.Any()) return result;
        var filters = kmsIdList.Select(kmsId => new MemoryFilter().ByTag(KmsConstantcs.KmsIdTag, kmsId)).ToList();
        var searchResult = await _memory.SearchAsync(msg, index: KmsConstantcs.KmsIndex, filters: filters);
        if (!searchResult.NoResult)
        {
            foreach (var item in searchResult.Results)
            {
                result.AddRange(item.Partitions.Select(part => new RelevantSource()
                {
                    SourceName = item.SourceName,
                    Text = part.Text,
                    Relevance = part.Relevance
                }));
            }
        }
        return result;
    }
}