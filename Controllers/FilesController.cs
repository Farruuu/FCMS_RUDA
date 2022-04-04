using BLL;
using DAL;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FCMS_RUDA.Controllers
{
    public class FilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;
        public FilesController(ILogger<FilesController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Documents> FilesList = new FilesDAL().GetAllDocuments();
            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View(FilesList);
        }

        public IActionResult DocumentTypes()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));
            return View(listtypes);
        }

        public IActionResult AddNewDocType()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View();
        }

        [HttpPost]
        public IActionResult AddNewDocType(DocumentType doctype)
        {
            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
                Int64 result = new FilesDAL().CreateNewDocumentType(doctype, UserID);

                if (result > 0)
                {
                    TempData["Success"] = "Document Type Added successfully!";
                    return RedirectToAction("DocumentTypes");
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
                _logger.LogError(ex.Message);
                return View();
            }
        }

        public IActionResult UpdateDocType(int DocTypeID)
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            DocumentType type = new FilesDAL().GetDocumentTypesByID(DocTypeID);
            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View(type);
        }

        [HttpPost]
        public IActionResult UpdateDocType(DocumentType doctype)
        {
            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
                int result = new FilesDAL().UpdateDocumentType(doctype, UserID);

                if (result == 0)
                {
                    TempData["Success"] = "Document Type Updated successfully!";
                    return RedirectToAction("DocumentTypes");
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
                _logger.LogError(ex.Message);
                return View();
            }
        }

        public IActionResult Departments()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View(listDept);
        }

        public IActionResult AddNewFile()
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDept = listDept;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatus = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatus;

            List<CommentCode> listCode = new FilesDAL().GetAllCommentCodes();
            ViewBag.ListCode = listCode;

            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View();
        }

        [HttpPost]
        public IActionResult AddNewFile(IFormFile Upload, DocumentViewModel viewModel, string Orgby, string Route, string Frwdto)
        {
            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDept = listDept;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatus = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatus;

            List<CommentCode> listCode = new FilesDAL().GetAllCommentCodes();
            ViewBag.ListCode = listCode;

            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                int maxnumber = new FilesDAL().GetMaxDocNumber(viewModel.Document.DocumentRoute != 3 ? viewModel.Document.OriginatedBy : viewModel.DocDetails.ForwardedTo, viewModel.Document.DocumentRoute);
                viewModel.Document.DocumentNumber = "RUDA-" + (viewModel.Document.DocumentRoute != 3 ? Orgby : Frwdto) + "-RR&DS-" + Route + "-" + maxnumber;

                if (viewModel != null && Upload.Length > 0)
                {
                    viewModel.DocDetails.AttachmentName = viewModel.Document.DocumentNumber + "_" + viewModel.DocDetails.RotationNo + Path.GetExtension(Upload.FileName);
                    viewModel.DocDetails.AttachmentPath = UploadFile(Upload, viewModel.Document.DocumentNumber, viewModel.DocDetails.AttachmentName);
                }

                if (viewModel.DocDetails.AttachmentPath == null)
                {
                    ViewBag.Message = "Error Uploading File to server. Please Try Again!";
                    return View();
                }

                int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
                Int64 result = new FilesDAL().CreateDocument(viewModel, UserID);

                if (result > 0)
                {
                    TempData["Success"] = "Document Added successfully!";
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
                _logger.LogError(ex.Message);
                return View();
            }
        }

        public IActionResult UpdateFile(int FileID)
        {
            if (HttpContext.Session.GetString("Name") == null)
            {
                TempData["Session"] = "Your Session has expired. Please Login again";
                return RedirectToAction("Logout", "Users");
            }

            DocumentViewModel ObjFile = new DocumentViewModel();

            ObjFile.Document = new FilesDAL().GetDocumentByID(FileID);

            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDept = listDept;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatus = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatus;

            List<CommentCode> listCode = new FilesDAL().GetAllCommentCodes();
            ViewBag.ListCode = listCode;

            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            return View(ObjFile);
        }

        [HttpPost]
        public IActionResult UpdateFile(IFormFile Upload, DocumentViewModel viewModel)
        {
            List<Departments> listDept = new UsersDAL().GetAllDepartments();
            ViewBag.ListDept = listDept;

            List<DocumentType> listtypes = new FilesDAL().GetAllDocumentTypes();
            ViewBag.ListTypes = listtypes;

            List<DocumentStatus> listStatus = new FilesDAL().GetAllDocumentStatus();
            ViewBag.ListStatus = listStatus;

            List<CommentCode> listCode = new FilesDAL().GetAllCommentCodes();
            ViewBag.ListCode = listCode;

            ViewBag.UserRole = Convert.ToInt32(HttpContext.Session.GetString("UserRole"));

            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }

                if (viewModel != null && Upload.Length > 0)
                {
                    viewModel.DocDetails.AttachmentName = viewModel.Document.DocumentNumber + "_" + viewModel.DocDetails.RotationNo + Path.GetExtension(Upload.FileName);
                    viewModel.DocDetails.AttachmentPath = UploadFile(Upload, viewModel.Document.DocumentNumber, viewModel.DocDetails.AttachmentName);
                }

                if (viewModel.DocDetails.AttachmentPath == null)
                {
                    ViewBag.Message = "Error Uploading File to server. Please Try Again!";
                    return View();
                }

                int UserID = Convert.ToInt32(HttpContext.Session.GetString("ID"));
                Int64 result = new FilesDAL().UpdateDocument(viewModel, UserID);

                if (result > 0)
                {
                    TempData["Success"] = "Document Updated successfully!";
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
                _logger.LogError(ex.Message);
                return View();
            }
        }


        #region SFTP Functions

        public string UploadFile(IFormFile Attachment, string DocNumber, string attachmentName)
        {
            var config = new SftpConfig
            {
                Host = "172.16.0.6",
                Port = 22,
                UserName = "Farrukh",
                Password = "Hello@321",
                BaseDirectory = "//RUDA-Downloads//DocumentControlSystem"
            }; // can be retrieved from appsettings.json

            using var client = new SftpClient(config.Host, config.Port, config.UserName, config.Password);

            try
            {
                client.Connect();
                client.ChangeDirectory(config.BaseDirectory);

                string FolderPath = config.BaseDirectory + "//" + DocNumber;
                string AttachmentPath = FolderPath + "//" + attachmentName;

                if (!client.Exists(FolderPath))
                    client.CreateDirectory(FolderPath);

                using (var filestream = Attachment.OpenReadStream())
                {
                    client.UploadFile(filestream, AttachmentPath);
                }

                return "//" + config.Host + AttachmentPath;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                client.Disconnect();
            }
        }

        public JsonResult DownloadFile(string DocumentNo, string remoteFilename)
        {
            var config = new SftpConfig
            {
                Host = "172.16.0.6",
                Port = 22,
                UserName = "Farrukh",
                Password = "Hello@321",
                BaseDirectory = "//RUDA-Downloads//DocumentControlSystem"
            }; // can be retrieved from appsettings.json

            using (SftpClient client = new SftpClient(config.Host, config.Port == 0 ? 22 : config.Port, config.UserName, config.Password))
            {
                string desktoppath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                try
                {
                    client.Connect();
                    string localFilePath = desktoppath + "\\" + remoteFilename;
                    string RemoteFilePath = config.BaseDirectory + "//" + DocumentNo + "//" + remoteFilename;

                    using (Stream stream = System.IO.File.Create(localFilePath))
                    {
                        client.DownloadFile(RemoteFilePath, stream);
                    }
                    client.Disconnect();

                    return Json("File Downloaded Successfully to Desktop");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed in downloading file");
                    return Json("Error Downloading file." + ex.Message + "\n" + desktoppath + " Please Try again!");
                }
                finally { client.Disconnect(); }
            }
        }

        #endregion
    }
}
