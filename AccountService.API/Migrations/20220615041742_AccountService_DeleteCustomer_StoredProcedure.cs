using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

#nullable disable

namespace AccountService.API.Migrations
{
    public partial class AccountService_DeleteCustomer_StoredProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string deleteCustomerStoredProcedure = @"
            CREATE OR REPLACE PROCEDURE delete_customer (customer_email VARCHAR, customer_id INT)
            LANGUAGE plpgsql
            AS $$
            BEGIN 

            -- Delete AspNetUsers Data
                DELETE FROM ""AspNetUsers"" WHERE ""Email"" = customer_email;
            
            -- Delete the non-essential customer data
                UPDATE ""Customers"" SET 
                    ""SanitationMethodCode"" = NULL,
                    ""PoolTypeCode"" = NULL,
                    ""PoolSizeCode"" = NULL, 
                    ""PoolMaterialCode"" = NULL, 
                    ""HotTubTypeCode"" = NULL,
                    ""LastCompletedOnboardingStep"" = NULL, 
                    ""ReceivePromotionalContent"" = NULL,
                    ""ProfilePhotoBlobId"" = NULL,
                    ""FirstFreeCallAvailed"" = NULL,
                    ""PasswordResetDate"" = NULL,
                    ""PoolSeasonCode"" = NULL,
                    ""IsDeleted"" = true
                WHERE ""Id"" = customer_id;

            -- Delete Customer Relationships Data
                DELETE FROM ""CustomerFavouriteDealerMappings"" WHERE ""CustomerId"" = customer_id;
                DELETE FROM ""CustomerSuggestedDealers"" WHERE ""CustomerId"" = customer_id;
                DELETE FROM ""CustomerDeliveryInstructions"" WHERE ""CustomerId"" = customer_id;
            
            -- Delete Audits Data
                DELETE FROM ""Audit_CustomerFavouriteDealerMappings"" WHERE ""CustomerId"" = customer_id;
                DELETE FROM ""Audit_CustomerSuggestedDealers"" WHERE ""CustomerId"" = customer_id;
                DELETE FROM ""Audit_CustomerDeliveryInstructions"" WHERE ""CustomerId"" = customer_id;
              
            END
            $$;";
            
            migrationBuilder.Operations.Add(new SqlOperation
            {
                Sql = deleteCustomerStoredProcedure,
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Operations.Add(new SqlOperation
            {
                Sql = "DROP PROCEDURE delete_customer (customer_email VARCHAR);",
            });
        }
    }
}
