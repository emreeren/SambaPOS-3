using System.Data.Entity;
using FluentMigrator;

namespace Samba.Persistance.DBMigration
{
    [Migration(13)]
    public class Migration_013 : Migration
    {
        public override void Up()
        {
            Create.Column("ToggleCalculation").OnTable("CalculationTypes").AsBoolean().WithDefaultValue(false);

            //-- Entity Type Assignments Reference Fix

            Execute.Sql("Delete from EntityTypeAssignments where TicketType_Id is null");
            Create.Column("TicketTypeId").OnTable("EntityTypeAssignments").AsInt32().WithDefaultValue(0);
            Execute.Sql("Update EntityTypeAssignments set TicketTypeId = TicketType_Id");

            Delete.ForeignKey("FK_dbo.EntityTypeAssignments_dbo.TicketTypes_TicketType_Id").OnTable("EntityTypeAssignments");
            Delete.Index("IX_TicketType_Id").OnTable("EntityTypeAssignments");

            Delete.Column("TicketType_Id").FromTable("EntityTypeAssignments");

            Create.ForeignKey("FK_dbo.EntityTypeAssignments_dbo.TicketTypes_TicketTypeId")
                .FromTable("EntityTypeAssignments").ForeignColumn("TicketTypeId")
                .ToTable("TicketTypes").PrimaryColumn("Id");

            Create.Index("IX_TicketTypeId").OnTable("EntityTypeAssignments").OnColumn("TicketTypeId").Ascending();

            //-- Menu Assignment Table

            Create.Table("MenuAssignments")
                  .WithColumn("Id").AsInt32().Identity().PrimaryKey()
                  .WithColumn("TicketTypeId").AsInt32().WithDefaultValue(0)
                  .WithColumn("TerminalName").AsString(128).Nullable()
                  .WithColumn("TerminalId").AsInt32().WithDefaultValue(0)
                  .WithColumn("MenuId").AsInt32().WithDefaultValue(0)
                  .WithColumn("SortOrder").AsInt32().WithDefaultValue(0);

            Create.ForeignKey("FK_dbo.MenuAssignments_dbo.TicketTypes_TicketTypeId")
                .FromTable("MenuAssignments").ForeignColumn("TicketTypeId")
                .ToTable("TicketTypes").PrimaryColumn("Id");

            Create.Index("IX_TicketTypeId").OnTable("MenuAssignments").OnColumn("TicketTypeId").Ascending();
        }

        public override void Down()
        {

        }
    }
}