using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegService.Domain.Models
{
    public class ConfirmedEmail
    {
        public int Id { get; set; }              
        public string Email { get; set; } = string.Empty;  
        public DateTime ConfirmedAt { get; set; }
    }
}
