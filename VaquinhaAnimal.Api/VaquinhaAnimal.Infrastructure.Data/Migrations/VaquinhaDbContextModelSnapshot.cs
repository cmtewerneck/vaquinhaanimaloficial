﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VaquinhaAnimal.Data.Context;

namespace VaquinhaAnimal.Infrastructure.Data.Migrations
{
    [DbContext(typeof(VaquinhaDbContext))]
    partial class VaquinhaDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Adocao", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Abrigo_Nome")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<bool>("Adotado")
                        .HasColumnType("bit");

                    b.Property<bool>("Castrado")
                        .HasColumnType("bit");

                    b.Property<string>("Celular")
                        .HasMaxLength(20)
                        .HasColumnType("varchar(20)");

                    b.Property<string>("Descricao")
                        .HasMaxLength(1000)
                        .HasColumnType("varchar(1000)");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Empresa_Nome")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Facebook")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<int>("FaixaEtaria")
                        .HasColumnType("int");

                    b.Property<string>("Foto")
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Instagram")
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("LinkVideo")
                        .HasColumnType("varchar(100)");

                    b.Property<string>("NomePet")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Particular_Nome")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("TipoAnunciante")
                        .HasColumnType("int");

                    b.Property<int>("TipoPet")
                        .HasColumnType("int");

                    b.Property<string>("UrlAdocao")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("UsuarioId")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Adocoes");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Artigo", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("EscritoPor")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("FotoCapa")
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<string>("Html")
                        .IsRequired()
                        .HasMaxLength(10000)
                        .HasColumnType("varchar(10000)");

                    b.Property<string>("Resumo")
                        .IsRequired()
                        .HasMaxLength(1500)
                        .HasColumnType("varchar(1500)");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<string>("UrlArtigo")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("Id");

                    b.ToTable("Artigos");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Assinatura", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("CampanhaId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SubscriptionId")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("CampanhaId");

                    b.ToTable("Assinatura");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Beneficiario", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("Campanha_Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CodigoBanco")
                        .IsRequired()
                        .HasMaxLength(3)
                        .HasColumnType("varchar(3)");

                    b.Property<string>("DigitoAgencia")
                        .HasMaxLength(1)
                        .HasColumnType("varchar(1)");

                    b.Property<string>("DigitoConta")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("varchar(2)");

                    b.Property<string>("Documento")
                        .IsRequired()
                        .HasMaxLength(16)
                        .HasColumnType("varchar(16)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("NumeroAgencia")
                        .IsRequired()
                        .HasMaxLength(4)
                        .HasColumnType("varchar(4)");

                    b.Property<string>("NumeroConta")
                        .IsRequired()
                        .HasMaxLength(13)
                        .HasColumnType("varchar(13)");

                    b.Property<string>("RecebedorId")
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Tipo")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("TipoConta")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.HasIndex("Campanha_Id")
                        .IsUnique();

                    b.ToTable("Beneficiario");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Campanha", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("DataCriacao")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DataEncerramento")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("DataInicio")
                        .HasColumnType("datetime2");

                    b.Property<string>("DescricaoCurta")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<string>("DescricaoLonga")
                        .IsRequired()
                        .HasMaxLength(5000)
                        .HasColumnType("varchar(5000)");

                    b.Property<int?>("DuracaoDias")
                        .HasColumnType("int");

                    b.Property<bool>("Premium")
                        .HasColumnType("bit");

                    b.Property<int>("StatusCampanha")
                        .HasColumnType("int");

                    b.Property<int>("TagCampanha")
                        .HasColumnType("int");

                    b.Property<bool>("Termos")
                        .HasColumnType("bit");

                    b.Property<int>("TipoCampanha")
                        .HasColumnType("int");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<decimal>("TotalArrecadado")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("UrlCampanha")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.Property<Guid>("Usuario_Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("ValorDesejado")
                        .HasColumnType("decimal(18,2)");

                    b.Property<string>("VideoUrl")
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Campanhas");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Cartao", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Card_Id")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Customer_Id")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<int>("Exp_Month")
                        .HasColumnType("int");

                    b.Property<int>("Exp_Year")
                        .HasColumnType("int");

                    b.Property<string>("First_Six_Digits")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Last_Four_Digits")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Cartoes");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Doacao", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("Campanha_Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Charge_Id")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Customer_Id")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("Data")
                        .HasColumnType("datetime2");

                    b.Property<string>("FormaPagamento")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Transacao_Id")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Url_Download")
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Usuario_Id")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<decimal>("Valor")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ValorBeneficiario")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ValorDestinadoPlataforma")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ValorPlataforma")
                        .HasColumnType("decimal(18,2)");

                    b.Property<decimal>("ValorTaxa")
                        .HasColumnType("decimal(18,2)");

                    b.HasKey("Id");

                    b.HasIndex("Campanha_Id");

                    b.ToTable("Doacoes");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Imagem", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Arquivo")
                        .IsRequired()
                        .HasMaxLength(500)
                        .HasColumnType("varchar(500)");

                    b.Property<Guid>("Campanha_Id")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Tipo")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("Campanha_Id");

                    b.ToTable("Imagens");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Suporte", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Assunto")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<DateTime>("Data")
                        .HasColumnType("datetime2");

                    b.Property<string>("Mensagem")
                        .IsRequired()
                        .HasColumnType("varchar(100)");

                    b.Property<bool>("Respondido")
                        .HasColumnType("bit");

                    b.Property<string>("Resposta")
                        .HasColumnType("varchar(100)");

                    b.Property<Guid>("Usuario_Id")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.ToTable("Suportes");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Assinatura", b =>
                {
                    b.HasOne("VaquinhaAnimal.Domain.Entities.Campanha", "Campanha")
                        .WithMany("Assinaturas")
                        .HasForeignKey("CampanhaId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Campanha");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Beneficiario", b =>
                {
                    b.HasOne("VaquinhaAnimal.Domain.Entities.Campanha", "Campanha")
                        .WithOne("Beneficiario")
                        .HasForeignKey("VaquinhaAnimal.Domain.Entities.Beneficiario", "Campanha_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Campanha");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Doacao", b =>
                {
                    b.HasOne("VaquinhaAnimal.Domain.Entities.Campanha", "Campanha")
                        .WithMany("Doacoes")
                        .HasForeignKey("Campanha_Id")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Campanha");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Imagem", b =>
                {
                    b.HasOne("VaquinhaAnimal.Domain.Entities.Campanha", "Campanha")
                        .WithMany("Imagens")
                        .HasForeignKey("Campanha_Id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Campanha");
                });

            modelBuilder.Entity("VaquinhaAnimal.Domain.Entities.Campanha", b =>
                {
                    b.Navigation("Assinaturas");

                    b.Navigation("Beneficiario");

                    b.Navigation("Doacoes");

                    b.Navigation("Imagens");
                });
#pragma warning restore 612, 618
        }
    }
}
