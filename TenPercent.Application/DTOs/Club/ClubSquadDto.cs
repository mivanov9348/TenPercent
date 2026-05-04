namespace TenPercent.Api.DTOs
{
    using System.Collections.Generic;    
    public class ClubSquadDto
    {
        public List<ClubPlayerDto> Goalkeepers { get; set; } = new();
        public List<ClubPlayerDto> Defenders { get; set; } = new();
        public List<ClubPlayerDto> Midfielders { get; set; } = new();
        public List<ClubPlayerDto> Strikers { get; set; } = new();
    }
}