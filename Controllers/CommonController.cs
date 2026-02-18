using ChatApp2.DataAccess;
using ChatApp2.Models;
using Microsoft.AspNetCore.Mvc;
using System.Transactions;

namespace ChatApp2.Controllers
{
    public class CommonController : Controller
    {
        private readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _HostEnvironment;

        public CommonController(Microsoft.AspNetCore.Hosting.IHostingEnvironment HostEnvironment)
        {
            _HostEnvironment = HostEnvironment;
        }

        [HttpPost]
        public IActionResult UploadFiles(AttachmentMap ObjAttachmentMap, List<IFormFile> files)
        {
            var Result = string.Empty;
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                if (files == null || files.Count == 0)
                {
                    return Json(Result);
                }
                var uploadPath = Path.Combine(_HostEnvironment.WebRootPath, "Uploads");

                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var DocFiles = new List<AttachmentMap>();
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {

                        string NewFileName = Guid.NewGuid().ToString() + "-" + DateTime.Now.ToString("ddMMyyyyhhmmss") + Path.GetExtension(file.FileName);
                        var filePath = Path.Combine(uploadPath, NewFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            file.CopyToAsync(fileStream);
                        }
                        ObjAttachmentMap.AttachmentName = file.FileName;
                        ObjAttachmentMap.Attachment = NewFileName;
                        DocFiles.Add(ObjAttachmentMap);
                        ManageData ObjManageData = new ManageData();
                        if (ObjManageData.InsertUpdateAttachmentData(ObjAttachmentMap))
                            Result = "Success";
                        else
                            Result = "";
                    }
                }
                if (Result == "Success")
                    scope.Complete();
            }
            return Json(Result);
        }
    }
}
