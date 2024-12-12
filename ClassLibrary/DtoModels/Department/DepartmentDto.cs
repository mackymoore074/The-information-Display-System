using System;
using System.ComponentModel.DataAnnotations;

namespace ClassLibrary.DtoModels.Department
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DateCreated { get; set; }
        public int? AgencyId { get; set; }
        public string? AgencyName { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; }
    }

    public class CreateDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public int? AgencyId { get; set; }
        
        [Required]
        public int LocationId { get; set; }
    }

    public class UpdateDepartmentDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        
        [StringLength(500)]
        public string Description { get; set; }
        
        public int? AgencyId { get; set; }
        
        [Required]
        public int LocationId { get; set; }
    }
} 
