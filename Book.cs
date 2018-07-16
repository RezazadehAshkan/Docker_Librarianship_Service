using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ContainerProd.Models
{
    public class Book
    {
        [Key]
        public int id { get; set; }
        public int number { get; set; }
        public string title { get; set; }
        public string author { get; set; }
        public int status { get; set; }

    }

}
