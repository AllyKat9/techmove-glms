using TechMoveGLMS_Web.Models;

namespace TechMoveGLMS.API.DTOs
{
    // DTO for creating a new client
    public class ClientCreateDto
    {
        public string Name { get; set; }
        public string ContactDetails { get; set; }
        public string Region { get; set; }
    }

    // DTO for updating an existing client
    public class ClientUpdateDto
    {
        public string Name { get; set; }
        public string ContactDetails { get; set; }
        public string Region { get; set; }
    }

    // DTO for returning client data in API responses
    public class ClientResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactDetails { get; set; }
        public string Region { get; set; }
        public int ContractCount { get; set; }
    }
}
