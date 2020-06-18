using System;
using System.ComponentModel.DataAnnotations;

namespace RaspberryIRBlaster.Server.Models
{
    public class SendBatchItemRequestModel
    {
        public const string Type_SendMessage = "SendMessage";
        public const string Type_Sleep = "Sleep";

        [Required(AllowEmptyStrings = false)]
        public string Type { get; set; }

        public System.Text.Json.JsonElement Data { get; set; }
    }
}
