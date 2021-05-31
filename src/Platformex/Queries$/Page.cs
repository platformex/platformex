using System.Collections.Generic;

namespace Platformex
{
    public class Page<T>
    {
        public IEnumerable<T> Items { get; set; }
        public int TotalCount { get; set; }
    }
}