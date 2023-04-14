using VaquinhaAnimal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VaquinhaAnimal.Data.Mappings
{
    public class RedeSocialMapping : IEntityTypeConfiguration<RedeSocial>
    {
        public void Configure(EntityTypeBuilder<RedeSocial> builder)
        {
            builder.Property(p => p.Tipo)
                .IsRequired();

            builder.Property(p => p.Url)
                .IsRequired()
                .HasMaxLength(200)
                .HasColumnType("varchar(200)");

            // RELATIONSHIP
            builder.HasOne(x => x.Campanha)
                .WithMany(x => x.RedesSociais)
                .HasForeignKey(x => x.Campanha_Id)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            builder.ToTable("RedesSociais");
        }
    }
}
