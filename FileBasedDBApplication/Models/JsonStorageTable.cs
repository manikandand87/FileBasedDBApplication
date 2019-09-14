using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FileBasedDBApplication.Models
{
    public class JsonStorageTable
    {
        [Required]
        [MaxLength(length:32,ErrorMessage ="Key should not more be than 32 chars")]
        public string Key { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        [MaxLength(length: 2048, ErrorMessage = "Value should not more be than 2048 chars")]
        public string Value { get; set; }
        
        public int TimeToLive { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Active { get; set; }
    }
}