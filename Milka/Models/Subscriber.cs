using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.IdentityModel.Tokens;

namespace Milka.Models
{
    [Table("Subscriber")]
    public class Subscriber
    {
        [Required]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Required]
        [StringLength(255)]
        public string Email { get; set; }
        
        [Required]
        public bool RulesAcceptance { get; set; }
        
        [Required] 
        public DateTime Created { get; set; }
    }

    
}