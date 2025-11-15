
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Aplication.DTOs
{
    public class UserKeycloakDTO
    {
        [JsonPropertyName("id")]
        public required string Id { get; set; }

        [JsonPropertyName("username")]
        public required string Username { get; set; }


    }
}
