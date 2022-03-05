using System.Collections.Generic;

namespace ClipboardDetector
{
    public class Item
    {
        public string name { get; set; }
        public string path { get; set; }
        public string format { get; set; }
        /// <summary>
        /// 如果匹配成功，是否替换剪切板里的内容
        /// </summary>
        public bool replace { get; set; }
        /// <summary>
        /// 是否是正则表达式匹配
        /// </summary>
        public bool regex { get; set; }
    }
    public class Config
    {
        public List<Item> items { get; set; }
    }
}
