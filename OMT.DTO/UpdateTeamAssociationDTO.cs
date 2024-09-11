﻿using System.ComponentModel.DataAnnotations;

namespace OMT.DTO
{
    public class UpdateTeamAssociationDTO
    {

        [Key]
        public int AssociationId { get; set; }
        public int? TeamId { get; set; }
        public int? ThresholdCount { get; set; }
        public string? Description { get; set; }

    }
}
