﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Azunt.TechnicianManagement
{
    public class TechniciansTableBuilder
    {
        private readonly string _masterConnectionString;
        private readonly ILogger<TechniciansTableBuilder> _logger;

        public TechniciansTableBuilder(string masterConnectionString, ILogger<TechniciansTableBuilder> logger)
        {
            _masterConnectionString = masterConnectionString;
            _logger = logger;
        }

        public void BuildTenantDatabases()
        {
            var tenantConnectionStrings = GetTenantConnectionStrings();

            foreach (var connStr in tenantConnectionStrings)
            {
                try
                {
                    EnsureTechniciansTable(connStr);
                    _logger.LogInformation($"Technicians table processed (tenant DB): {connStr}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[{connStr}] Error processing tenant DB");
                }
            }
        }

        public void BuildMasterDatabase()
        {
            try
            {
                EnsureTechniciansTable(_masterConnectionString);
                _logger.LogInformation("Technicians table processed (master DB)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing master DB");
            }
        }

        private List<string> GetTenantConnectionStrings()
        {
            var result = new List<string>();

            using (var connection = new SqlConnection(_masterConnectionString))
            {
                connection.Open();
                var cmd = new SqlCommand("SELECT ConnectionString FROM dbo.Tenants", connection);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var connectionString = reader["ConnectionString"]?.ToString();
                        if (!string.IsNullOrEmpty(connectionString))
                        {
                            result.Add(connectionString);
                        }
                    }
                }
            }

            return result;
        }

        private void EnsureTechniciansTable(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Check if 'Technicians' table exists
                var cmdCheck = new SqlCommand(@"
                    SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_NAME = 'Technicians'", connection);

                int tableCount = (int)cmdCheck.ExecuteScalar();

                if (tableCount == 0)
                {
                    // Create 'Technicians' table if it doesn't exist
                    var cmdCreate = new SqlCommand(@"
                        CREATE TABLE [dbo].[Technicians] (
                            [Id] BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                            [Active] BIT NOT NULL DEFAULT(1),
                            [IsDeleted] BIT NOT NULL DEFAULT(0),
                            [Created] DATETIMEOFFSET(7) NOT NULL,
                            [CreatedBy] NVARCHAR(255) NULL,
                            [Name] NVARCHAR(MAX) NULL,
                            [DisplayOrder] INT DEFAULT 0 NULL
                        )", connection);

                    cmdCreate.ExecuteNonQuery();
                    _logger.LogInformation("Technicians table created.");
                }

                // Check and add missing columns (if any)
                var expectedColumns = new Dictionary<string, string>
                {
                    // All columns from the Alls table
                    ["ParentId"] = "BIGINT NULL",
                    ["ParentKey"] = "NVARCHAR(255) NULL",
                    ["CreatedBy"] = "NVARCHAR(255) NULL",
                    ["Created"] = "DATETIMEOFFSET NULL DEFAULT SYSDATETIMEOFFSET()",
                    ["ModifiedBy"] = "NVARCHAR(255) NULL",
                    ["Modified"] = "DATETIMEOFFSET NULL",
                    ["Name"] = "NVARCHAR(255) NULL",
                    ["PostDate"] = "DATETIME NULL DEFAULT GETDATE()",
                    ["PostIp"] = "NVARCHAR(20) NULL",
                    ["Title"] = "NVARCHAR(512) NULL",
                    ["Content"] = "NTEXT NULL",
                    ["Category"] = "NVARCHAR(255) DEFAULT('Free') NULL",
                    ["Email"] = "NVARCHAR(255) NULL",
                    ["Password"] = "NVARCHAR(255) NULL",
                    ["ReadCount"] = "INT DEFAULT 0 NULL",
                    ["Encoding"] = "NVARCHAR(20) DEFAULT('HTML') NULL",
                    ["Homepage"] = "NVARCHAR(100) NULL",
                    ["ModifyDate"] = "DATETIME NULL",
                    ["ModifyIp"] = "NVARCHAR(15) NULL",
                    ["CommentCount"] = "INT DEFAULT 0 NULL",
                    ["IsPinned"] = "BIT DEFAULT 0 NULL",
                    ["FileName"] = "NVARCHAR(255) NULL",
                    ["FileSize"] = "INT DEFAULT 0 NULL",
                    ["DownCount"] = "INT DEFAULT 0 NULL",
                    ["Ref"] = "INT DEFAULT 0 NULL",
                    ["Step"] = "INT DEFAULT 0 NULL",
                    ["RefOrder"] = "INT DEFAULT 0 NULL",
                    ["AnswerNum"] = "INT DEFAULT 0 NULL",
                    ["ParentNum"] = "INT DEFAULT 0 NULL",
                    ["Status"] = "NVARCHAR(255) NULL",
                    ["TenantId"] = "BIGINT DEFAULT 0 NULL",
                    ["TenantName"] = "NVARCHAR(255) NULL",
                    ["AppId"] = "INT DEFAULT 0 NULL",
                    ["AppName"] = "NVARCHAR(255) NULL",
                    ["ModuleId"] = "INT DEFAULT 0 NULL",
                    ["ModuleName"] = "NVARCHAR(255) NULL",
                    ["IsLocked"] = "BIT DEFAULT 0 NULL",
                    ["Vote"] = "INT DEFAULT 0 NULL",
                    ["Weather"] = "TINYINT DEFAULT 0 NULL",
                    ["ReplyEmail"] = "BIT DEFAULT 0 NULL",
                    ["Published"] = "BIT DEFAULT 0 NULL",
                    ["BoardType"] = "NVARCHAR(100) NULL",
                    ["BoardName"] = "NVARCHAR(255) NULL",
                    ["NickName"] = "NVARCHAR(255) NULL",
                    ["IconName"] = "NVARCHAR(100) NULL",
                    ["Price"] = "DECIMAL(18,2) DEFAULT 0.00 NULL",
                    ["Community"] = "NVARCHAR(255) NULL",
                    ["StartDate"] = "DATETIMEOFFSET(7) NULL",
                    ["EndDate"] = "DATETIMEOFFSET(7) NULL",
                    ["Video"] = "NVARCHAR(1024) NULL",
                    ["SecurityLevel"] = "NVARCHAR(10) NULL",
                    ["AvailableCustomerLevel"] = "NVARCHAR(10) NULL",
                    ["Num"] = "INT DEFAULT 0 NULL",
                    ["UID"] = "INT DEFAULT 0 NULL",
                    ["UserId"] = "NVARCHAR(255) NULL",
                    ["UserName"] = "NVARCHAR(255) NULL",
                    ["DivisionId"] = "INT DEFAULT 0 NULL",
                    ["CategoryId"] = "INT DEFAULT 0 NULL",
                    ["BoardId"] = "INT DEFAULT 0 NULL",
                    ["ApplicationId"] = "INT DEFAULT 0 NULL",
                    ["IsDeleted"] = "BIT DEFAULT 0 NULL",
                    ["DeletedBy"] = "NVARCHAR(255) NULL",
                    ["Deleted"] = "DATETIMEOFFSET NULL",
                    ["ApprovalStatus"] = "NVARCHAR(50) NULL",
                    ["ApprovalBy"] = "NVARCHAR(255) NULL",
                    ["ApprovalDate"] = "DATETIMEOFFSET NULL",
                    ["UserAgent"] = "NVARCHAR(512) NULL",
                    ["Referer"] = "NVARCHAR(512) NULL",
                    ["SessionId"] = "NVARCHAR(255) NULL",
                    ["DisplayOrder"] = "INT DEFAULT 0 NULL",
                    ["ViewRoles"] = "NVARCHAR(255) NULL",
                    ["Tags"] = "NVARCHAR(255) NULL",
                    ["LikeCount"] = "INT DEFAULT 0 NULL",
                    ["DislikeCount"] = "INT DEFAULT 0 NULL",
                    ["Rating"] = "DECIMAL(3,2) DEFAULT 0.0 NULL",
                    ["Culture"] = "NVARCHAR(10) NULL",
                    ["IsSystem"] = "BIT DEFAULT 0 NULL",
                    ["SearchKeywords"] = "NVARCHAR(1024) NULL",
                    ["SortKey"] = "NVARCHAR(255) NULL",
                    ["Version"] = "INT DEFAULT 1 NULL",
                    ["HistoryGroupId"] = "UNIQUEIDENTIFIER NULL",
                    ["IsNotified"] = "BIT DEFAULT 0 NULL",
                    ["IsSubscribed"] = "BIT DEFAULT 0 NULL",
                    ["ExternalId"] = "NVARCHAR(255) NULL",
                    ["ExternalUrl"] = "NVARCHAR(1024) NULL",
                    ["SourceType"] = "NVARCHAR(50) NULL",
                    ["IsMobile"] = "BIT DEFAULT 0 NULL"
                };

                foreach (var (columnName, columnDefinition) in expectedColumns)
                {
                    var cmdColCheck = new SqlCommand(@"
                        SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS 
                        WHERE TABLE_NAME = 'Technicians' AND COLUMN_NAME = @ColumnName", connection);
                    cmdColCheck.Parameters.AddWithValue("@ColumnName", columnName);

                    int columnExists = (int)cmdColCheck.ExecuteScalar();

                    if (columnExists == 0)
                    {
                        var cmdAlter = new SqlCommand($@"
                            ALTER TABLE [dbo].[Technicians] 
                            ADD [{columnName}] {columnDefinition}", connection);
                        cmdAlter.ExecuteNonQuery();

                        _logger.LogInformation($"Column added to Technicians: {columnName} ({columnDefinition})");
                    }
                }

                // Insert default rows if the table is empty
                var cmdCountRows = new SqlCommand("SELECT COUNT(*) FROM [dbo].[Technicians]", connection);
                int rowCount = (int)cmdCountRows.ExecuteScalar();

                if (rowCount == 0)
                {
                    var cmdInsertDefaults = new SqlCommand(@"
                        INSERT INTO [dbo].[Technicians] (Active, IsDeleted, Created, CreatedBy, Name, DisplayOrder)
                        VALUES
                            (1, 0, SYSDATETIMEOFFSET(), 'System', 'Initial Technician 1', 1),
                            (1, 0, SYSDATETIMEOFFSET(), 'System', 'Initial Technician 2', 2)", connection);

                    int inserted = cmdInsertDefaults.ExecuteNonQuery();
                    _logger.LogInformation($"Technicians default data inserted: {inserted} rows.");
                }
            }
        }

        // Run method to call EnhanceMasterDatabase or EnhanceTenantDatabases
        public static void Run(IServiceProvider services, bool forMaster, string? optionalConnectionString = null)
        {
            try
            {
                var logger = services.GetRequiredService<ILogger<TechniciansTableBuilder>>();
                var config = services.GetRequiredService<IConfiguration>();

                string connectionString;

                if (!string.IsNullOrWhiteSpace(optionalConnectionString))
                {
                    connectionString = optionalConnectionString;
                }
                else
                {
                    var tempConnectionString = config.GetConnectionString("DefaultConnection");
                    if (string.IsNullOrEmpty(tempConnectionString))
                    {
                        throw new InvalidOperationException("DefaultConnection is not configured in appsettings.json.");
                    }

                    connectionString = tempConnectionString;
                }

                var builder = new TechniciansTableBuilder(connectionString, logger);

                if (forMaster)
                {
                    builder.BuildMasterDatabase();
                }
                else
                {
                    builder.BuildTenantDatabases();
                }
            }
            catch (Exception ex)
            {
                var fallbackLogger = services.GetService<ILogger<TechniciansTableBuilder>>();
                fallbackLogger?.LogError(ex, "Error while processing Technicians table.");
            }
        }
    }
}