using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.Core.Entities
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public virtual Role Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public virtual Permission Permission { get; set; } = null!;

        public bool IsGranted { get; set; } = true;
    }
}
