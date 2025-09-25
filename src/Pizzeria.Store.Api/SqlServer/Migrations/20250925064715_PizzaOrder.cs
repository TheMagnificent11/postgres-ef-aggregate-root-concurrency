using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pizzeria.Store.Api.SqlServer.Migrations;
    /// <inheritdoc />
    public partial class PizzaOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sto");

            migrationBuilder.CreateTable(
                name: "Orders",
                schema: "sto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    StartedDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PreparedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pizzas",
                schema: "sto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ModifiedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pizzas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderPizza",
                schema: "sto",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PizzaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderPizza", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderPizza_Orders_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "sto",
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderPizza_Pizzas_PizzaId",
                        column: x => x.PizzaId,
                        principalSchema: "sto",
                        principalTable: "Pizzas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderPizza_OrderId",
                schema: "sto",
                table: "OrderPizza",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPizza_PizzaId",
                schema: "sto",
                table: "OrderPizza",
                column: "PizzaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderPizza",
                schema: "sto");

            migrationBuilder.DropTable(
                name: "Orders",
                schema: "sto");

            migrationBuilder.DropTable(
                name: "Pizzas",
                schema: "sto");
        }
    }
