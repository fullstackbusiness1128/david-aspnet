using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FunTokenz.Migrations
{
    public partial class AddDbFilesTb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DBFiles",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Identifier = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    DateCreated = table.Column<long>(nullable: false),
                    DatetimeModified = table.Column<long>(nullable: false),
                    DatetimeCancelled = table.Column<long>(nullable: false),
                    RecordStatus = table.Column<bool>(nullable: false),
                    Furl = table.Column<string>(nullable: true),
                    Extension = table.Column<string>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    Size = table.Column<string>(nullable: true),
                    Type = table.Column<string>(nullable: true),
                    Category = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DBFiles", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DBFiles");
        }
    }
}
