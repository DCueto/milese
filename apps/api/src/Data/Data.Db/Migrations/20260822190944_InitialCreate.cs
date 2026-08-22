using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Milese.Data.Db.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "curriculum");

            migrationBuilder.CreateTable(
                name: "tracks",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tracks", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    track_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.id);
                    table.ForeignKey(
                        name: "fk_subjects_tracks_track_id",
                        column: x => x.track_id,
                        principalSchema: "curriculum",
                        principalTable: "tracks",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "concepts",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    subject_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_concepts", x => x.id);
                    table.ForeignKey(
                        name: "fk_concepts_subjects_subject_id",
                        column: x => x.subject_id,
                        principalSchema: "curriculum",
                        principalTable: "subjects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                schema: "curriculum",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    concept_id = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    estimated_minutes = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons", x => x.id);
                    table.ForeignKey(
                        name: "fk_lessons_concepts_concept_id",
                        column: x => x.concept_id,
                        principalSchema: "curriculum",
                        principalTable: "concepts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_concepts_subject_id",
                schema: "curriculum",
                table: "concepts",
                column: "subject_id");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_concept_id",
                schema: "curriculum",
                table: "lessons",
                column: "concept_id");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_track_id",
                schema: "curriculum",
                table: "subjects",
                column: "track_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "lessons",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "concepts",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "subjects",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "tracks",
                schema: "curriculum");
        }
    }
}
