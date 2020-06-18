using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RaspberryIRBlaster.Server.Models
{
    public class SendBatchRequestModel
    {
        [Required]
        public ICollection<SendBatchItemRequestModel> Items { get; set; }
    }
}
