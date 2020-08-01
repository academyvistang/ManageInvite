using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IencircleAdmin.Models;

using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text;
using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Buffers.Text;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using CosmosDBService;
using CosmosDBEntities.Models;
using System.Globalization;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ZXing;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.AspNetCore.Identity;
using IdentitySample.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Authorization;

namespace IencircleAdmin.Controllers
{
    [Authorize()]
    public class HomeController : Controller
    {
        
        private UserManager<ApplicationUser> _userManager;
       // private IDbProvider _dbProvider = null;
        protected ICompositeViewEngine viewEngine;
        protected IDbProvider _cosmosDbProvider;
        public HomeController(ICompositeViewEngine viewEngine, UserManager<ApplicationUser> userManager, IMemoryCache cache, IDbProvider dbProvider)
        {
            try
            {
                this.viewEngine = viewEngine;
                _userManager = userManager;
                _cosmosDbProvider = dbProvider;
                _cosmosDbProvider.SetCache(cache);
            }
            catch(Exception ex)
            {
                string pp = ex.Message;
            }
        }

       



        protected string RenderViewAsString(object model, string viewName = null)
        {
            viewName = viewName ?? ControllerContext.ActionDescriptor.ActionName;
            ViewData.Model = model;

            using (StringWriter sw = new StringWriter())
            {
                IView view = viewEngine.FindView(ControllerContext, viewName, true).View;
                ViewContext viewContext = new ViewContext(ControllerContext, view, ViewData, TempData, sw, new HtmlHelperOptions());

                view.RenderAsync(viewContext).Wait();

                return sw.GetStringBuilder().ToString();
            }
        }


        //private string CreateVisitorPass(BusinessCard businessCards, Location place)
        //{

        //    IFormFile file = Request.Form.Files[0];

        //    byte[] filearray = null;

        //    //if (Request.Files.Count > 0)
        //    //{
        //    //    var binaryFileData = new byte[Request.Files[0].ContentLength];
        //    //    Request.Files[0].InputStream.Read(binaryFileData, 0, Request.Files[0].ContentLength);
        //    //    filearray = binaryFileData;
        //    //}

        //    BusinessCardGen g = new BusinessCardGen();

        //    var fileName = businessCards.Phone + ".png";

        //    var path = Path.Combine(Server.MapPath("~/BusinessCards/" + businessCards.PlaceId.ToString()), fileName);

        //    var fullname = businessCards.Fullname;
        //    var jobTitle = businessCards.JobTitle;
        //    var industry = businessCards.Industry;
        //    var address = businessCards.AddressLine1;
        //    var telephone = businessCards.Phone;
        //    var email = businessCards.Email;
        //    var companyname = businessCards.Company;

        //    var imagePath = g.ProcessRequest(path, fullname, jobTitle, address, industry, telephone, email, companyname, filearray);

        //    return imagePath;


        //}

        [HttpGet]
        public IActionResult PolicyStatement()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TermsOfService()
        {
            return View();
        }



        [HttpGet]
        [Route("Import")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Import(string id, string eventName)
        {

            Business newEvent = null;

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(eventName))
            {
              
                newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);
            }
             
            if(newEvent != null)
            {
                //if (newEvent.BusinessDate.HasValue && newEvent.BusinessDate.Value < DateTime.Now)
                //{
                //    return RedirectToAction("IndexEvent");
                //}

                var model = new EventModel { AppBusinessName = eventName, Id = id };

                return View(model);
            }
            else
            {
                return RedirectToAction("IndexEvent");
            }
        }

        [HttpGet]
        public IActionResult Delete(string id)
        {
            //_dbService.DeleteGuest(id.ToString());
            return RedirectToAction("Guests");
        }

      

  
        [HttpGet]
        [Route("Search")]
        [Route("{id}/{quickSearch})")]
        public async Task<IActionResult> Search(string id, string quickSearch)
        {
            var gLst = new List<Guest>();

            var newEvent = new Business();

            try
            {


                newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);

                if (null != newEvent)
                {
                    gLst = newEvent.Guests.ToList();

                    if (!string.IsNullOrEmpty(quickSearch))
                    {
                        //gLst = gLst.Where(x => x.Fullname.ToUpper().Contains(quickSearch.ToUpper()) || x.Telephone == quickSearch).ToList();
                        gLst = gLst.Where(x => x.Fullname.ToUpper().Contains(quickSearch.ToUpper())).OrderBy(x => x.Fullname).ToList();
                    }
                }
            }
            catch(Exception)
            {
                //IFormFile file = Request.Form.Files[0];
                byte[] file = System.IO.File.ReadAllBytes(@"C:\MasterList.xlsx");

                var memoryStream = new MemoryStream(file);
                //

                using (var stream = memoryStream)
                {
                    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false))

                    {
                        //create the object for workbook part  
                        WorkbookPart workbookPart = doc.WorkbookPart;
                        Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();
                        //StringBuilder excelResult = new StringBuilder();

                        int counter = 0;

                        //using for each loop to get the sheet from the sheetcollection  
                        foreach (Sheet thesheet in thesheetcollection)
                        {

                            //excelResult.AppendLine("Excel Sheet Name : " + thesheet.Name);
                            //excelResult.AppendLine("----------------------------------------------- ");
                            //statement to get the worksheet object by using the sheet id  
                            Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                            SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                            List<Guest> guestList = new List<Guest>();

                            foreach (Row thecurrentrow in thesheetdata)
                            {
                                counter++;

                                if (counter == 1) continue;

                                var cellCounter = 0;

                                var gu = Guid.NewGuid().ToString();

                                var g = new Guest { Id = gu, Status = "OTHERS", Telephone = gu, IsActive = true, BusinessId = "", AppBusinessName = "" };

                                foreach (Cell thecurrentcell in thecurrentrow)
                                {
                                    //statement to take the integer value  
                                    string currentcellvalue = string.Empty;

                                    if (thecurrentcell.DataType != null)
                                    {
                                        if (thecurrentcell.DataType == CellValues.SharedString)
                                        {
                                            int idNew;

                                            if (Int32.TryParse(thecurrentcell.InnerText, out idNew))
                                            {
                                                SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(idNew);

                                                if (item.Text != null)
                                                {
                                                    //code to take the string
                                                    var result = item.Text.Text;

                                                    if (cellCounter == 0)
                                                    {
                                                        g.Fullname = result;
                                                    }
                                                    else if (cellCounter == 1)
                                                    {
                                                        g.CompanyName = result;
                                                    }
                                                    else if (cellCounter == 2)
                                                    {
                                                        g.Email = result;
                                                    }
                                                    else if (cellCounter == 4)
                                                    {
                                                        if (string.IsNullOrEmpty(result))
                                                        {
                                                            g.Status = "OTHERS";
                                                        }
                                                        else
                                                        {
                                                            g.Status = result;
                                                        }

                                                    }
                                                    else if (cellCounter == 5)
                                                    {
                                                        g.Telephone = result;
                                                    }
                                                }
                                                else if (item.InnerText != null)
                                                {
                                                    currentcellvalue = item.InnerText;
                                                }
                                                else if (item.InnerXml != null)
                                                {
                                                    currentcellvalue = item.InnerXml;
                                                }
                                            }
                                        }
                                    }
                               
                                    cellCounter++;
                                }

                                if (!string.IsNullOrEmpty(g.Fullname))
                                {
                                    guestList.Add(g);
                                }


                            }

                            gLst = guestList;

                            if (!string.IsNullOrEmpty(quickSearch))
                            {
                                //gLst = gLst.Where(x => x.Fullname.ToUpper().Contains(quickSearch.ToUpper()) || x.Telephone == quickSearch).ToList();
                                gLst = gLst.Where(x => x.Fullname.ToUpper().Contains(quickSearch.ToUpper())).OrderBy(x => x.Fullname).ToList();
                            }

                        }
                    }
                }

            }

            return View(new EventModel { AppBusinessName = newEvent.AppBusinessName, Id = newEvent.Id, Guests = gLst });

        }

        
        [HttpGet]
        public async Task<IActionResult> Download(string id, string telephone)
        {
             var events = await _cosmosDbProvider.GetFastBusinesssAllEventsCached(true);

            var newEvent = events.FirstOrDefault(x => x.Id == id);

            var items = newEvent.Guests.ToList();

            var item = items.FirstOrDefault(x => x.Telephone == telephone);

            item.QRCode = GenerateQR(item,newEvent.Id, 300);


            var imageBytes = Convert.FromBase64String(item.QRCode.Replace("data:image/png;base64,", ""));
            var image = new FileContentResult(imageBytes, "image/jpg");
            return File(imageBytes, "image/png", "invite.png");

           // return image;

        }

        [HttpGet]
        [Route("EditGuest")]
        [Route("{id}/{eventName}/{guestId}")]
        public async Task<IActionResult> EditGuest(string id, string eventName, string guestId)
        {

            var newEvent = await _cosmosDbProvider.GetFastBusinesss(id);

            Guest guest = null;

            if (newEvent.Guests == null)
            {
                guest = new Guest { Id = Guid.NewGuid().ToString(), IsActive = true, BusinessId = id, AppBusinessName = eventName, Processed = false };
                return View(new EventModel { AppBusinessName = eventName, Id = id, Guest = guest });
            }

            var items = newEvent.Guests;

            guest = items.FirstOrDefault(x => x.Id == guestId);

            if (guest == null)
            {
                guest = new Guest { Id = Guid.NewGuid().ToString(), IsActive = true, BusinessId = id, AppBusinessName = eventName };
            }

            return View(new EventModel { AppBusinessName = eventName, Id = id, Guest = guest });
        }

        
        [HttpGet]
        [Route("SendInviteSingle")]
        //[Route("{id}/{eventName}/{email}")]
        [Route("{id}/{eventName}/{guestid}")]
        public async Task<IActionResult> SendInviteSingle(string id, string eventName, string guestid)
        {

            var newEvent = await _cosmosDbProvider.GetFastBusinesss(id,true);

            var items = newEvent.Guests;

            var allOthergGuests = items.Where(x => x.Id != guestid).ToList();

            var thisCrew = items.Where(x => x.Id == guestid).ToList();

            thisCrew.ForEach(x => x.InviteSent = true);


            foreach (var item in thisCrew)
            {
                item.QRCode = GenerateQR(item, newEvent.Id);

                var existingItem = item;

                if (!string.IsNullOrEmpty(item.Email))
                {
                   await SendEmail(newEvent, item, item.Fullname, item.Email, item.QRCode, item.Telephone, newEvent.BusinessLogoPath, item.NoOfInvites, item.InvitePath, item.CompanyName, item.Id);
                }
            }

            allOthergGuests.AddRange(thisCrew);

            newEvent.Guests = allOthergGuests.ToArray();

            await _cosmosDbProvider.ReplaceNewBusinessItem(eventName, id, newEvent);

            return RedirectToAction("Guests", new { id = newEvent.Id, eventName = newEvent.AppBusinessName });

        }

        
        [HttpGet]
        [Route("ProcessInvite")]
        [Route("{id}/{eventName}/{guestid}")]
        public async Task<IActionResult> ProcessInvite(string id, string eventName, string guestid)
        {
            ////var events = await _cosmosDbProvider.GetItems(id,eventName);

            //var newEvent = events.FirstOrDefault(x => x.Id == id && x.AppBusinessName == eventName);

            //var items = newEvent.Guests;

            //var thisGuest = items.FirstOrDefault(x => x.Id == guestid);
            //await _cosmosDbProvider.UpdateGuestDetails(eventName, id, guestid);

            return RedirectToAction("Guests", new { id = id, eventName = eventName });

            //return View(new EventModel { AppBusinessName = eventName, Id = id, Guests = allGuestsList });
        }

        [HttpGet]
        [Route("ResendInvite")]
        [Route("{id}/{eventName}/{guestid}")]
        public async Task<IActionResult> ResendInvite(string id, string eventName, string guestid)
        {
             var events = await _cosmosDbProvider.GetFastBusinesssAllEventsCached(true);

            var newEvent = events.FirstOrDefault(x => x.Id == id && x.AppBusinessName == eventName);

            var items = newEvent.Guests;

            var allOthergGuests = items.Where(x => x.Id != guestid).ToList();
            var thisCrew = items.Where(x => x.Id == guestid).ToList();

            thisCrew.ForEach(x => x.InviteSent = true);


            foreach (var item in thisCrew)
            {
                item.QRCode = GenerateQR(item, newEvent.Id);

                var existingItem = item;

                if (!string.IsNullOrEmpty(item.Email))
                {
                    //SendEmail(newEvent.Id,item.Fullname, item.Email, item.QRCode, item.Telephone);
                   await SendEmail(newEvent, item, item.Fullname, item.Email, item.QRCode, item.Telephone, newEvent.BusinessLogoPath, item.NoOfInvites, item.InvitePath, item.CompanyName, item.Id);

                }
            }

            allOthergGuests.AddRange(thisCrew);

            newEvent.Guests = allOthergGuests.ToArray();

            await _cosmosDbProvider.ReplaceNewBusinessItem(eventName, id, newEvent);

            return RedirectToAction("Guests", new { id = newEvent.Id, eventName = newEvent.AppBusinessName });

            //return View(new EventModel { AppBusinessName = eventName, Id = id, Guests = allGuestsList });
        }

        [HttpGet]
        [Route("SendInvites")]
        [Route("{id}/{eventName}/{confirmed})")]
        public async Task<IActionResult> SendInvites(string id, string eventName, bool? confirmed)
        {

            if(confirmed.HasValue && confirmed.Value)
            {
                var events = await _cosmosDbProvider.GetFastBusinesssAllEventsCached(true);

                var newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);

                var dc = newEvent.DiaryQRCode;
                var lc = newEvent.LocationQRCode;

                var items = newEvent.Guests;

                var updatedGuests = new List<Guest>();

                foreach (var item in items)
                {
                    var existingItem = item;

                    if (!string.IsNullOrEmpty(item.Email) && !item.InviteSent && item.IsActive)
                    {
                        item.QRCode = GenerateQR(item, newEvent.Id);

                        await SendEmail(newEvent, item, item.Fullname, item.Email, item.QRCode, item.Telephone, newEvent.BusinessLogoPath, item.NoOfInvites, item.InvitePath, item.CompanyName, item.Id);

                        if (null != existingItem)
                        {
                            existingItem.InviteSent = true;
                            updatedGuests.Add(existingItem);
                        }

                    }
                    else
                    {
                        if (null != existingItem)
                        {
                            existingItem.InviteSent = false;
                            updatedGuests.Add(existingItem);
                        }
                    }
                }

                newEvent.Guests = updatedGuests.ToArray();

                await _cosmosDbProvider.ReplaceNewBusinessItem(eventName, id, newEvent);

                return RedirectToAction("Guests", new { id = newEvent.Id, eventName = newEvent.AppBusinessName });
            }
            else
            {
                return View("Confirmation", new EventViewModel { Id = id, EventName = eventName, Confirmed = true });
            }
            
        }




        private async Task<int> SendEmail(Business newEvent, Guest guest, string fullname, string email, string qrCode, string telephone, string eventlogo, int noOfInvites, string invitePath, string companyName, string guestId)
        {
            if(!string.IsNullOrEmpty(telephone))
            {
                if(!telephone.StartsWith("0"))
                {
                    telephone = companyName;
                }
            }

            var fullUrl = $"https://businesscardgeneratorlive.azurewebsites.net/api/Values?guestId={guestId}&telephone={telephone}&fullname={fullname}&email={email}&id={newEvent.Id}&companyName={companyName}&eventName={newEvent.AppBusinessName}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(fullUrl))
                    {
                        using (HttpContent content = res.Content)
                        {
                            try
                            {
                                var jsoninvitePath = await content.ReadAsStringAsync();
                                invitePath = JsonConvert.DeserializeObject<string>(jsoninvitePath);
                            }
                            catch (Exception)
                            {
                                
                            }
                        }
                    }
                }

                var emailTemplate = string.Empty;

                //guestid
                var inviteAcceptPath = "https://iencircle.azurewebsites.net/guest/?id=" + newEvent.Id + "&guestId=" + guestId + "&inviteAcceped=" + bool.TrueString;
                var inviteDownloadPath = invitePath.Replace(".png", "_qrcode.png");

                guest.BusinessId = newEvent.Id;
                guest.AppBusinessName = newEvent.AppBusinessName;
                guest.BusinessLogo = newEvent.BusinessLogoPath;
                guest.InvitePath = invitePath;
                guest.InviteDownloadPath = inviteDownloadPath;


                emailTemplate = await this.RenderViewAsync("_EmailTemplate1", new EmailTemplateModel { Business = newEvent, Guest = guest, InvitePath = invitePath, NoOfInvites = noOfInvites, newEvent = newEvent, FullName = fullname, QrCode = qrCode, Telephone = telephone, InviteAcceptPath = inviteAcceptPath }, true);

                //emailTemplate = RenderViewAsString(, "_EmailTemplate");
                email = "leboston@yahoo.com";
                MailMessage mail = new MailMessage("info@iencircle.com ", email, newEvent.BusinessEmailSubject, emailTemplate);
                mail.From = new MailAddress("info@iencircle.com ", "Inkheart Studio");
                mail.IsBodyHtml = true; // necessary if you're using html email
               
                NetworkCredential credential = new NetworkCredential("azure_a85c8105e9f55cd2be7e4a439af82d66@azure.com", "Lauren280701");
             
               
                SmtpClient smtp = new SmtpClient("smtp.sendgrid.net", 587);

                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = credential;
                
                smtp.Send(mail);
            }
            catch (Exception)
            {
                return 0;
            }

            return 1;
        }

        [HttpGet]
        [Route("Guests")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Guests(string id, string eventName)
        {
            var newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);

            var guests  = newEvent.Guests;

            var counter = 0;

            if(guests != null)
            {
               counter = guests.Count(x => x.Processed);

                //foreach (var item in guests)
                //{
                //    item.QRCode = GenerateQR(item, newEvent.Id);
                //}

                var allguests = guests.OrderBy(x => x.Fullname).ToList();

                return View(new EventModel { Counter = counter, EventLogo = newEvent.BusinessLogoPath, AppBusinessName = eventName, Id = id, Guests = allguests });
            }

            var allguestsNew = new List<Guest>();

            return View(new EventModel { Counter = counter, EventLogo = newEvent.BusinessLogoPath, AppBusinessName = eventName, Id = id, Guests = allguestsNew });
        }

        private string GenerateQR(Guest item, string id, int size = 200)
        {
            //string fullname, string description, string telephone, string noofinvites
            var txtCode = "https://iencircle.azurewebsites.net/guest/?id=" + id + "&guestId=" + item.Id;

            var qrCode = "";

            using (MemoryStream ms = new MemoryStream())
            {
                var writer = new BarcodeWriter();
                writer.Format = BarcodeFormat.QR_CODE;
                var result = writer.Write(txtCode);

                using (Bitmap bitMap = result)
                {
                    bitMap.Save(ms, ImageFormat.Png);
                    qrCode = "data:image/png;base64," + Convert.ToBase64String(ms.ToArray());
                }
            }

            return qrCode;


            //var url = string.Format("http://chart.apis.google.com/chart?cht=qr&chs={1}x{2}&chl={0}", txtCode, size, size);
            //WebResponse response = default(WebResponse);
            //Stream remoteStream = default(Stream);
            //StreamReader readStream = default(StreamReader);
            //WebRequest request = WebRequest.Create(url);
            //response = request.GetResponse();
            //remoteStream = response.GetResponseStream();
            //readStream = new StreamReader(remoteStream);

            //var bytes = default(byte[]);
            //string result;
            //using (var memstream = new MemoryStream())
            //{
            //    remoteStream.CopyTo(memstream);
            //    bytes = memstream.ToArray();
            //    result = "data:image/png;base64," + Convert.ToBase64String(bytes);
            //}

            //response.Close();
            //remoteStream.Close();
            //readStream.Close();

            //return result;

            //System.Drawing.Image img = System.Drawing.Image.FromStream(remoteStream);
            //img.Save("D:/QRCode/" + txtCode.Text + ".png");

            //txtCode.Text = string.Empty;
            //txtWidth.Text = string.Empty;
            //txtHeight.Text = string.Empty;
            //lblMsg.Text = "The QR Code generated successfully";
        }

        
        [HttpPost]
        public async Task<IActionResult> EditGuestDetail(EventModel model)
        {
            //var events = await _cosmosDbProvider.GetItems(model.Id,model.AppBusinessName);

            var newEvent = await _cosmosDbProvider.GetFastBusinesss(model.Id);

            var items = new List<Guest>();

            var updatedGuests = new List<Guest>();

            Guest newGuestCreation = null;

            if (newEvent.Guests != null)
            {
                items = newEvent.Guests.ToList();
                newGuestCreation = items.FirstOrDefault(x => x.Id == model.Guest.Id);
            }

            var guestAdded = false;

            if(newGuestCreation != null)
            {
                guestAdded = true;

                foreach (var item in items)
                {
                    item.QRCode = GenerateQR(item, newEvent.Id);

                    var existingItem = item;

                    if (!string.IsNullOrEmpty(item.Telephone) && item.Telephone == model.Guest.Telephone)
                    {
                        if (null != existingItem)
                        {
                            if (!string.IsNullOrEmpty(model.Guest.TableNum))
                            {
                                existingItem.TableNum = model.Guest.TableNum;
                            }
                            //done

                            if (!string.IsNullOrEmpty(model.Guest.Seat))
                            {
                                existingItem.Seat = model.Guest.Seat;
                            }

                            if (!string.IsNullOrEmpty(model.Guest.Fullname))
                            {
                                existingItem.Fullname = model.Guest.Fullname;
                            }

                            if (!string.IsNullOrEmpty(model.Guest.Email))
                            {
                                existingItem.Email = model.Guest.Email;
                            }

                            if (!string.IsNullOrEmpty(model.Guest.Status))
                            {
                                existingItem.Status = model.Guest.Status;
                            }

                            if (model.Guest.NoOfInvites > 0)
                            {
                                existingItem.NoOfInvites = model.Guest.NoOfInvites;
                            }

                            existingItem.IsActive = model.Guest.IsActive;

                            updatedGuests.Add(existingItem);
                        }
                    }
                    else
                    {
                        if (null != existingItem)
                        {
                            updatedGuests.Add(existingItem);
                        }
                    }
                }
            }
            else
            {
                if(!string.IsNullOrEmpty(model.Guest.Telephone) && !string.IsNullOrEmpty(model.Guest.Fullname))
                {
                    guestAdded = true;
                    updatedGuests.AddRange(items);
                    updatedGuests.Add(model.Guest);
                }
            }

            if(guestAdded)
            {
                newEvent.Guests = updatedGuests.ToArray();
                await _cosmosDbProvider.ReplaceNewBusinessItem(model.AppBusinessName, model.Id, newEvent);
            }
           

            return RedirectToAction("Guests", new { id = model.Id, eventName = model.AppBusinessName });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewBusiness(CosmosDBEntities.Models.Business newBusiness, string BusinessDate, int BusinessNewType,
            List<IFormFile> menufiles, List<IFormFile> programfiles, List<IFormFile> mapfiles, List<IFormFile> logofiles, List<IFormFile> headerlogofiles, List<IFormFile> sponsorsfiles,
            List<IFormFile> mcfiles, List<IFormFile> comedianfiles, List<IFormFile> catererfiles, List<IFormFile> plannerfiles,
            List<IFormFile> food1files, List<IFormFile> food2files, List<IFormFile> food3files, List<IFormFile> chairmanfiles, List<IFormFile> djfiles)
        {

            //var all = await _dbProvider.GetBusinesss();

            //foreach(var b in all)
            //{
            //    await _dbProvider.AddBusinessToContainerNew(b);
            //}



            //var user = await _userManager.GetUserAsync(User);

            DateTime? dt = null;
            DateTime? newdate = null;
            bool newBusinessCreation = false;

            if (BusinessNewType != 0)
            {
                newBusiness.BusinessType = BusinessNewType;
            }
            else
            {
                newBusiness.BusinessType = 5;
            }

            if (string.IsNullOrEmpty(newBusiness.Id))
            {
                newBusiness.Id = Guid.NewGuid().ToString();
                newBusinessCreation = true;
            }


            if (!string.IsNullOrEmpty(BusinessDate))
            {
                try
                {
                    dt = DateTime.ParseExact(BusinessDate, "mm/dd/yyyy",
                                       CultureInfo.InvariantCulture);

                    newdate = Convert.ToDateTime(dt.Value.ToString("dd/mm/yyyy"));
                }
                catch
                {
                    newdate = Convert.ToDateTime(BusinessDate);
                }
            }

            if (chairmanfiles.Count > 0 && !string.IsNullOrEmpty(newBusiness.BusinessChairman))
            {
                newBusiness.BusinessChairmanPic = await SaveFiles(chairmanfiles[0], newBusiness.Id, true, false, false, true);
            }

            if (menufiles.Count > 0)
            {
                newBusiness.MenuPath = await SaveFiles(menufiles[0], newBusiness.Id, false);
            }

            if (programfiles.Count > 0)
            {
                newBusiness.ProgramPath = await SaveFiles(programfiles[0], newBusiness.Id, false);
            }

            if (mapfiles.Count > 0)
            {
                newBusiness.MapPath = await SaveFiles(mapfiles[0], newBusiness.Id, true, false, false, true);
            }

            if (logofiles.Count > 0)
            {
                newBusiness.BusinessLogoPath = await SaveFiles(logofiles[0], newBusiness.Id, true, false, false, true);
            }

            if (headerlogofiles.Count > 0)
            {
                newBusiness.BusinessHeaderLogoPath = await SaveFiles(headerlogofiles[0], newBusiness.Id, true, false, false, true);
            }

            if (sponsorsfiles.Count > 0 && !string.IsNullOrEmpty(newBusiness.BusinessSponsors))
            {
                newBusiness.BusinessSponsorsLogo = await SaveFiles(sponsorsfiles[0], newBusiness.Id, true, false, false, true);
            }

            if (!string.IsNullOrEmpty(newBusiness.Id))
            {
                //string id, string internetId, string internetPassword
                var fullUrl = $"https://cabbashutility.azurewebsites.net/api/url?id={newBusiness.Id}";
                var cabbashCode = "";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage res = await client.GetAsync(fullUrl))
                        {
                            using (HttpContent content = res.Content)
                            {
                                try
                                {
                                    var jsoninvitePath = await content.ReadAsStringAsync();
                                    cabbashCode = JsonConvert.DeserializeObject<string>(jsoninvitePath);
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    cabbashCode = "";
                }

                newBusiness.MyCabbashQrCode = cabbashCode;
            }

            if (!string.IsNullOrEmpty(newBusiness.BusinessInternetId) && !string.IsNullOrEmpty(newBusiness.BusinessInternetPassword))
            {
                //string id, string internetId, string internetPassword
                var fullUrl = $"https://cabbashutility.azurewebsites.net/api/Wifi?id={newBusiness.Id}&internetId={newBusiness.BusinessInternetId}&internetPassword={newBusiness.BusinessInternetPassword}";
                //var fullUrl = $"http://localhost:51521/api/Wifi?id={newBusiness.Id}&internetId={newBusiness.BusinessInternetId}&internetPassword={newBusiness.BusinessInternetPassword}";


                var invitePath = "";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage res = await client.GetAsync(fullUrl))
                        {
                            using (HttpContent content = res.Content)
                            {
                                try
                                {
                                    var jsoninvitePath = await content.ReadAsStringAsync();
                                    invitePath = JsonConvert.DeserializeObject<string>(jsoninvitePath);
                                }
                                catch (Exception ex)
                                {
                                    int yy = 90;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    invitePath = "";
                }

                newBusiness.BusinessInternetPath = invitePath;
            }


            if (mcfiles.Count > 0 && !string.IsNullOrEmpty(newBusiness.BusinessMC))
            {
                newBusiness.BusinessMCPic = await SaveFiles(mcfiles[0], newBusiness.Id, true, false, false, true);
            }

            if (comedianfiles.Count > 0 && !string.IsNullOrEmpty(newBusiness.BusinessComedian))
            {
                newBusiness.BusinessComedianPic = await SaveFiles(comedianfiles[0], newBusiness.Id, true, false, false, true);
            }

            if (catererfiles.Count > 0 && !string.IsNullOrEmpty(newBusiness.BusinessCaterer))
            {
                newBusiness.BusinessCatererPic = await SaveFiles(catererfiles[0], newBusiness.Id, true, false, false, true);
            }

            if (plannerfiles.Count > 0 && !string.IsNullOrEmpty(newBusiness.BusinessPlaner))
            {
                newBusiness.BusinessPlanerPic = await SaveFiles(plannerfiles[0], newBusiness.Id, true, false, false, true);
            }

            if (food1files.Count > 0 && !string.IsNullOrEmpty(newBusiness.FoodName1))
            {
                newBusiness.FoodPic1 = await SaveFiles(food1files[0], newBusiness.Id);
            }

            if (food2files.Count > 0 && !string.IsNullOrEmpty(newBusiness.FoodName2))
            {
                newBusiness.FoodPic2 = await SaveFiles(food2files[0], newBusiness.Id);
            }

            if (food3files.Count > 0 && !string.IsNullOrEmpty(newBusiness.FoodName3))
            {
                newBusiness.FoodPic3 = await SaveFiles(food3files[0], newBusiness.Id);
            }

            if (chairmanfiles.Count > 0 && !string.IsNullOrEmpty(newBusiness.BusinessChairman))
            {
                newBusiness.BusinessChairmanPic = await SaveFiles(chairmanfiles[0], newBusiness.Id, true, false, false, true);
            }

            if (djfiles.Count > 0 && !string.IsNullOrEmpty(newBusiness.BusinessDJ))
            {
                newBusiness.BusinessDJPic = await SaveFiles(djfiles[0], newBusiness.Id, true, false, false, true);
            }


            if (!newBusiness.BusinessDate.HasValue)
            {
                newBusiness.BusinessDate = newdate;
                newBusiness.IsRegistered = true;
            }



            if (newBusiness.Longitude.HasValue && newBusiness.Latitude.HasValue)
            {
                if (newBusiness.Latitude.Value > 0 && newBusiness.Longitude.Value > 0)
                {
                    newBusiness.LocationQRCode = await CreateLocationQRCode(newBusiness);
                    newBusiness.DiaryQRCode = await CreateDiaryQRCode(newBusiness);
                }
            }

            if (newBusinessCreation)
            {
                //newBusiness.IsActive = true;
                newBusiness.BusinessType = 5;
                newBusiness.IsEvent = true;
                newBusiness = await _cosmosDbProvider.AddBusinessToContainer(newBusiness);
            }
            else
            {
                //    var glee = await _dbProvider.GetBusinesss("54064877-d776-4a95-9db4-8d8298473109");
                //    newBusiness.AllergyDetails = glee.AllergyDetails;
                //    newBusiness.AppHandles = glee.AppHandles;
                //    newBusiness.BestDays = glee.BestDays;
                //    newBusiness.CarPark = glee.CarPark;
                //    newBusiness.CarParking = glee.CarParking;
                //    newBusiness.ChangeMyOrderDetails = glee.ChangeMyOrderDetails;
                //    newBusiness.CheckinTime = glee.CheckinTime;
                //    newBusiness.ClosingTime = glee.ClosingTime;
                //    newBusiness.CockerageDetails = glee.CockerageDetails;
                //    newBusiness.DeliveryDetails = glee.DeliveryDetails;
                //    newBusiness.DeliverySpecificTime = glee.DeliverySpecificTime;
                //    newBusiness.DepartureTime = glee.DepartureTime;
                //    newBusiness.Dinning = glee.Dinning;
                //    newBusiness.DisabilityAccess = glee.DisabilityAccess;
                //    newBusiness.Discounts = glee.Discounts;
                //    newBusiness.DoYouAllowPets = glee.DoYouAllowPets;
                //    newBusiness.DoYouCaterForChildren = glee.DoYouCaterForChildren;
                //    newBusiness.Exit = glee.Exit;
                //    newBusiness.FavouriteMeal = glee.FavouriteMeal;
                //    newBusiness.FireExit = glee.FireExit;
                //    newBusiness.FirePrecautions = glee.FirePrecautions;
                //    newBusiness.FirstAid = glee.FirstAid;
                //    newBusiness.Fitness = glee.Fitness;
                //    newBusiness.FoodPreparationTime = glee.FoodPreparationTime;
                //    newBusiness.ForgotSomething = glee.ForgotSomething;
                //    newBusiness.FreeOrComplimentary = glee.FreeOrComplimentary;
                //    newBusiness.FrontOffice = glee.FrontOffice;
                //    newBusiness.KidsMenuDetails = glee.KidsMenuDetails;
                //    newBusiness.LoyaltyScheme = glee.LoyaltyScheme;
                //    newBusiness.OpeningTime = glee.OpeningTime;
                //    newBusiness.PartyHire = glee.PartyHire;
                //    newBusiness.PaymentDetails = glee.PaymentDetails;
                //    newBusiness.RefundDetails = glee.RefundDetails;
                //    newBusiness.ServiceChargePolicy = glee.ServiceChargePolicy;
                //    newBusiness.Smoking = glee.Smoking;

                //    newBusiness.SpecialDateCelebration = glee.SpecialDateCelebration;
                //    newBusiness.SpicyMeals = glee.SpicyMeals;
                //    newBusiness.TableReservationDetails = glee.TableReservationDetails;
                //    newBusiness.Toilet = "We do not have a toilets as we a delivery online.";
                //    newBusiness.VegetarianOptions = glee.VegetarianOptions;

                //    newBusiness.Website = "www.mywaks.com";

                var roomTypes = new List<RoomType>();

                roomTypes.Add(new RoomType { ActualHotelType = 0, BusinessId = newBusiness.Id, MinimumDeposit = 40000, Price = 40000, Name = "ALL", Description = "ALL", Id = Guid.NewGuid().ToString() });
                roomTypes.Add(new RoomType { ActualHotelType = 2017, BusinessId = newBusiness.Id, MinimumDeposit = 40000, Price = 40000, Name = "DELUXE", Description = "DELUXE", Id = Guid.NewGuid().ToString() });
                roomTypes.Add(new RoomType { ActualHotelType = 2018, BusinessId = newBusiness.Id, MinimumDeposit = 55000, Price = 55000, Name = "EXECUTIVE", Description = "EXECUTIVE", Id = Guid.NewGuid().ToString() });
                roomTypes.Add(new RoomType { ActualHotelType = 2109, BusinessId = newBusiness.Id, MinimumDeposit = 30000, Price = 30000, Name = "CLASSIC", Description = "CLASSIC", Id = Guid.NewGuid().ToString() });
                roomTypes.Add(new RoomType { ActualHotelType = 2021, BusinessId = newBusiness.Id, MinimumDeposit = 25000, Price = 25000, Name = "STANDARD", Description = "STANDARD", Id = Guid.NewGuid().ToString() });

                newBusiness.RoomTypes = roomTypes.ToArray();

                await _cosmosDbProvider.ReplaceNewBusinessItem(newBusiness.BusinessName, newBusiness.Id, newBusiness);

            }

            return RedirectToAction("EditEvent", new { id = newBusiness.Id, eventName = newBusiness.AppBusinessName });
        }

        private async Task<string> SaveFilesNonImage(IFormFile formFile, string businessId)
        {
            string filePath = "";

            try
            {
                var fileName = formFile.FileName;

                using (var stream = formFile.OpenReadStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        byte[] imageBytes = ms.ToArray();
                        filePath = await SaveFile(imageBytes, fileName, businessId);
                    }

                }

            }
            catch (Exception)
            {
            }


            return filePath;
        }

        private async Task<string> SaveFiles(IFormFile formFile, string businessId, bool isImage = true, bool useThumbnail = false, bool iscategoryImage = false, bool useSquareImage = false)
        {

            if (!isImage)
            {
                return await SaveFilesNonImage(formFile, businessId);
            }

            string filePath = "";

            try
            {
                var fileName = formFile.FileName;

                using (var stream = formFile.OpenReadStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);

                        byte[] imageBytes = ms.ToArray();

                        try
                        {

                            var base64 = Convert.ToBase64String(imageBytes);

                            var strBus = JsonConvert.SerializeObject(base64);


                            //var request = new HttpRequestMessage(HttpMethod.Get, "https://cabbashutility.azurewebsites.net/api/imageresizer/");
                            var request = new HttpRequestMessage(HttpMethod.Get, "https://cabbashutility.azurewebsites.net/api/business/");


                            if (useThumbnail)
                            {
                                request = new HttpRequestMessage(HttpMethod.Get, "https://cabbashutility.azurewebsites.net/api/thumbnail/");
                            }
                            else if (iscategoryImage)
                            {
                                request = new HttpRequestMessage(HttpMethod.Get, "https://cabbashutility.azurewebsites.net/api/category/");
                            }
                            else if (useSquareImage)
                            {
                                request = new HttpRequestMessage(HttpMethod.Get, "https://cabbashutility.azurewebsites.net/api/square/");
                            }

                            request.Content = new StringContent(strBus);

                            var cabbashCode = "";

                            using (var client = new HttpClient())
                            using (HttpResponseMessage response = await client.SendAsync(request))
                            {
                                var b = response.StatusCode;
                                var responsestr = await response.Content.ReadAsStringAsync();
                                cabbashCode = JsonConvert.DeserializeObject<string>(responsestr);
                            }

                            var newImage = cabbashCode;

                            byte[] newImagebytes = Convert.FromBase64String(newImage);

                            filePath = await SaveFile(newImagebytes, fileName, businessId);

                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                }

            }
            catch (Exception)
            {
            }


            return filePath;
        }

        private async Task<string> SaveFile(Byte[] data, string filename, string BusinessId)
        {
            string accountname = "cabbashstorage";

            string accesskey = "7w2G8CTPGZC7W6xXUvJAGRd467gXEyDlxzj3+m6PtTsDu2sZ2cKJdhygUw4MjitANP30TLtLgYqy6vgvCyp+rA==";

            try

            {

                StorageCredentials creden = new StorageCredentials(accountname, accesskey);

                CloudStorageAccount acc = new CloudStorageAccount(creden, useHttps: true);

                CloudBlobClient client = acc.CreateCloudBlobClient();

                CloudBlobContainer cont = client.GetContainerReference(BusinessId);

                await cont.CreateIfNotExistsAsync();

                await cont.SetPermissionsAsync(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });

                CloudBlockBlob cblob = cont.GetBlockBlobReference(filename);

                await cblob.UploadFromByteArrayAsync(data, 0, data.Length);

                return cblob.Uri.AbsoluteUri;

            }
            catch (Exception ex)
            {
                int p = 909;
            }

            return "";
        }


        private async Task<string> CreateLocationQRCode(Business newBusiness)
        {
            var returnPath = "";

            var fullUrl = $"https://cabbashutility.azurewebsites.net/api/Location?id={newBusiness.Id}&eventName={newBusiness.AppBusinessName}&latitude={newBusiness.Latitude.Value}&longitude={newBusiness.Longitude.Value}&businessDate={DateTime.Now.AddDays(-7)}";

            //fullUrl = $"http://localhost:58874/api/Location?id={newBusiness.Id}&eventName={newBusiness.AppBusinessName}&latitude={newBusiness.Latitude.Value}&longitude={newBusiness.Longitude.Value}";



            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(fullUrl))
                    {
                        using (HttpContent content = res.Content)
                        {
                            try
                            {
                                var jsoninvitePath = await content.ReadAsStringAsync();
                                returnPath = JsonConvert.DeserializeObject<string>(jsoninvitePath);
                            }
                            catch (Exception)
                            {

                            }
                        }
                    }
                }


            }
            catch
            {

            }

            return returnPath;
        }

        private async Task<string> CreateDiaryQRCode(Business newBusiness)
        {
            var returnPath = "";

            var address = "The Dome, This Present House, Freedom Way By, Lekki Phase 1, Lagos, Nigeria";

            if(!string.IsNullOrEmpty(newBusiness.Street))
            {
                address = newBusiness.Street;
            }

            var startDate = newBusiness.BusinessDate.Value.AddHours(14);
            var endDate = newBusiness.BusinessDate.Value.AddHours(22);
            // public HttpResponseMessage Get(string id, string eventName, decimal? latitude, decimal? longitude, DateTime? businessStartDate, DateTime? businessEndDate, string address)


            var fullUrl = $"https://cabbashutility.azurewebsites.net/api/diary?id={newBusiness.Id}&eventName={newBusiness.AppBusinessName}&latitude={newBusiness.Latitude.Value}&longitude={newBusiness.Longitude.Value}&businessStartDate={startDate}&businessEndDate={endDate}&address={address}";
                                   //var fullUrl = $"http://localhost:51521/api/diary?id={newBusiness.Id}&eventName={newBusiness.AppBusinessName}&latitude={newBusiness.Latitude.Value}&longitude={newBusiness.Longitude.Value}&businessStartDate={startDate}&businessEndDate={endDate}&address={address}";

            

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (HttpResponseMessage res = await client.GetAsync(fullUrl))
                    {
                        using (HttpContent content = res.Content)
                        {
                            try
                            {
                                var jsoninvitePath = await content.ReadAsStringAsync();
                                returnPath = JsonConvert.DeserializeObject<string>(jsoninvitePath);
                            }
                            catch (Exception ex)
                            {
                                returnPath = ex.Message;
                            }
                        }
                    }
                }


            }
            catch
            {

            }

            return returnPath;
        }

        public async Task<IActionResult> Index()
        {
            //await _cosmosDbProvider.DeleteEventItem("UEFA Yearly Event", "1");
            //await _cosmosDbProvider.DeleteEventItem("The Unofficial Christmas Party", "2");
            //await _cosmosDbProvider.DeleteEventItem("Heineken Shine On Event", "3");

            //var events = await _cosmosDbProvider.GetFastBusinesssAllWaks();

            //events = events.Where(x => x.BusinessType == 4).ToList();

            //foreach (var v in events)
            //{
            //    await _cosmosDbProvider.DeleteBusinessItem(v.AppBusinessName, v.Id);
            //}


            //return View(events.OrderByDescending(x =>  x.BusinessDate).ThenBy(x => x.AppBusinessName).ToList());
            return View();
        }

        [HttpGet]
        [Route("IndexEvent")]
        [Route("{id}")]
        public async Task<IActionResult> IndexEvent(int? id)
        {
            //await _cosmosDbProvider.DeleteEventItem("Uefa Champions League Event", "UefaChampionsLeagueEvent");
             var events = await _cosmosDbProvider.GetFastBusinesssAllEventsCached(true);

            if(id.HasValue && id.Value == 999)
            {
                events = events.Where(x => !x.IsRegistered).OrderByDescending(x => x.BusinessDate).ThenBy(x => x.AppBusinessName).ToList();
            }
            else
            {
                events = events.Where(x => x.IsRegistered).OrderByDescending(x => x.BusinessDate).ThenBy(x => x.AppBusinessName).ToList();
            }

            return View(events);
        }

        public async Task<IActionResult> UpdateBusiness(string id, string eventName)
        {

            Business newEvent = null;

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(eventName))
            {
               
                newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);
            }

            var model = new CosmosDBEntities.Models.Business { };

            if (newEvent != null)
            {
                model = newEvent;
            }

            return View("Business", model);
        }

        
        [HttpGet]
        [Route("QrEvent")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> QrEvent(string id, string eventName)
        {

            Business newEvent = null;

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(eventName))
            {
                newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);
            }

            var model = new CosmosDBEntities.Models.Business { };

            if (newEvent != null)
            {
                model = newEvent;

                var botUrl = $"https://iencircle.azurewebsites.net/?id={newEvent.Id}";

                var fullUrl = $"https://businesscardgeneratorlive.azurewebsites.net/api/Url?Id={botUrl}";
                var invitePath = "";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        using (HttpResponseMessage res = await client.GetAsync(fullUrl))
                        {
                            using (HttpContent content = res.Content)
                            {
                                try
                                {
                                    var jsoninvitePath = await content.ReadAsStringAsync();
                                    invitePath = JsonConvert.DeserializeObject<string>(jsoninvitePath);
                                    model.DiaryQRCode = "data:image/png;base64," + invitePath;
                                }
                                catch (Exception)
                                {

                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }


            }

            return View(model);
        }

        [HttpGet]
        [Route("EditEvent")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> EditEvent(string id, string eventName)
        {
           
            Business newEvent = null;

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(eventName))
            {
                newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);
            }

            var model = new CosmosDBEntities.Models.Business { };

            if (newEvent != null)
            {
                model = newEvent;

                //if(newEvent.BusinessDate.HasValue && newEvent.BusinessDate.Value < DateTime.Now)
                //{
                //    return RedirectToAction("IndexEvent");
                //}
            }

            return View(model);
        }

        [HttpGet]
      
        public async Task<IActionResult> Business(string id, string eventName)
        {

            Business newEvent = null;

            if (!string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(eventName))
            {
                newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);
            }

            var model = new CosmosDBEntities.Models.Business { BusinessDate = DateTime.Now};

            if(newEvent != null)
            {
                model = newEvent;
            }

            return View("NewEvent",model);
        }


        [HttpPost]
        [Route("Import")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Import(string id, string eventName, int? dummy)
        {
            try
            {
                IFormFile file = Request.Form.Files[0];

                using (var stream = file.OpenReadStream())
                {
                    using (SpreadsheetDocument doc = SpreadsheetDocument.Open(stream, false))

                    {
                        //create the object for workbook part  
                        WorkbookPart workbookPart = doc.WorkbookPart;
                        Sheets thesheetcollection = workbookPart.Workbook.GetFirstChild<Sheets>();
                        //StringBuilder excelResult = new StringBuilder();

                        int counter = 0;

                        //using for each loop to get the sheet from the sheetcollection  
                        foreach (Sheet thesheet in thesheetcollection)
                        {

                            //excelResult.AppendLine("Excel Sheet Name : " + thesheet.Name);
                            //excelResult.AppendLine("----------------------------------------------- ");
                            //statement to get the worksheet object by using the sheet id  
                            Worksheet theWorksheet = ((WorksheetPart)workbookPart.GetPartById(thesheet.Id)).Worksheet;

                            SheetData thesheetdata = (SheetData)theWorksheet.GetFirstChild<SheetData>();

                            List<Guest> guestList = new List<Guest>();

                            foreach (Row thecurrentrow in thesheetdata)
                            {
                                counter++;

                                if (counter == 1) continue;

                                var cellCounter = 0;

                                var gu = Guid.NewGuid().ToString();

                                var g = new Guest { Id = gu, Status = "OTHERS", Telephone = "NOT SUPPLIED", IsActive = true, BusinessId = id, AppBusinessName = eventName};

                                foreach (Cell thecurrentcell in thecurrentrow)
                                {
                                    //statement to take the integer value  
                                    string currentcellvalue = string.Empty;

                                    if (thecurrentcell.DataType != null || thecurrentcell.CellValue != null)
                                    {
                                        if (thecurrentcell.DataType != null && thecurrentcell.DataType == CellValues.SharedString)
                                        {
                                            int idNew;

                                            if (Int32.TryParse(thecurrentcell.InnerText, out idNew))
                                            {
                                                SharedStringItem item = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ElementAt(idNew);
                                                //Name Company Email Telephone Table Seat NumberOfInvites Status
                                                if (item.Text != null)
                                                {
                                                    //code to take the string
                                                    var result = item.Text.Text;

                                                    if(cellCounter == 0)
                                                    {
                                                        g.Fullname = result;
                                                    }
                                                    else if (cellCounter == 1)
                                                    {
                                                        g.CompanyName = result;
                                                    }
                                                    else if (cellCounter == 2)
                                                    {
                                                        g.Email = result;
                                                        g.Email = "leboston@yahoo.com";

                                                    }
                                                    else if (cellCounter == 3)
                                                    {
                                                        g.Telephone = result;
                                                    }
                                                    else if (cellCounter == 4)
                                                    {
                                                        g.TableNum = result;
                                                    }
                                                    else if (cellCounter == 5)
                                                    {
                                                        g.Seat = result;
                                                    }
                                                    else if (cellCounter == 6)
                                                    {
                                                        g.NoOfInvites = 1;
                                                    }
                                                    else if (cellCounter == 6)
                                                    {
                                                        g.Status = result;
                                                    }
                                                }
                                                else if (item.InnerText != null)
                                                {
                                                    currentcellvalue = item.InnerText;
                                                }
                                                else if (item.InnerXml != null)
                                                {
                                                    currentcellvalue = item.InnerXml;
                                                }
                                            }
                                        }
                                        else if (thecurrentcell.CellValue != null)
                                        {
                                            var result = thecurrentcell.CellValue.Text;

                                            if (cellCounter == 0)
                                            {
                                                g.Fullname = result;
                                            }
                                            else if (cellCounter == 1)
                                            {
                                                g.CompanyName = result;
                                            }
                                            else if (cellCounter == 2)
                                            {
                                                g.Email = result;
                                                g.Email = "leboston@yahoo.com";

                                            }
                                            else if (cellCounter == 3)
                                            {
                                                g.Telephone = result;
                                            }
                                            else if (cellCounter == 4)
                                            {
                                                g.TableNum = result;
                                            }
                                            else if (cellCounter == 5)
                                            {
                                                g.Seat = result;
                                            }
                                            else if (cellCounter == 6)
                                            {
                                                g.NoOfInvites = 1;
                                            }
                                            else if (cellCounter == 6)
                                            {
                                                g.Status = result;
                                            }

                                        }
                                    }
                                   
                                    cellCounter++;
                                }

                                if(!string.IsNullOrEmpty(g.Fullname))
                                {
                                    guestList.Add(g);
                                }

                             
                            }


                            if(guestList.Any())
                            {
                                await SaveToCosmosDB(id, eventName, guestList);
                            }
                            
                        }
                    }
                }
                
            }
            catch (Exception)
            {
                
            }

             var events = await _cosmosDbProvider.GetFastBusinesssAllEventsCached(true);

            var newEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);

            var guests = newEvent.Guests;

            if(guests != null)
            {

                var items = guests.OrderBy(x => x.Fullname).ToList();
                return View("Guests",new EventModel { AppBusinessName = eventName, Id = id, Guests = items });
            }

            var allguestsNew = new List<Guest>();
            return View("Guests", new EventModel { AppBusinessName = eventName, Id = id, Guests = allguestsNew });

        }

        private async Task<bool> SaveToCosmosDB(string id, string eventName, List<Guest> guests, bool overideAll = false)
        {
            var thisEvent = await _cosmosDbProvider.GetFastBusinesss(id, true);

            var eventGuests = thisEvent.Guests;

            var listOfUptoDateGuests = new List<Guest>();

            //overideAll = true;

            if (overideAll)
            {
                listOfUptoDateGuests = guests;
            }
            else
            {
                if (eventGuests != null)
                {
                    foreach (var g in guests)
                    {
                        var existingGuest = eventGuests.FirstOrDefault(x => x.Telephone == g.Telephone);

                        if (existingGuest != null)
                        {
                            existingGuest.Fullname = g.Fullname;
                            existingGuest.Email = g.Email;
                            existingGuest.NoOfInvites = g.NoOfInvites;
                            existingGuest.Status = g.Status;
                            existingGuest.Telephone = g.Telephone;
                            listOfUptoDateGuests.Add(existingGuest);
                        }
                        else
                        {
                            listOfUptoDateGuests.Add(g);
                        }
                    }
                }
                else
                {
                    listOfUptoDateGuests = guests;
                }
            }

            var newEvent = new Business { Id = id, AppBusinessName = eventName, Guests = listOfUptoDateGuests.ToArray() };
            await _cosmosDbProvider.ReplaceNewBusinessItem(eventName, id, newEvent);
            return true;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
