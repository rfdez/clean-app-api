﻿
using CleanApp.Core.Enumerations;

namespace CleanApp.Core.Entities
{
    public partial class HomeTenant
    {
        public int HomeId { get; set; }
        public int TenantId { get; set; }
        public TenantRole TenantRole { get; set; }

        public virtual Home Home { get; set; }
        public virtual Tenant Tenant { get; set; }
    }
}
