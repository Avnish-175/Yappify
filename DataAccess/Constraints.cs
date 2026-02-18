namespace ChatApp2.DataAccess
{
    /// <summary>
    /// Constraints
    /// </summary>
    public class Constraints
    {
        public const string DefaultMail = "tsuisl.conn_mngt@tatasteel.com";
    }

    /// <summary>
    /// Queries
    /// </summary>
    public class Queries
    {
        /// <summary>
        /// Common Queries
        /// </summary>
        public const string qryGetMasterDataByCode = @"USP_GetMasterDateByCode";
        public const string qryUsp_Update_RouteStageWiseUserMap = @"Usp_Update_RouteStageWiseUserMap";
        public const string qryUsp_Update_ApplicationCurrentStage = @"Usp_Update_ApplicationCurrentStage";
        public const string qryGetUserAreaByID = @"Usp_GetUserAreaByID";

        /// <summary>
        /// Report Queries
        /// </summary>
        public const string qryGetReportDataByType = @"Usp_getReportByType";
        public const string qryGetReportData = @"APIDataFetch";

    }

    /// <summary>
    /// Enums 
    /// </summary>
    enum ApplicationType
    {
        LT = 1,
        HT = 2,
        PC = 3,
        TC = 4
    }
    enum ApprovalStatus
    {
        Pending = 0,
        Approved = 1,
        Returned = 2,
        Rejected = 3
    }

    enum Role
    {
        Admin = 1,
        Super_Admin = 2,
        CRO = 3,
        Approver = 4
    }
    enum MasterCode
    {
        UserRole = 1,
        Attachment = 2,
        Stage = 3,
        Module = 4,
        Units = 5,
        Location = 6,
        RateCategoryJSR = 7,
        RateCategorySaraikela = 8,
        MCBDETAILS = 9,
        Report = 10
    }
}
