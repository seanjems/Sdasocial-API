using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace sdakccapi.Migrations
{
    public partial class AddedShoppingCartModels : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Sorting = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    CustomerID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CardNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressID = table.Column<long>(type: "bigint", nullable: false),
                    creditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    deleted = table.Column<bool>(type: "bit", nullable: false),
                    Company = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.CustomerID);
                });

            migrationBuilder.CreateTable(
                name: "orderDetails",
                columns: table => new
                {
                    detailID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    productID = table.Column<int>(type: "int", nullable: false),
                    qty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    costExc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    costInc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    priceInc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    priceExc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    taxID = table.Column<int>(type: "int", nullable: false),
                    taxPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    discountID = table.Column<int>(type: "int", nullable: false),
                    discountPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    totalPriceExc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    totalPriceInc = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    sortOrder = table.Column<int>(type: "int", nullable: false),
                    specialPricingUsed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orderDetails", x => x.detailID);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    orderCode = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    saleTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    change = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    shiftId = table.Column<int>(type: "int", nullable: false),
                    customerId = table.Column<long>(type: "bigint", nullable: false),
                    transactionStatus = table.Column<int>(type: "int", nullable: false),
                    saleAgentId = table.Column<int>(type: "int", nullable: false),
                    quotationID = table.Column<int>(type: "int", nullable: false),
                    orderStatus = table.Column<int>(type: "int", nullable: false),
                    transactionComment = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "paymentModes",
                columns: table => new
                {
                    PaymentModeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paymentModes", x => x.PaymentModeID);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    PaymentID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentModeID = table.Column<int>(type: "int", nullable: false),
                    SaleID = table.Column<long>(type: "bigint", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerID = table.Column<long>(type: "bigint", nullable: false),
                    CardRef = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameOnCheque = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BankID = table.Column<int>(type: "int", nullable: false),
                    BankingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CustomerDepositID = table.Column<int>(type: "int", nullable: false),
                    SupplierPaymentID = table.Column<int>(type: "int", nullable: false),
                    SupplierID = table.Column<int>(type: "int", nullable: false),
                    ExpenseID = table.Column<int>(type: "int", nullable: false),
                    PaymentEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PaymentDetailJson = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.PaymentID);
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    productCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    barCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    productName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    costExclusive = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    costInclusive = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    costIncStatus = table.Column<bool>(type: "bit", nullable: false),
                    inStock = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    priceExclusive = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    priceInclusive = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    categoryId = table.Column<int>(type: "int", nullable: false),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    segmentId = table.Column<int>(type: "int", nullable: false),
                    supplierId = table.Column<int>(type: "int", nullable: false),
                    productImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    createdDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    createdBy = table.Column<int>(type: "int", nullable: false),
                    deleted = table.Column<bool>(type: "bit", nullable: false),
                    trackInventory = table.Column<bool>(type: "bit", nullable: false),
                    reOrderLevel = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    reOrderQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    favourite = table.Column<bool>(type: "bit", nullable: false),
                    hasSubProduct = table.Column<bool>(type: "bit", nullable: false),
                    isAsubProduct = table.Column<bool>(type: "bit", nullable: false),
                    compoundCostPricing = table.Column<int>(type: "int", nullable: false),
                    tax = table.Column<int>(type: "int", nullable: false),
                    priceInclusive2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceExclusive2 = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_products_categories_categoryId",
                        column: x => x.categoryId,
                        principalTable: "categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_products_categoryId",
                table: "products",
                column: "categoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "orderDetails");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "paymentModes");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
