using ChatApp2.Helper; // Assuming SqlHelper is located in ChatApp2.Helper
using ChatApp2.Models;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration; // Add this using directive
using System; // Add this using directive for Convert

namespace ChatApp2.DataAccess
{
    public class ManageData
    {
        private string UISL_DB = string.Empty;

        public ManageData()
        {
            // This way of getting configuration is acceptable if not using DI in ManageData's constructor directly.
            UISL_DB = GetConfiguration().GetSection("ConnectionStrings").GetSection("DefaultConnection").Value;
        }

        public IConfigurationRoot GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.IO.Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            return builder.Build();
        }

        #region Common Start
        /// <summary>
        /// Get Master Data by Code
        /// </summary>
        /// <param name="Code"></param>
        /// <param name="UserID"></param>
        /// <returns></returns>
        public DataTable GetMasterDataByCode(string Code, string UserID)
        {
            SqlHelper obj = new SqlHelper();
            var parameters = new[] {
                new SqlParameter("@Code", Code),
                new SqlParameter("@UserID", UserID)
            };
            // Assuming Queries.qryGetMasterDataByCode is a constant string for your stored procedure/query name
            // If it's a direct string, replace `Queries.qryGetMasterDataByCode` with the actual string like "USP_GetMasterDataByCode"
            return obj.Select(UISL_DB, "USP_GetMasterDataByCode", CommandType.StoredProcedure, parameters); // Adjust as per your actual query/SP
        }

        public DataTable GetGetUserAreaByUserID(string UserID)
        {
            SqlHelper obj = new SqlHelper();
            var parameters = new[] {
                new SqlParameter("@UserID", UserID)
            };
            // Assuming Queries.qryGetGetUserAreaByUserID is a constant string for your stored procedure/query name
            // If it's a direct string, replace `Queries.qryGetGetUserAreaByUserID` with the actual string like "USP_GetUserAreaByUserID"
            return obj.Select(UISL_DB, "USP_GetUserAreaByUserID", CommandType.StoredProcedure, parameters); // Adjust as per your actual query/SP
        }
        // Add other common methods if they exist in your original file
        #endregion

        // Existing SignUp method
        public bool SignUp(Login ObjLogin)
        {
            SqlHelper obj = new SqlHelper();
            var param = new[] {
                            new SqlParameter("@UserName", ObjLogin.Name),
                            new SqlParameter("@UserEmail", ObjLogin.EmailID),
                            new SqlParameter("@UserPhoneNo", ObjLogin.PhoneNo),
                            // IMPORTANT: You should hash the password BEFORE passing it here for security
                            new SqlParameter("@UserPassword", ObjLogin.Password),
                            new SqlParameter("@CreatedBy", "0")
                            // You might want to add PublicUsername here with NULL or a default value initially
                            // new SqlParameter("@PublicUsername", DBNull.Value)
                        };
            bool Result = Convert.ToInt32(obj.Execute(UISL_DB, "USP_User_SignUp", CommandType.StoredProcedure, param)) > 0;
            return Result;
        }

        // Updated Login method to retrieve PublicUsername
        public Login Login(string email, string password)
        {
            Login user = null;
            SqlHelper helper = new SqlHelper();

            var parameters = new[]
            {
                new SqlParameter("@UserEmail", email),
                new SqlParameter("@UserPassword", password) // Ensure this is hashed if storing hashed passwords in DB
            };

            DataTable dt = helper.Select(UISL_DB, "USP_User_Login", CommandType.StoredProcedure, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                user = new Login
                {
                    ID = Convert.ToInt32(row["ID"]),
                    Name = row["UserName"].ToString(),
                    EmailID = row["UserEmail"].ToString(),
                    PhoneNo = row["UserPhoneNo"].ToString(),
                    PublicUsername = row["PublicUsername"] != DBNull.Value ? row["PublicUsername"].ToString() : null
                };
            }
            return user;
        }

        // Existing SaveMessage method
        public bool SaveMessage(string message, int createdBy, int receiverId, string type, string fileUrl, int updatedBy)
        {
            using (var conn = new SqlConnection(UISL_DB))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
            INSERT INTO T_Chat_trans (Message, MessageType, CreatedBy, ReceiverId, CreatedOn, IsActive,UpdatedBy)
            VALUES (@msg, @type, @sender, @receiver, GETDATE(), 1,@UpdatedBy)", conn);

                cmd.Parameters.AddWithValue("@msg", message);
                cmd.Parameters.AddWithValue("@type", type);
                cmd.Parameters.AddWithValue("@sender", createdBy);
                cmd.Parameters.AddWithValue("@receiver", receiverId);
                cmd.Parameters.AddWithValue("@UpdatedBy", updatedBy);
                return cmd.ExecuteNonQuery() > 0;
            }
        }


        // Method to get user details (including PublicUsername) by ID
        public Login GetUserById(int userId)
        {
            Login user = null;
            SqlHelper helper = new SqlHelper();
            var parameters = new[]
            {
                new SqlParameter("@UserID", userId) // Assuming your stored procedure takes @UserID
            };
            DataTable dt = helper.Select(UISL_DB, "USP_User_GetByID", CommandType.StoredProcedure, parameters);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                user = new Login
                {
                    ID = Convert.ToInt32(row["ID"]),
                    Name = row["UserName"].ToString(),
                    EmailID = row["UserEmail"].ToString(),
                    PhoneNo = row["UserPhoneNo"].ToString(),
                    PublicUsername = row["PublicUsername"] != DBNull.Value ? row["PublicUsername"].ToString() : null
                };
            }
            return user;
        }

        // ✅ NEW METHOD: GetReport for ReportApiController
        public DataTable GetReport(PayLoad ObjPayLoad)
        {
            SqlHelper helper = new SqlHelper();
            var parameters = new[]
            {
                new SqlParameter("@StartDate", ObjPayLoad.StartDate),
                new SqlParameter("@EndDate", ObjPayLoad.EndDate),
                new SqlParameter("@Key", ObjPayLoad.Key)
                // Add other parameters if your USP_GetReport stored procedure requires them
            };
            // IMPORTANT: Replace "USP_GetReport" with the actual name of your stored procedure
            // that fetches the report data based on the provided payload.
            return helper.Select(UISL_DB, "USP_GetReport", CommandType.StoredProcedure, parameters);
        }

        // ✅ NEW METHOD: InsertUpdateAttachmentData for CommonController
        public bool InsertUpdateAttachmentData(AttachmentMap ObjAttachmentMap)
        {
            SqlHelper helper = new SqlHelper();
            var parameters = new[]
            {
                // Assuming your stored procedure (e.g., USP_InsertUpdateAttachment) takes these parameters
                new SqlParameter("@ID", ObjAttachmentMap.ID), // If ID is 0, it might be an insert, otherwise update
                new SqlParameter("@UserID", ObjAttachmentMap.UserID),
                new SqlParameter("@MasterID", ObjAttachmentMap.MasterID),
                new SqlParameter("@Type", ObjAttachmentMap.Type),
                new SqlParameter("@EntityTypeID", ObjAttachmentMap.EntityTypeID),
                new SqlParameter("@SectionID", ObjAttachmentMap.SectionID),
                new SqlParameter("@Attachment", ObjAttachmentMap.Attachment), // Stored filename
                new SqlParameter("@AttachmentName", ObjAttachmentMap.AttachmentName), // Original filename
                new SqlParameter("@IsActive", ObjAttachmentMap.IsActive)
            };
            // IMPORTANT: Replace "USP_InsertUpdateAttachment" with the actual name of your stored procedure
            // that handles the insertion or update of attachment data.
            int result = helper.Execute(UISL_DB, "USP_InsertUpdateAttachment", CommandType.StoredProcedure, parameters);
            return result > 0; // Return true if at least one row was affected
        }
    }
}