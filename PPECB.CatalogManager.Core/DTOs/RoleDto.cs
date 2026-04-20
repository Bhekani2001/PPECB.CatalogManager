using System;
using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class RoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsSystemRole { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
    }

    public class CreateRoleDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }

    public class UpdateRoleDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> Permissions { get; set; } = new List<string>();
    }
}