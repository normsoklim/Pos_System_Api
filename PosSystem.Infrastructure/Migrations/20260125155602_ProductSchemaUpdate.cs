using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
         {
             // First, drop the foreign key constraint
             migrationBuilder.Sql("ALTER TABLE [Products] DROP CONSTRAINT [FK_Products_Categories_CategoryId1]");
 
             // Drop the index
             migrationBuilder.Sql("DROP INDEX [IX_Products_CategoryId1] ON [Products]");
 
             // Create a temporary table to store the existing data
             migrationBuilder.Sql(@"
                 SELECT Id, ProductName, Price, QtyInStock, Description, CategoryId, CreatedAt
                 INTO [Products_Temp]
                 FROM [Products]
             ");
 
             // Drop the old Products table
             migrationBuilder.Sql("DROP TABLE [Products]");
 
             // Create the new Products table with correct schema
             migrationBuilder.Sql(@"
                 CREATE TABLE [Products] (
                     [Id] uniqueidentifier NOT NULL,
                     [ProductName] nvarchar(255) NOT NULL,
                     [Barcode] nvarchar(100) NOT NULL DEFAULT '',
                     [Price] decimal(18,2) NOT NULL,
                     [QtyInStock] int NOT NULL,
                     [Description] nvarchar(1000) NOT NULL,
                     [ImageUrl] nvarchar(500) NULL,
                     [CategoryId] uniqueidentifier NOT NULL,
                     [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
                     CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
                     CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
                 )
             ");
 
             // Create the index
             migrationBuilder.Sql("CREATE INDEX [IX_Products_CategoryId] ON [Products] ([CategoryId])");
 
             // Restore data from the temporary table to the new table
             // Since we're changing from int Id to Guid Id and int CategoryId to Guid CategoryId,
             // we need to handle the conversion appropriately
             // For existing data, we'll generate new Guids for the Id and use the first CategoryId for all records
             migrationBuilder.Sql(@"
                 INSERT INTO [Products] ([Id], [ProductName], [Barcode], [Price], [QtyInStock], [Description], [ImageUrl], [CategoryId], [CreatedAt])
                 SELECT NEWID(), [ProductName], '', [Price], [QtyInStock], [Description], NULL,
                        (SELECT TOP 1 [Id] FROM [Categories]), [CreatedAt]
                 FROM [Products_Temp]
             ");
 
             // Drop the temporary table
             migrationBuilder.Sql("DROP TABLE [Products_Temp]");
         }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
         {
             // Store data from the new schema in a temporary table
             migrationBuilder.Sql(@"
                 SELECT Id, ProductName, Price, QtyInStock, Description, CategoryId, CreatedAt
                 INTO [Products_Restore]
                 FROM [Products]
             ");
 
             // Drop the new Products table
             migrationBuilder.Sql("DROP TABLE [Products]");
 
             // Recreate the old Products table structure
             migrationBuilder.Sql(@"
                 CREATE TABLE [Products] (
                     [Id] int IDENTITY(1,1) NOT NULL,
                     [ProductName] nvarchar(max) NOT NULL,
                     [Price] decimal(18,2) NOT NULL,
                     [QtyInStock] int NOT NULL,
                     [Description] nvarchar(max) NOT NULL,
                     [CategoryId1] uniqueidentifier NOT NULL,
                     [CategoryId] int NOT NULL,
                     [CreatedAt] datetime2 NOT NULL,
                     CONSTRAINT [PK_Products] PRIMARY KEY ([Id]),
                     CONSTRAINT [FK_Products_Categories_CategoryId1] FOREIGN KEY ([CategoryId1]) REFERENCES [Categories] ([Id]) ON DELETE CASCADE
                 )
             ");
 
             // Create the old index
             migrationBuilder.Sql("CREATE INDEX [IX_Products_CategoryId1] ON [Products] ([CategoryId1])");
 
             // Drop the temporary restore table
             migrationBuilder.Sql("DROP TABLE [Products_Restore]");
         }
    }
}
