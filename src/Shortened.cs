using System;
using System.ComponentModel.DataAnnotations;

namespace Pier
{
    public class Shortened
    {
        [Key]
        public int Id { get; set; }
        public string ShortUrl { get; set; }
        public string LongUrl { get; set; }
        public DateTime Date { get; set; }
        public long Hits { get; set; }
    }
}