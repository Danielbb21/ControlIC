﻿// <auto-generated />
using System;
using ControlIC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Oracle.EntityFrameworkCore.Metadata;

namespace ControlIC.Migrations
{
    [DbContext(typeof(ControlICContext))]
    partial class ControlICContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn)
                .HasAnnotation("ProductVersion", "3.1.9")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("ControlIC.Models.Atividade", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Cor")
                        .HasColumnType("varchar(13)");

                    b.Property<DateTime>("DataPrevista")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<int>("ProjetoID")
                        .HasColumnType("NUMBER(10)");

                    b.Property<bool>("Restricao")
                        .HasColumnType("NUMBER(1)");

                    b.Property<string>("Texto")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Titulo")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("ID");

                    b.HasIndex("ProjetoID");

                    b.ToTable("Atividades");
                });

            modelBuilder.Entity("ControlIC.Models.AtividadeResponsavel", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("Arquivo")
                        .HasColumnType("BLOB");

                    b.Property<int>("AtividadeID")
                        .HasColumnType("NUMBER(10)");

                    b.Property<DateTime?>("DataEntrega")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<bool>("Entregue")
                        .HasColumnType("NUMBER(1)");

                    b.Property<string>("NomeArquivo")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("TipoArquivo")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int>("UsuarioID")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("ID");

                    b.HasIndex("AtividadeID");

                    b.HasIndex("UsuarioID");

                    b.ToTable("AtividadeResponsaveis");
                });

            modelBuilder.Entity("ControlIC.Models.CampoPesquisa", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Nome")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("ID");

                    b.ToTable("CampoPesquisas");
                });

            modelBuilder.Entity("ControlIC.Models.Curso", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Nome")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("ID");

                    b.ToTable("Cursos");
                });

            modelBuilder.Entity("ControlIC.Models.Postagem", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DataPostagem")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<int>("ProjetoID")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("Texto")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int>("UsuarioID")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("ID");

                    b.HasIndex("ProjetoID");

                    b.HasIndex("UsuarioID");

                    b.ToTable("Postagens");
                });

            modelBuilder.Entity("ControlIC.Models.Projeto", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Aprovado")
                        .HasColumnType("NUMBER(1)");

                    b.Property<int>("CampoPesquisaID")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("Descricao")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<byte[]>("ImgProjeto")
                        .HasColumnType("BLOB");

                    b.Property<string>("Nome")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("OutrasInformacoes")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<bool>("Status")
                        .HasColumnType("NUMBER(1)");

                    b.Property<int>("UsuarioID")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("ID");

                    b.HasIndex("CampoPesquisaID");

                    b.HasIndex("UsuarioID");

                    b.ToTable("Projetos");
                });

            modelBuilder.Entity("ControlIC.Models.ProjetoCoorientador", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Aprovado")
                        .HasColumnType("NUMBER(1)");

                    b.Property<int>("ProjetoID")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("Token")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int>("UsuarioID")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("ID");

                    b.HasIndex("ProjetoID");

                    b.HasIndex("UsuarioID");

                    b.ToTable("ProjetoCoorientadores");
                });

            modelBuilder.Entity("ControlIC.Models.ProjetoEstudante", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<bool>("Aprovado")
                        .HasColumnType("NUMBER(1)");

                    b.Property<int>("ProjetoID")
                        .HasColumnType("NUMBER(10)");

                    b.Property<string>("Token")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int>("UsuarioID")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("ID");

                    b.HasIndex("ProjetoID");

                    b.HasIndex("UsuarioID");

                    b.ToTable("ProjetoEstudantes");
                });

            modelBuilder.Entity("ControlIC.Models.Recrutamento", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<byte[]>("Arquivo")
                        .HasColumnType("BLOB");

                    b.Property<DateTime>("DataPostagem")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<string>("Descricao")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("LinkExterno")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<int>("ProjetoID")
                        .HasColumnType("NUMBER(10)");

                    b.Property<bool>("status")
                        .HasColumnType("NUMBER(1)");

                    b.HasKey("ID");

                    b.HasIndex("ProjetoID");

                    b.ToTable("Recrutamentos");
                });

            modelBuilder.Entity("ControlIC.Models.Titulacao", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DataTitulacao")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<string>("NomeInstituicao")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("NomeTitulacao")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.HasKey("ID");

                    b.ToTable("Titulacoes");
                });

            modelBuilder.Entity("ControlIC.Models.Usuario", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("NUMBER(10)")
                        .HasAnnotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn);

                    b.Property<int?>("AnoIngresso")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int?>("CursoID")
                        .HasColumnType("NUMBER(10)");

                    b.Property<DateTime>("DataNascimento")
                        .HasColumnType("TIMESTAMP(7)");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<byte[]>("ImgUsuario")
                        .HasColumnType("BLOB");

                    b.Property<string>("LinkedIn")
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Nome")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Senha")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(2000)");

                    b.Property<string>("Sexo")
                        .IsRequired()
                        .HasColumnType("NVARCHAR2(1)");

                    b.Property<int>("TipoUsuario")
                        .HasColumnType("NUMBER(10)");

                    b.Property<int?>("TitulacaoID")
                        .HasColumnType("NUMBER(10)");

                    b.HasKey("ID");

                    b.HasIndex("CursoID");

                    b.HasIndex("TitulacaoID");

                    b.ToTable("Usuarios");
                });

            modelBuilder.Entity("ControlIC.Models.Atividade", b =>
                {
                    b.HasOne("ControlIC.Models.Projeto", "Projeto")
                        .WithMany("Atividades")
                        .HasForeignKey("ProjetoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ControlIC.Models.AtividadeResponsavel", b =>
                {
                    b.HasOne("ControlIC.Models.Atividade", "Atividade")
                        .WithMany("Participantes")
                        .HasForeignKey("AtividadeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControlIC.Models.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ControlIC.Models.Postagem", b =>
                {
                    b.HasOne("ControlIC.Models.Projeto", "Projeto")
                        .WithMany("Postagens")
                        .HasForeignKey("ProjetoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControlIC.Models.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ControlIC.Models.Projeto", b =>
                {
                    b.HasOne("ControlIC.Models.CampoPesquisa", "CampoPesquisa")
                        .WithMany("Projetos")
                        .HasForeignKey("CampoPesquisaID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControlIC.Models.Usuario", "Usuario")
                        .WithMany()
                        .HasForeignKey("UsuarioID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ControlIC.Models.ProjetoCoorientador", b =>
                {
                    b.HasOne("ControlIC.Models.Projeto", "Projeto")
                        .WithMany("projetoCoorientadores")
                        .HasForeignKey("ProjetoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControlIC.Models.Usuario", "Usuario")
                        .WithMany("projetoCoorientadores")
                        .HasForeignKey("UsuarioID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ControlIC.Models.ProjetoEstudante", b =>
                {
                    b.HasOne("ControlIC.Models.Projeto", "Projeto")
                        .WithMany("ProjetoEstudantes")
                        .HasForeignKey("ProjetoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ControlIC.Models.Usuario", "Usuario")
                        .WithMany("ProjetoEstudantes")
                        .HasForeignKey("UsuarioID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ControlIC.Models.Recrutamento", b =>
                {
                    b.HasOne("ControlIC.Models.Projeto", "Projeto")
                        .WithMany("Recrutamentos")
                        .HasForeignKey("ProjetoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("ControlIC.Models.Usuario", b =>
                {
                    b.HasOne("ControlIC.Models.Curso", "Curso")
                        .WithMany("Usuarios")
                        .HasForeignKey("CursoID");

                    b.HasOne("ControlIC.Models.Titulacao", "Titulacao")
                        .WithMany()
                        .HasForeignKey("TitulacaoID");
                });
#pragma warning restore 612, 618
        }
    }
}
