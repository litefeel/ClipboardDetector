using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClipboardDetector
{
    public class Item
    {
        public string name { get; set; }
        public string path { get; set; }
        public string format { get; set; }
    }
    public class Config
    {
        public List<Item> items { get; set; }
    }
}
