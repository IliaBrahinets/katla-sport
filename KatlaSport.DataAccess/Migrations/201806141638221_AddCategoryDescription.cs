namespace KatlaSport.DataAccess.Migrations
{
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Migration to create a description property of product category entity.
    /// </summary>
    public partial class AddCategoryDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.product_categories", "category_description", c => c.String(maxLength: 400));
        }

        public override void Down()
        {
            DropColumn("dbo.product_categories", "category_description");
        }
    }
}
