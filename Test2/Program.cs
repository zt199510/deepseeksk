// See https://aka.ms/new-console-template for more information
using DeepseekRAG.Model;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.MemoryStorage;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// 假设 fileid 和 req 是从某个地方获取的
string fileid = "exampleFileId"; // 示例 fileid
var req = new
{
    FilePath = @"C:\Users\Administrator\Desktop\溺水防范告家长书.txt", // 示例文件路径
    KmsId = KmsConstantcs.KmsIdTag // 示例 KmsId
};

KMService kMService = new KMService();

// 创建MemoryServerless实例
MemoryServerless memoryServerless = kMService.CreateMemoryByApp();

// 导入文档
var importResult = await memoryServerless.ImportDocumentAsync(new Document(fileid).AddFile(req.FilePath)
                               .AddTag(KmsConstantcs.KmsIdTag, req.KmsId)
                           , index: KmsConstantcs.KmsIndex);
if (importResult=="")
{
    Console.WriteLine("Document imported successfully.");
}
else
{
    Console.WriteLine("Document import failed.");
}
// 获取所有索引
var memories = await kMService._memory.ListIndexesAsync();
var memoryDbs = kMService._memory.Orchestrator.GetMemoryDbs();
var docTextList = new List<KMFile>();

// 遍历所有索引和数据库，获取文档信息
foreach (var memoryIndex in memories)
{
    foreach (var memoryDb in memoryDbs)
    {
        var items = await memoryDb.GetListAsync(memoryIndex.Name, new List<MemoryFilter>() { new MemoryFilter().ByDocument(fileid) }, 1000, true).ToListAsync();
        docTextList.AddRange(items.Select(item => new KMFile()
        {
            DocumentId = item.GetDocumentId(),
            Text = item.GetPartitionText(),
            Url = item.GetWebPageUrl(KmsConstantcs.KmsIndex),
            LastUpdate = item.GetLastUpdate().LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
            File = item.GetFileName()
        }));
    }
}

// 输出MemoryServerless实例信息
Console.WriteLine(memoryServerless);

var searchResult = await kMService.GetRelevantSourceList("溺水防范", KmsConstantcs.KmsIdTag);
if (searchResult.Any())
{
    foreach (var result in searchResult)
    {
        Console.WriteLine($"相关度: {result.Relevance}");
        Console.WriteLine($"文档ID: {result.SourceName}");
        Console.WriteLine($"内容片段: {result.Text}");
        Console.WriteLine("-------------------");
    }
}
else
{
    Console.WriteLine("No relevant sources found.");
}