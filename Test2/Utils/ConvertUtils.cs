using Microsoft.KernelMemory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UglyToad.PdfPig.Logging;

namespace DeepseekRAG.Utils;

public static class ConvertUtils
{

    /// <summary>
    /// 判断是否为空，为空返回true
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static bool IsNull(this object data)
    {
        //如果为null
        if (data == null)
        {
            return true;
        }

        //如果为""
        if (data.GetType() == typeof(String))
        {
            if (string.IsNullOrEmpty(data.ToString().Trim()))
            {
                return true;
            }
        }
        return false;
    }
    /// <summary>
    /// object 转int32
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static Int32 ConvertToInt32(this object s)
    {
        int i = 0;
        if (s == null)
        {
            return 0;
        }
        else
        {
            int.TryParse(s.ToString(), out i);
        }
        return i;
    }

    /// <summary>
    /// 是否为流式请求
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static bool IsStream(this string value)
    {
        // 正则表达式忽略空格的情况
        string pattern = @"\s*""stream""\s*:\s*true\s*";

        // 使用正则表达式匹配
        bool contains = Regex.IsMatch(value, pattern);
        return contains;
    }


    /// <summary>
    /// \uxxxx转中文,保留换行符号
    /// </summary>
    /// <param name="unicodeString"></param>
    /// <returns></returns>
    public static string Unescape(this string value)
    {
        if (value.IsNull())
        {
            return "";
        }

        try
        {
            Formatting formatting = Formatting.None;

            object jsonObj = JsonConvert.DeserializeObject(value);
            string unescapeValue = JsonConvert.SerializeObject(jsonObj, formatting);
            return unescapeValue;
        }
        catch (Exception ex)
        {

            return "";
        }
    }

    /// <summary>
    /// 将obj类型转换为string
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ConvertToString(this object s)
    {
        if (s == null)
        {
            return "";
        }
        else
        {
            return Convert.ToString(s);
        }
    }

       /// <summary>
    /// 计算余弦相似度
    /// </summary>
    public static double CalculateCosineSimilarity(IEnumerable<float> vecA, IEnumerable<float> vecB)
    {
        var dotProduct = vecA.Zip(vecB, (a, b) => a * b).Sum();
        var magnitudeA = Math.Sqrt(vecA.Sum(x => x * x));
        var magnitudeB = Math.Sqrt(vecB.Sum(x => x * x));
        return magnitudeA * magnitudeB == 0 ? 0 : dotProduct / (magnitudeA * magnitudeB);
    }


}
