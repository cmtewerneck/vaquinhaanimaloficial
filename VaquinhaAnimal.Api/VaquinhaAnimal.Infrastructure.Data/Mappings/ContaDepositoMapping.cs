using VaquinhaAnimal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace VaquinhaAnimal.Data.Mappings
{
    public class ContaDepositoMapping : IEntityTypeConfiguration<ContaDeposito>
    {
        public void Configure(EntityTypeBuilder<ContaDeposito> builder)
        {
            builder.Property(p => p.Banco)
                .IsRequired();

            builder.Property(p => p.Agencia)
                .IsRequired()
                .HasMaxLength(10)
                .HasColumnType("varchar(10)");

            builder.Property(p => p.AgenciaDigito)
                .HasMaxLength(1)
                .HasColumnType("varchar(1)");

            builder.Property(p => p.Conta)
                .IsRequired()
                .HasMaxLength(13)
                .HasColumnType("varchar(13)");

            builder.Property(p => p.ContaDigito)
                .IsRequired()
                .HasMaxLength(2)
                .HasColumnType("varchar(2)");

            builder.Property(p => p.Documento)
                .IsRequired()
                .HasMaxLength(14)
                .HasColumnType("varchar(14)");

            builder.ToTable("ContasDeposito");
        }
    }
}
