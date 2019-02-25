using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Milka.Models
{
    public class PostModel
    {
       
        [Required]
        public string Author { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.MultilineText)]
        public string PostText { get; set; }
        
        public string Img1 { get; set; }
    }
}