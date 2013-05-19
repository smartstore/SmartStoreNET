using System;
using System.Collections.Generic;
using System.Linq;

using System.Data.Entity.ModelConfiguration;
using SmartStore.Core.Domain.Themes;

namespace SmartStore.Data.Mapping.Themes
{
    
    public class ThemeVariableMap : EntityTypeConfiguration<ThemeVariable>
    {
        public ThemeVariableMap()
        { 
            this.ToTable("ThemeVariable");
            this.HasKey(t => t.Id);
            this.Property(t => t.Theme).HasMaxLength(400);
            this.Property(t => t.Name).HasMaxLength(400);
            this.Property(t => t.Value).HasMaxLength(2000).IsOptional();
        }
    }

}
