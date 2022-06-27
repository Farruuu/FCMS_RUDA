using BLL;
using DAL;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FCMS_RUDA.Controllers
{
    public class EFileController : Controller
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            DataTable dt = new EFileDAL().GetMyFiles(Convert.ToInt32(HttpContext.Session.GetString("ID")));

            return View(dt);
        }

        public IActionResult FileRequests()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            DataTable dt = new EFileDAL().GetPendingFileRequests(Convert.ToInt32(HttpContext.Session.GetString("ID")));
            //ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View(dt);
        }

        public IActionResult AddNewFile()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Users> listUsers = new UsersDAL().GetUsersForFileMark(Convert.ToInt32(HttpContext.Session.GetString("Department")), Convert.ToInt32(HttpContext.Session.GetString("ID")));
            ViewBag.ListUsers = listUsers;

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            return View();
        }

        [HttpPost]
        public IActionResult AddNewFile(EFileViewModel viewModel)
        {
            List<Users> listUsers = new UsersDAL().GetUsersForFileMark(Convert.ToInt32(HttpContext.Session.GetString("Department")), Convert.ToInt32(HttpContext.Session.GetString("ID")));
            ViewBag.ListUsers = listUsers;

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                string deptname = HttpContext.Session.GetString("DepartmentName").ToString();
                int index1 = HttpContext.Session.GetString("DepartmentName").ToString().IndexOf("-") + 1;

                string DeptInitials = deptname[index1..];

                int maxnumber = new FilesDAL().GetMaxDocNumber(Convert.ToInt32(HttpContext.Session.GetString("Department")), 0);
                viewModel.File.FileNumber = "RUDA/" + DeptInitials + "/" + DateTime.Now.Year + "/" + maxnumber;

                if (viewModel != null && viewModel.FileDetails.FileAttachment != null && viewModel.FileDetails.FileAttachment.Length > 0)
                {
                    viewModel.FileDetails.AttachmentName = viewModel.File.FileNumber + "_" + viewModel.FileDetails.AttachmentName + Path.GetExtension(viewModel.FileDetails.FileAttachment.FileName);
                    viewModel.FileDetails.AttachmentPath = UploadFile(viewModel.FileDetails.FileAttachment, viewModel.File.FileNumber, viewModel.FileDetails.AttachmentName);
                }
                else
                {
                    viewModel.FileDetails.AttachmentName = string.Empty;
                    viewModel.FileDetails.AttachmentPath = string.Empty;
                }

                viewModel.File.FileStatus = 2;
                viewModel.File.OriginatedBy = UserID;
                viewModel.FileDetails.MarkedDate = DateTime.Now.Date;
                viewModel.FileDetails.Comments = string.Empty;

                Int64 result = new EFileDAL().CreateFile(viewModel);

                if (result > 0)
                {
                    TempData["Success"] = "File Created successfully!";
                    return RedirectToAction("Index");
                }
                else
                {
                    ViewBag.Message = "Error Occured while saving Record. Please Try Again!";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Error Occured while saving Record. Please Try Again!";
                return View();
            }
        }

        public IActionResult ViewFile(int FileID)
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            EFile file = new EFileDAL().GetFilebyID(FileID);

            List<DocumentPriority> listPriority = new FilesDAL().GetAllDocumentPriorities();
            ViewBag.ListPriority = listPriority;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            return View(file);
        }

        #region SFTP Functions

        public string UploadFile(IFormFile Attachment, string DocNumber, string attachmentName)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();

            SftpConfig ftp = new SftpConfig
            {
                Host = config["SftpSetting:Host"],
                Port = Convert.ToInt32(config["SftpSetting:Port"]),
                UserName = config["SftpSetting:UserName"],
                Password = config["SftpSetting:Password"],
                BaseDirectory = config["SftpSetting:BaseDirectory"],
            };

            using var client = new SftpClient(ftp.Host, ftp.Port, ftp.UserName, ftp.Password);

            try
            {
                client.Connect();
                client.ChangeDirectory(ftp.BaseDirectory);

                string FolderPath = ftp.BaseDirectory + "//" + DocNumber;
                string AttachmentPath = FolderPath + "//" + attachmentName;

                if (!client.Exists(FolderPath))
                    client.CreateDirectory(FolderPath);

                using (var filestream = Attachment.OpenReadStream())
                {
                    client.UploadFile(filestream, AttachmentPath);
                }

                return "//" + ftp.Host + AttachmentPath;
            }
            catch (Exception) { return null; }
            finally { client.Disconnect(); }
        }

        public IActionResult DownloadFile(string remoteFilename)
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.Development.json").Build();

            SftpConfig ftp = new SftpConfig
            {
                Host = config["SftpSetting:Host"],
                Port = Convert.ToInt32(config["SftpSetting:Port"]),
                UserName = config["SftpSetting:UserName"],
                Password = config["SftpSetting:Password"],
                BaseDirectory = config["SftpSetting:BaseDirectory"],
            };

            using (SftpClient client = new SftpClient(ftp.Host, ftp.Port == 0 ? 22 : ftp.Port, ftp.UserName, ftp.Password))
            {
                string desktoppath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                try
                {
                    client.Connect();

                    string RemoteFilePath = ftp.BaseDirectory + "//" + remoteFilename;
                    byte[] fileBytes = client.ReadAllBytes(RemoteFilePath);
                    return File(fileBytes, "application/force-download", remoteFilename);
                }
                catch (Exception ex) { return Json("Error Downloading file." + ex.Message + "\n" + desktoppath + " Please Try again!"); }
                finally { client.Disconnect(); }
            }
        }

        #endregion
    }
}
