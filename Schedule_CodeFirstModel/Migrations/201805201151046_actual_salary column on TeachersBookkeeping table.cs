namespace Schedule_CodeFirstModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class actual_salarycolumnonTeachersBookkeepingtable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TeachersBookkeepings", "ActualSalary", c => c.Double(nullable: false, defaultValue: 0));
        }
        
        public override void Down()
        {
            DropColumn("dbo.TeachersBookkeepings", "ActualSalary");
        }
    }
}
