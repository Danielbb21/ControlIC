using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Oracle.EntityFrameworkCore.Metadata;

namespace ControlIC.Migrations
{
    public partial class initialize : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CampoPesquisas",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampoPesquisas", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Titulacoes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    NomeTitulacao = table.Column<string>(nullable: false),
                    NomeInstituicao = table.Column<string>(nullable: false),
                    DataTitulacao = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titulacoes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(nullable: false),
                    Senha = table.Column<string>(nullable: false),
                    Nome = table.Column<string>(nullable: false),
                    Sexo = table.Column<string>(nullable: false),
                    LinkedIn = table.Column<string>(nullable: true),
                    DataNascimento = table.Column<DateTime>(nullable: false),
                    ImgUsuario = table.Column<byte[]>(type: "BLOB", nullable: true),
                    TipoUsuario = table.Column<int>(nullable: false),
                    AnoIngresso = table.Column<int>(nullable: true),
                    CursoID = table.Column<int>(nullable: true),
                    TitulacaoID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Usuarios_Cursos_CursoID",
                        column: x => x.CursoID,
                        principalTable: "Cursos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuarios_Titulacoes_TitulacaoID",
                        column: x => x.TitulacaoID,
                        principalTable: "Titulacoes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projetos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(nullable: true),
                    Descricao = table.Column<string>(nullable: true),
                    OutrasInformacoes = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    Aprovado = table.Column<bool>(nullable: false),
                    ImgProjeto = table.Column<byte[]>(type: "BLOB", nullable: true),
                    CampoPesquisaID = table.Column<int>(nullable: false),
                    UsuarioID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projetos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Projetos_CampoPesquisas_CampoPesquisaID",
                        column: x => x.CampoPesquisaID,
                        principalTable: "CampoPesquisas",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Projetos_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Atividades",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Titulo = table.Column<string>(nullable: true),
                    Texto = table.Column<string>(nullable: true),
                    DataPrevista = table.Column<DateTime>(nullable: false),
                    Restricao = table.Column<bool>(nullable: false),
                    Cor = table.Column<string>(type: "varchar(13)", nullable: true),
                    ProjetoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Atividades", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Atividades_Projetos_ProjetoID",
                        column: x => x.ProjetoID,
                        principalTable: "Projetos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Postagens",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Texto = table.Column<string>(nullable: true),
                    DataPostagem = table.Column<DateTime>(nullable: false),
                    UsuarioID = table.Column<int>(nullable: false),
                    ProjetoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Postagens", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Postagens_Projetos_ProjetoID",
                        column: x => x.ProjetoID,
                        principalTable: "Projetos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Postagens_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjetoCoorientadores",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Aprovado = table.Column<bool>(nullable: false),
                    ProjetoID = table.Column<int>(nullable: false),
                    UsuarioID = table.Column<int>(nullable: false),
                    Token = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjetoCoorientadores", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProjetoCoorientadores_Projetos_ProjetoID",
                        column: x => x.ProjetoID,
                        principalTable: "Projetos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjetoCoorientadores_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjetoEstudantes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Aprovado = table.Column<bool>(nullable: false),
                    ProjetoID = table.Column<int>(nullable: false),
                    UsuarioID = table.Column<int>(nullable: false),
                    Token = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjetoEstudantes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProjetoEstudantes_Projetos_ProjetoID",
                        column: x => x.ProjetoID,
                        principalTable: "Projetos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjetoEstudantes_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recrutamentos",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(nullable: true),
                    LinkExterno = table.Column<string>(nullable: true),
                    Arquivo = table.Column<byte[]>(type: "BLOB", nullable: true),
                    DataPostagem = table.Column<DateTime>(nullable: false),
                    status = table.Column<bool>(nullable: false),
                    ProjetoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recrutamentos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Recrutamentos_Projetos_ProjetoID",
                        column: x => x.ProjetoID,
                        principalTable: "Projetos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AtividadeResponsaveis",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Oracle:ValueGenerationStrategy", OracleValueGenerationStrategy.IdentityColumn),
                    DataEntrega = table.Column<DateTime>(nullable: true),
                    Arquivo = table.Column<byte[]>(type: "BLOB", nullable: true),
                    Entregue = table.Column<bool>(nullable: false),
                    TipoArquivo = table.Column<string>(nullable: true),
                    NomeArquivo = table.Column<string>(nullable: true),
                    UsuarioID = table.Column<int>(nullable: false),
                    AtividadeID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AtividadeResponsaveis", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AtividadeResponsaveis_Atividades_AtividadeID",
                        column: x => x.AtividadeID,
                        principalTable: "Atividades",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AtividadeResponsaveis_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AtividadeResponsaveis_AtividadeID",
                table: "AtividadeResponsaveis",
                column: "AtividadeID");

            migrationBuilder.CreateIndex(
                name: "IX_AtividadeResponsaveis_UsuarioID",
                table: "AtividadeResponsaveis",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Atividades_ProjetoID",
                table: "Atividades",
                column: "ProjetoID");

            migrationBuilder.CreateIndex(
                name: "IX_Postagens_ProjetoID",
                table: "Postagens",
                column: "ProjetoID");

            migrationBuilder.CreateIndex(
                name: "IX_Postagens_UsuarioID",
                table: "Postagens",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjetoCoorientadores_ProjetoID",
                table: "ProjetoCoorientadores",
                column: "ProjetoID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjetoCoorientadores_UsuarioID",
                table: "ProjetoCoorientadores",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjetoEstudantes_ProjetoID",
                table: "ProjetoEstudantes",
                column: "ProjetoID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjetoEstudantes_UsuarioID",
                table: "ProjetoEstudantes",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_CampoPesquisaID",
                table: "Projetos",
                column: "CampoPesquisaID");

            migrationBuilder.CreateIndex(
                name: "IX_Projetos_UsuarioID",
                table: "Projetos",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Recrutamentos_ProjetoID",
                table: "Recrutamentos",
                column: "ProjetoID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_CursoID",
                table: "Usuarios",
                column: "CursoID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_TitulacaoID",
                table: "Usuarios",
                column: "TitulacaoID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AtividadeResponsaveis");

            migrationBuilder.DropTable(
                name: "Postagens");

            migrationBuilder.DropTable(
                name: "ProjetoCoorientadores");

            migrationBuilder.DropTable(
                name: "ProjetoEstudantes");

            migrationBuilder.DropTable(
                name: "Recrutamentos");

            migrationBuilder.DropTable(
                name: "Atividades");

            migrationBuilder.DropTable(
                name: "Projetos");

            migrationBuilder.DropTable(
                name: "CampoPesquisas");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropTable(
                name: "Titulacoes");
        }
    }
}
