using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SuppliesManagement.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DonViTinhs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonViTinhs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "KhoHangs",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenKho = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DiaChi = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhoHangs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "NhomHangs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhomHangs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonNhaps",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NhaCungCap = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayNhap = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Serial = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KhoHangID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonNhaps", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HoaDonNhaps_KhoHangs_KhoHangID",
                        column: x => x.KhoHangID,
                        principalTable: "KhoHangs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HoaDonXuats",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NguoiNhan = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NgayNhan = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LyDoXuat = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KhoHangID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDonXuats", x => x.ID);
                    table.ForeignKey(
                        name: "FK_HoaDonXuats_KhoHangs_KhoHangID",
                        column: x => x.KhoHangID,
                        principalTable: "KhoHangs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RoleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Accounts_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HangHoaHoaDons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenHangHoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonViTinhID = table.Column<int>(type: "int", nullable: false),
                    DonGiaTruocThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonGiaSauThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongGiaTruocThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VAT = table.Column<int>(type: "int", nullable: false),
                    TongGiaSauThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NhomHangID = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    KhoHangID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoaDonNhapID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HangHoaHoaDons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HangHoaHoaDons_DonViTinhs_DonViTinhID",
                        column: x => x.DonViTinhID,
                        principalTable: "DonViTinhs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HangHoaHoaDons_HoaDonNhaps_HoaDonNhapID",
                        column: x => x.HoaDonNhapID,
                        principalTable: "HoaDonNhaps",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_HangHoaHoaDons_KhoHangs_KhoHangID",
                        column: x => x.KhoHangID,
                        principalTable: "KhoHangs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HangHoaHoaDons_NhomHangs_NhomHangID",
                        column: x => x.NhomHangID,
                        principalTable: "NhomHangs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HangHoas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenHangHoa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonViTinhID = table.Column<int>(type: "int", nullable: false),
                    DonGiaTruocThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DonGiaSauThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TongGiaTruocThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    VAT = table.Column<int>(type: "int", nullable: false),
                    TongGiaSauThue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SoLuongDaXuat = table.Column<int>(type: "int", nullable: true),
                    SoLuongConLai = table.Column<int>(type: "int", nullable: true),
                    NhomHangID = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    KhoHangID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoaDonNhapID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HangHoas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HangHoas_DonViTinhs_DonViTinhID",
                        column: x => x.DonViTinhID,
                        principalTable: "DonViTinhs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HangHoas_HoaDonNhaps_HoaDonNhapID",
                        column: x => x.HoaDonNhapID,
                        principalTable: "HoaDonNhaps",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_HangHoas_KhoHangs_KhoHangID",
                        column: x => x.KhoHangID,
                        principalTable: "KhoHangs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HangHoas_NhomHangs_NhomHangID",
                        column: x => x.NhomHangID,
                        principalTable: "NhomHangs",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NhapKhos",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoaDonNhapID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HangHoaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HangHoaHoaDonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhapKhos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NhapKhos_HangHoaHoaDons_HangHoaHoaDonId",
                        column: x => x.HangHoaHoaDonId,
                        principalTable: "HangHoaHoaDons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NhapKhos_HoaDonNhaps_HoaDonNhapID",
                        column: x => x.HoaDonNhapID,
                        principalTable: "HoaDonNhaps",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "XuatKhos",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HoaDonXuatID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HangHoaHoaDonID = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_XuatKhos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_XuatKhos_HangHoaHoaDons_HangHoaHoaDonID",
                        column: x => x.HangHoaHoaDonID,
                        principalTable: "HangHoaHoaDons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_XuatKhos_HoaDonXuats_HoaDonXuatID",
                        column: x => x.HoaDonXuatID,
                        principalTable: "HoaDonXuats",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_RoleID",
                table: "Accounts",
                column: "RoleID");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoaHoaDons_DonViTinhID",
                table: "HangHoaHoaDons",
                column: "DonViTinhID");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoaHoaDons_HoaDonNhapID",
                table: "HangHoaHoaDons",
                column: "HoaDonNhapID");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoaHoaDons_KhoHangID",
                table: "HangHoaHoaDons",
                column: "KhoHangID");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoaHoaDons_NhomHangID",
                table: "HangHoaHoaDons",
                column: "NhomHangID");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoas_DonViTinhID",
                table: "HangHoas",
                column: "DonViTinhID");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoas_HoaDonNhapID",
                table: "HangHoas",
                column: "HoaDonNhapID");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoas_KhoHangID",
                table: "HangHoas",
                column: "KhoHangID");

            migrationBuilder.CreateIndex(
                name: "IX_HangHoas_NhomHangID",
                table: "HangHoas",
                column: "NhomHangID");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonNhaps_KhoHangID",
                table: "HoaDonNhaps",
                column: "KhoHangID");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDonXuats_KhoHangID",
                table: "HoaDonXuats",
                column: "KhoHangID");

            migrationBuilder.CreateIndex(
                name: "IX_NhapKhos_HangHoaHoaDonId",
                table: "NhapKhos",
                column: "HangHoaHoaDonId");

            migrationBuilder.CreateIndex(
                name: "IX_NhapKhos_HoaDonNhapID",
                table: "NhapKhos",
                column: "HoaDonNhapID");

            migrationBuilder.CreateIndex(
                name: "IX_XuatKhos_HangHoaHoaDonID",
                table: "XuatKhos",
                column: "HangHoaHoaDonID");

            migrationBuilder.CreateIndex(
                name: "IX_XuatKhos_HoaDonXuatID",
                table: "XuatKhos",
                column: "HoaDonXuatID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "HangHoas");

            migrationBuilder.DropTable(
                name: "NhapKhos");

            migrationBuilder.DropTable(
                name: "XuatKhos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "HangHoaHoaDons");

            migrationBuilder.DropTable(
                name: "HoaDonXuats");

            migrationBuilder.DropTable(
                name: "DonViTinhs");

            migrationBuilder.DropTable(
                name: "HoaDonNhaps");

            migrationBuilder.DropTable(
                name: "NhomHangs");

            migrationBuilder.DropTable(
                name: "KhoHangs");
        }
    }
}
