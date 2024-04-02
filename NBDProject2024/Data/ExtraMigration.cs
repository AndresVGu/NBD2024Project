using Microsoft.EntityFrameworkCore.Migrations;

namespace NBD2024.Data
{
    public static class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetProjectTimestampOnUpdate
                    AFTER UPDATE ON Projects
                    BEGIN
                        UPDATE Projects
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END

                ");
            migrationBuilder.Sql(@"
                CREATE TRIGGER SetProjectTimestampOnInsert
                    AFTER  INSERT ON Projects
                    BEGIN
                        UPDATE Projects
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
            
            ");

            migrationBuilder.Sql(
               @"
                    CREATE TRIGGER SetClientTimestampOnUpdate
                    AFTER UPDATE ON Clients
                    BEGIN
                        UPDATE Clients
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END

                ");
            migrationBuilder.Sql(@"
                CREATE TRIGGER SetClientTimestampOnInsert
                    AFTER  INSERT ON Clients
                    BEGIN
                        UPDATE Clients
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
            
            ");


            migrationBuilder.Sql(
               @"
                    CREATE TRIGGER SetBidTimestampOnUpdate
                    AFTER UPDATE ON Bids
                    BEGIN
                        UPDATE Bids
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END

                ");
            migrationBuilder.Sql(@"
                CREATE TRIGGER SetBidTimestampOnInsert
                    AFTER  INSERT ON Bids
                    BEGIN
                        UPDATE Bids
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
            
            ");
        }
    }
}
