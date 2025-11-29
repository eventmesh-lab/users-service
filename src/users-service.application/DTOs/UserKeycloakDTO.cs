
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace users_service.application.DTOs
{/// DTO para recibir un usuario desde keycloak.
    public class UserKeycloakDTO
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }


    }
}
