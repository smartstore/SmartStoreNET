using System;
using System.Linq.Expressions;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.ModelConfiguration.Configuration;
using SmartStore.Core.Domain.Catalog;

namespace SmartStore.Data.Mapping.Catalog
{
    public partial class ProductVariantMap : EntityTypeConfiguration<ProductVariant>
    {
        public ProductVariantMap()
        {
            this.ToTable("ProductVariant");
            this.HasKey(pv => pv.Id);
            this.Property(pv => pv.Name).HasMaxLength(400);
            this.Property(pv => pv.Sku).HasMaxLength(400);
            this.Property(pv => pv.ManufacturerPartNumber).HasMaxLength(400);
            this.Property(pv => pv.Gtin).HasMaxLength(400);

            this.Property(pv => pv.AdditionalShippingCharge).HasPrecision(18, 4);
            this.Property(pv => pv.Price).HasPrecision(18, 4);
            this.Property(pv => pv.OldPrice).HasPrecision(18, 4);
            this.Property(pv => pv.ProductCost).HasPrecision(18, 4);
            this.Property(pv => pv.SpecialPrice).HasPrecision(18, 4);
            this.Property(pv => pv.MinimumCustomerEnteredPrice).HasPrecision(18, 4);
            this.Property(pv => pv.MaximumCustomerEnteredPrice).HasPrecision(18, 4);
            this.Property(pv => pv.Weight).HasPrecision(18, 4);
            this.Property(pv => pv.Length).HasPrecision(18, 4);
            this.Property(pv => pv.Width).HasPrecision(18, 4);
            this.Property(pv => pv.Height).HasPrecision(18, 4);
            
            //codehint: sm-add
            //this.Property(pv => pv.DeliveryTimeId);
            //this.HasOptional(pv => pv.DeliveryTime)
            //    .WithOptionalDependent();

            this.HasOptional(pv => pv.DeliveryTime)
                .WithMany()
                .HasForeignKey(pv => pv.DeliveryTimeId)
                .WillCascadeOnDelete(false);

            this.Property(pv => pv.RequiredProductVariantIds).HasMaxLength(1000);
            this.Property(pv => pv.AllowedQuantities).HasMaxLength(1000);

            this.Ignore(pv => pv.BackorderMode);
            this.Ignore(pv => pv.DownloadActivationType);
            this.Ignore(pv => pv.GiftCardType);
            this.Ignore(pv => pv.LowStockActivity);
            this.Ignore(pv => pv.ManageInventoryMethod);
            this.Ignore(pv => pv.RecurringCyclePeriod);


            
            this.HasRequired(pv => pv.Product)
                .WithMany(p => p.ProductVariants)
                .HasForeignKey(pv => pv.ProductId);
        }
    }

    // codehint: sm-add
    public partial class BasePriceQuotationMap : ComplexTypeConfiguration<BasePriceQuotation>
    {
        public BasePriceQuotationMap()
        {
            this.Property(x => x.Enabled).HasColumnName("BasePrice_Enabled");
            this.Property(x => x.MeasureUnit).HasColumnName("BasePrice_MeasureUnit");
            this.Property(x => x.Amount).HasColumnName("BasePrice_Amount");
            this.Property(x => x.BaseAmount).HasColumnName("BasePrice_BaseAmount");
        }
    }
}