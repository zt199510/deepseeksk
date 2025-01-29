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
    // ��������
    public const string KmsIdTag = "kmsid";
    public const string KmsIndex = "kms";
    public const string KmsSearchNull = "֪ʶ��δ�������������";

}

public class KMService
{
    public MemoryServerless _memory;

    // ����������MemoryServerlessʵ��
    public MemoryServerless CreateMemoryByApp()
    {
        // ���������ͻ���
        var searchClientConfig = new SearchClientConfig
        {
            MaxAskPromptSize = 2048, // �������token��
            MaxMatchesCount = 3, // ����ƥ����
            AnswerTokens = 2048, // �ش����token��
            EmptyAnswer = KmsConstantcs.KmsSearchNull
        };

        // ��ȡHTTP�ͻ���
        var chatHttpClient = OpenAIHttpClientHandlerUtil.GetHttpClient("http://abc.ztgametv.cn:10086/");
        var embeddingHttpClient = OpenAIHttpClientHandlerUtil.GetHttpClient("http://abc.ztgametv.cn:10086/");

        // ����KernelMemoryʵ��
        var memoryBuild = new KernelMemoryBuilder()
            .WithSearchClientConfig(searchClientConfig)
            .WithOpenAITextGeneration(new OpenAIConfig
            {
                APIKey = "sk-uxaC405uujdT3CSS828dC75fF89b49B09dFa9a76B13667E3",
                TextModel = "gpt-4o-mini" // ʹ����ȷ�Ĳ�������ģ��ID
            }, null, chatHttpClient)
            .WithOpenAITextEmbeddingGeneration(new OpenAIConfig
            {
                APIKey = "sk-z557jeuF1sNO4Gn6A697895c04A74c76BfFd0e624d218299",
                EmbeddingModel = "text-embedding-3-small" // ʹ����ȷ�Ĳ�����
            }, null, false, embeddingHttpClient)
            .WithSimpleFileStorage(new SimpleFileStorageConfig
            {
                StorageType = FileSystemTypes.Disk,
                Directory = AppDomain.CurrentDomain.BaseDirectory + "test1" // ָ���洢·��
            })
            .WithSimpleVectorDb(new SimpleVectorDbConfig
            {
                StorageType = FileSystemTypes.Disk,
                Directory = AppDomain.CurrentDomain.BaseDirectory + "test2" // ��ѡ��ָ�������洢·��
            });

        // ����MemoryServerlessʵ��
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