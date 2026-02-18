using ChatApp2.DataAccess;
using ChatApp2.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace ChatApp.Controllers.Api
{
    [Route("api/[controller]/[action]")]
    public class ReportApiController : ControllerBase
    {
        //[HttpPost]
        //[ActionName("getReportDataByType")]
        //public IActionResult GetReportDataByType([FromBody] Report ObjReport)
        //{
        //    ManageData ObjManageData = new ManageData();
        //    ResponseModel model = new ResponseModel();
        //    if(!string.IsNullOrEmpty(HttpContext.Session.GetString("UserID")))
        //    ObjReport.UserID = HttpContext.Session.GetString("UserID");
        //    DataTable dtReqList = ObjManageData.GetReportDataByType(ObjReport);
        //    List<Dictionary<string, object>> _ReqList = new List<Dictionary<string, object>>();
        //    foreach (DataRow row in dtReqList.Rows)
        //    {
        //        var childRow = new Dictionary<string, object>();
        //        foreach (DataColumn col in dtReqList.Columns)
        //        {
        //            childRow.Add(col.ColumnName, row[col]);
        //        }
        //        _ReqList.Add(childRow);
        //    }

        //    model.AdditionalInfor1 = dtReqList.Columns.Count.ToString();
        //    model.AdditionalInfor2 = string.Join(",", dtReqList.Columns.Cast<DataColumn>().Select(col => col.ColumnName));
        //    model.Table = _ReqList;
        //    model.Status = true;
        //    model.Message = "OK";
        //    return Ok(model);
        //}

        [HttpPost]
        [ActionName("getDataCMGMSTOGIS")]
        public IActionResult getReport([FromBody] PayLoad ObjPayLoad)
        {
            if (ObjPayLoad != null)
            {
                ManageData ObjManageData = new ManageData();
                var KeyData = ObjManageData.GetMasterDataByCode("14", ""); // for key
                if (KeyData.Rows.Count > 0)
                {
                    for (int i = 0; i < KeyData.Rows.Count; i++)
                    {
                        if (ObjPayLoad.Key == KeyData.Rows[i]["value"].ToString())
                        {
                            DataTable dt = ObjManageData.GetReport(ObjPayLoad);
                            if (dt.Rows.Count > 0)
                            {
                                return Ok(JsonConvert.SerializeObject(dt));
                            }
                            else
                            {
                                return BadRequest(new
                                {
                                    Message = "Check your date range!",
                                    Errors = "Error"
                                });
                            }
                        }
                    }
                    return BadRequest(new
                    {
                        Message = "Invalid key!",
                        Errors = "Error"
                    });
                }
                return BadRequest(new
                {
                    Message = "Key not found!",
                    Errors = "Error"
                });
            }
            else
            {
                return BadRequest(new
                {
                    Message = "Payload is missing!",
                    Errors = "Error"
                });
            }
        }
    }
}
