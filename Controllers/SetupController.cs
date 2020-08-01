using CosmosDBService;
using IdentitySample.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using System;
using CosmosDBEntities.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Net.Http;
using Newtonsoft.Json;
using IencircleAdmin.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.SignalR;
using System.Net;
using System.Web;
using System.Collections.Specialized;

namespace IencircleAdmin.Controllers
{
    [Authorize()]
    public class Setup : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        private IDbProvider _dbProvider = null;
        //private static AzureSignalR signalR = new AzureSignalR(Environment.GetEnvironmentVariable("AzureSignalRConnectionString"));



        public Setup(UserManager<ApplicationUser> userManager, IMemoryCache cache, IDbProvider dbProvider)
        {

            _userManager = userManager;
            _dbProvider = dbProvider;
            _dbProvider.SetCache(cache);
        }
        //AdjustAddProduct
  
        private async Task<bool> CreateOrUpdateGuestRecordWithMacAdd(Business newBusiness)
        {
            var pd = await _dbProvider.GetGuestRecord(newBusiness.MacAddress, newBusiness.Id);

            if (pd == null)
            {
                var gr = new CosmosDBEntities.Model.GuestRecord();
                gr.MacAddress = newBusiness.MacAddress;
                gr.BusinessId = newBusiness.Id;
                gr.Id = newBusiness.MacAddress;

                await _dbProvider.InsertGuestRecord(gr);
            }



            return true;
        }

        [HttpPost]
        public async Task<IActionResult> EditStockDetail(Stock model, List<IFormFile> stockfiles, string stockId, string category, string id)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            if (null != model)
            {
                var thisBusiness = await _dbProvider.GetFastBusinesss(id);
                ////var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
                newBusiness = thisBusiness;

                model.Name = MakeCamelcase(model.Name);
                model.Description = MakeCamelcase(model.Description);
                model.FoodDescription = MakeCamelcase(model.FoodDescription);
            }

            var items = new List<Stock>();

            var updatedGuests = new List<Stock>();

            Stock newGuestCreation = null;

            if (newBusiness.Stocks != null)
            {
                items = newBusiness.Stocks.ToList();
                newGuestCreation = items.FirstOrDefault(x => x.Id == stockId);
            }

            var guestAdded = false;

            if (stockfiles.Count > 0)
            {
                model.PicturePath = "";
                model.PicturePath = await SaveFiles(stockfiles[0], newBusiness.Id, true, true);
            }


            if (newGuestCreation != null)
            {
                guestAdded = true;

                foreach (var item in items)
                {
                    var existingItem = item;

                    if (existingItem.Id == stockId)
                    {

                        existingItem.Name = model.Name;
                        existingItem.Description = model.Description;

                        existingItem.FoodDescription = model.FoodDescription;

                        existingItem.Price = model.Price;
                        existingItem.Currency = model.Currency;

                        if (newBusiness.Categories != null)
                        {
                            var cat = newBusiness.Categories.FirstOrDefault(x => x.Id == category);
                            if (cat != null)
                            {
                                existingItem.Category = cat;
                            }
                        }
                        //existingItem.Category = model.Category;

                        existingItem.IsActive = model.IsActive;

                        if (!string.IsNullOrEmpty(model.PicturePath))
                        {
                            existingItem.PicturePath = model.PicturePath;
                        }

                        updatedGuests.Add(existingItem);
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
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Description))
                {
                    guestAdded = true;
                    model.Id = Guid.NewGuid().ToString();
                    model.BusinessId = newBusiness.Id;
                    model.IsActive = true;

                    if (newBusiness.Categories != null)
                    {
                        var cat = newBusiness.Categories.FirstOrDefault(x => x.Id == category);

                        if (cat != null)
                        {
                            model.Category = cat;
                        }
                    }



                    updatedGuests.AddRange(items);
                    updatedGuests.Add(model);
                }
            }

            if (guestAdded)
            {
                newBusiness.Stocks = updatedGuests.Where(x => !string.IsNullOrEmpty(x.PicturePath)).ToArray();
                await _dbProvider.ReplaceNewBusinessItem(newBusiness.BusinessName, newBusiness.Id, newBusiness);
            }


            return RedirectToAction("Stocks", new { id = newBusiness.Id, eventName = newBusiness.AppBusinessName });
        }

        //EditCategoryDetail
        [HttpPost]
        public async Task<IActionResult> EditCategoryDetail(Category model, List<IFormFile> categoryfiles, string categoryId, string id)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            if (null != model)
            {
                var thisBusiness = await _dbProvider.GetFastBusinesss(id);
                ////var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
                newBusiness = thisBusiness;

                model.Name = MakeCamelcase(model.Name);
                model.Description = MakeCamelcase(model.Description);
            }

            var items = new List<Category>();

            var updatedGuests = new List<Category>();

            Category newGuestCreation = null;

            if (newBusiness.Categories != null)
            {
                items = newBusiness.Categories.ToList();
                newGuestCreation = items.FirstOrDefault(x => x.Id == categoryId);
            }

            var guestAdded = false;

            if (categoryfiles.Count > 0)
            {
                model.PicturePath = "";
                model.PicturePath = await SaveFiles(categoryfiles[0], newBusiness.Id, true, false, true);
            }


            if (newGuestCreation != null)
            {
                guestAdded = true;

                foreach (var item in items)
                {
                    var existingItem = item;

                    if (existingItem.Id == categoryId)
                    {

                        existingItem.Name = model.Name;
                        existingItem.Description = model.Description;


                        if (!string.IsNullOrEmpty(model.PicturePath))
                        {
                            existingItem.PicturePath = model.PicturePath;
                        }

                        updatedGuests.Add(existingItem);
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
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Description))
                {
                    guestAdded = true;
                    model.Id = Guid.NewGuid().ToString();
                    model.BusinessId = newBusiness.Id;
                    //model.IsActive = true;

                    updatedGuests.AddRange(items);
                    updatedGuests.Add(model);
                }
            }

            if (guestAdded)
            {
                newBusiness.Categories = updatedGuests.ToArray();
                await _dbProvider.ReplaceNewBusinessItem(newBusiness.BusinessName, newBusiness.Id, newBusiness);
            }


            return RedirectToAction("Categories", new { id = newBusiness.Id, eventName = newBusiness.AppBusinessName });
        }


        [HttpPost]
        public async Task<IActionResult> EditPlaceDetail(PlaceOfInterest model, string BusinessDate, string placeId, List<IFormFile> placefiles, string id)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            DateTime? dt = null;
            DateTime? newdate = null;

            if (null != model)
            {
                var thisBusiness = await _dbProvider.GetFastBusinesss(id);
                //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
                newBusiness = thisBusiness;


                model.Name = MakeCamelcase(model.Name);
                model.Description = MakeCamelcase(model.Description);
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

            var items = new List<PlaceOfInterest>();

            var updatedAdverts = new List<PlaceOfInterest>();

            PlaceOfInterest newGuestCreation = null;

            if (placefiles.Count > 0)
            {
                foreach (var f in placefiles)
                {
                    model.PicturePaths = new List<string>();
                    var picturePath = await SaveFiles(f, newBusiness.Id);
                    model.PicturePaths.Add(picturePath);
                }
            }

            if (newBusiness.PlacesOfInterest != null)
            {
                items = newBusiness.PlacesOfInterest.ToList();
                newGuestCreation = items.FirstOrDefault(x => x.Id == placeId);
            }

            var guestAdded = false;

            if (newGuestCreation != null)
            {
                guestAdded = true;

                foreach (var item in items)
                {
                    var existingItem = item;

                    if (existingItem.Id == placeId)
                    {

                        existingItem.Name = model.Name;
                        existingItem.Description = model.Description;
                        existingItem.BusinessDate = newdate.Value;
                        existingItem.OpeningTimes = model.OpeningTimes;
                        existingItem.ClosingTimes = model.ClosingTimes;
                        existingItem.Telephone = model.Telephone;
                        existingItem.Address = model.Address;
                        existingItem.Website = model.Website;
                        existingItem.IsActive = model.IsActive;

                        if (model.PicturePaths.Any())
                        {
                            existingItem.PicturePaths = model.PicturePaths;
                        }

                        updatedAdverts.Add(existingItem);
                    }
                    else
                    {
                        if (null != existingItem)
                        {
                            updatedAdverts.Add(existingItem);
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Description))
                {
                    guestAdded = true;
                    model.Id = Guid.NewGuid().ToString();
                    model.BusinessId = newBusiness.Id;
                    model.IsActive = true;
                    model.BusinessDate = newdate.Value;
                    model.PicturePaths = model.PicturePaths;
                    updatedAdverts.AddRange(items);
                    updatedAdverts.Add(model);
                }
            }

            if (guestAdded)
            {
                newBusiness.PlacesOfInterest = updatedAdverts.ToArray();
                await _dbProvider.ReplaceNewBusinessItem(newBusiness.BusinessName, newBusiness.Id, newBusiness);
            }

            return RedirectToAction("Places", new { id = newBusiness.Id, eventName = newBusiness.AppBusinessName });
        }

        [HttpPost]
        public async Task<IActionResult> EditSpeakerDetail(Speaker model, string speakerId, List<IFormFile> speakerfiles, string id)
        {
            //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;
            

            if (null != model)
            {
                var thisBusiness = await _dbProvider.GetFastBusinesss(id);
                newBusiness = thisBusiness;

                model.Name = MakeCamelcase(model.Name);
                model.Description = MakeCamelcase(model.Description);

            }

            var items = new List<Speaker>();

            var updatedSpeakers = new List<Speaker>();

            Speaker newGuestCreation = null;

            if (speakerfiles.Count > 0)
            {
                model.PicturePath = "";
                model.PicturePath = await SaveFiles(speakerfiles[0], newBusiness.Id);
            }

            if (newBusiness.Speakers != null)
            {
                items = newBusiness.Speakers.ToList();
                newGuestCreation = items.FirstOrDefault(x => x.Id == speakerId);
            }

            var guestAdded = false;

            if (newGuestCreation != null)
            {
                guestAdded = true;

                foreach (var item in items)
                {
                    var existingItem = item;

                    if (existingItem.Id == speakerId)
                    {

                        existingItem.Name = model.Name;
                        existingItem.Description = model.Description;
                        existingItem.Order = model.Order;

                        existingItem.IsActive = model.IsActive;

                        if (!string.IsNullOrEmpty(model.PicturePath))
                        {
                            existingItem.PicturePath = model.PicturePath;
                        }

                        updatedSpeakers.Add(existingItem);
                    }
                    else
                    {
                        if (null != existingItem)
                        {
                            updatedSpeakers.Add(existingItem);
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Description))
                {
                    guestAdded = true;
                    model.Id = Guid.NewGuid().ToString();
                    model.BusinessId = newBusiness.Id;
                    model.IsActive = true;
                    model.PicturePath = model.PicturePath;
                    updatedSpeakers.AddRange(items);
                    updatedSpeakers.Add(model);
                }
            }

            if (guestAdded)
            {
                newBusiness.Speakers = updatedSpeakers.ToArray();
                await _dbProvider.ReplaceNewBusinessItem(newBusiness.BusinessName, newBusiness.Id, newBusiness);
            }


            return RedirectToAction("Speakers", new { id = newBusiness.Id, eventName = newBusiness.AppBusinessName });
        }



        [HttpPost]
        public async Task<IActionResult> EditAdDetail(Advert model, string BusinessDate, string BusinessEndDate, string adId, List<IFormFile> adfiles, string id)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            DateTime? dt = null;
            DateTime? newdate = DateTime.Now;
            DateTime? newEnddate = newdate;

            if (null != model)
            {
                var thisBusiness = await _dbProvider.GetFastBusinesss(id);
                newBusiness = thisBusiness;

                model.Name = MakeCamelcase(model.Name);
                model.Description = MakeCamelcase(model.Description);

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

            if (!string.IsNullOrEmpty(BusinessEndDate))
            {
                try
                {
                    dt = DateTime.ParseExact(BusinessDate, "mm/dd/yyyy",
                                       CultureInfo.InvariantCulture);

                    newEnddate = Convert.ToDateTime(dt.Value.ToString("dd/mm/yyyy"));
                }
                catch
                {
                    newEnddate = Convert.ToDateTime(BusinessEndDate);
                }
            }

            var items = new List<Advert>();

            var updatedAdverts = new List<Advert>();

            Advert newGuestCreation = null;

            if (adfiles.Count > 0)
            {
                model.PicturePath = "";
                model.PicturePath = await SaveFiles(adfiles[0], newBusiness.Id);
            }

            if (newBusiness.Adverts != null)
            {
                items = newBusiness.Adverts.ToList();
                newGuestCreation = items.FirstOrDefault(x => x.Id == adId);
            }

            var guestAdded = false;

            if (newGuestCreation != null)
            {
                guestAdded = true;

                foreach (var item in items)
                {
                    var existingItem = item;

                    if (existingItem.Id == adId)
                    {

                        existingItem.Name = model.Name;
                        existingItem.Description = model.Description;
                        existingItem.BusinessDate = newdate.Value;
                        existingItem.BusinessEndDate = newEnddate.Value;
                        existingItem.Index = model.Index;

                        existingItem.IsActive = model.IsActive;

                        if (!string.IsNullOrEmpty(model.PicturePath))
                        {
                            existingItem.PicturePath = model.PicturePath;
                        }

                        updatedAdverts.Add(existingItem);
                    }
                    else
                    {
                        if (null != existingItem)
                        {
                            updatedAdverts.Add(existingItem);
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Description))
                {
                    guestAdded = true;
                    model.Id = Guid.NewGuid().ToString();
                    model.BusinessId = newBusiness.Id;
                    model.IsActive = true;
                    model.BusinessDate = newdate.Value;
                    model.BusinessEndDate = newEnddate.Value;
                    model.PicturePath = model.PicturePath;
                    updatedAdverts.AddRange(items);
                    updatedAdverts.Add(model);
                }
            }

            if (guestAdded)
            {
                newBusiness.Adverts = updatedAdverts.ToArray();
                await _dbProvider.ReplaceNewBusinessItem(newBusiness.BusinessName, newBusiness.Id, newBusiness);
            }


            return RedirectToAction("Ads", new { id = newBusiness.Id, eventName = newBusiness.AppBusinessName });
        }

        private string MakeCamelcase(string val)
        {
            if (string.IsNullOrEmpty(val))
            {
                return val;
            }

            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(val.ToLower());
        }

        [HttpPost]
        public async Task<IActionResult> EditSpecialDetail(DailySpecial model, string BusinessDate, string specialId, string[] allSelected, string id)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            DateTime? dt = null;
            DateTime? newdate = null;



            if (null != model)
            {
                var thisBusiness = await _dbProvider.GetFastBusinesss(id);
                //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
                newBusiness = thisBusiness;

                model.Name = MakeCamelcase(model.Name);
                model.Description = MakeCamelcase(model.Description);
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

            var items = new List<DailySpecial>();

            var updatedGuests = new List<DailySpecial>();

            DailySpecial newGuestCreation = null;

            if (newBusiness.DailySpecials != null)
            {
                items = newBusiness.DailySpecials.ToList();
                newGuestCreation = items.FirstOrDefault(x => x.Id == specialId);
            }

            var guestAdded = false;

            if (newGuestCreation != null)
            {
                guestAdded = true;

                foreach (var item in items)
                {
                    var existingItem = item;

                    if (existingItem.Id == specialId)
                    {

                        existingItem.Name = model.Name;
                        existingItem.Description = model.Description;
                        existingItem.BusinessDate = newdate.Value;

                        existingItem.IsActive = model.IsActive;

                        if (!string.IsNullOrEmpty(model.PicturePath))
                        {
                            existingItem.PicturePath = model.PicturePath;
                        }

                        if (newBusiness.Stocks != null && newBusiness.Stocks.Any())
                        {
                            if (allSelected != null && allSelected.Any())
                            {
                                existingItem.Stocks = newBusiness.Stocks.Where(x => allSelected.Contains(x.Id)).ToArray();

                                if (existingItem.Stocks.Any())
                                {
                                    existingItem.PicturePath = existingItem.Stocks.FirstOrDefault().PicturePath;
                                }
                            }
                        }

                        updatedGuests.Add(existingItem);
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
                if (!string.IsNullOrEmpty(model.Name) && !string.IsNullOrEmpty(model.Description))
                {
                    guestAdded = true;
                    model.Id = Guid.NewGuid().ToString();
                    model.BusinessId = newBusiness.Id;
                    model.IsActive = true;
                    model.BusinessDate = newdate.Value;

                    if (newBusiness.Stocks != null && newBusiness.Stocks.Any())
                    {
                        if (allSelected != null && allSelected.Any())
                        {
                            model.Stocks = newBusiness.Stocks.Where(x => allSelected.Contains(x.Id)).ToArray();
                        }
                    }

                    updatedGuests.AddRange(items);
                    updatedGuests.Add(model);
                }
            }

            if (guestAdded)
            {
                newBusiness.DailySpecials = updatedGuests.Where(x => x.Stocks.Any()).ToArray();
                await _dbProvider.ReplaceNewBusinessItem(newBusiness.BusinessName, newBusiness.Id, newBusiness);
            }


            return RedirectToAction("Specials", new { id = newBusiness.Id, eventName = newBusiness.AppBusinessName });
        }


   
        [HttpGet]
        [Route("EditPlace")]
        [Route("{id}/{adName}/{adId}")]
        public async Task<IActionResult> EditPlace(string id, string placeName, string placeId)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;

            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            PlaceOfInterest advert = null;

            if (newBusiness.PlacesOfInterest != null && newBusiness.PlacesOfInterest.Any() && !string.IsNullOrEmpty(placeId))
            {
                advert = newBusiness.PlacesOfInterest.FirstOrDefault(x => x.Id == placeId);
            }

            if (null == advert)
            {
                advert = new PlaceOfInterest { BusinessId = newBusiness.Id, BusinessDate = DateTime.Now };
            }

            return View(advert);
        }

        

        [HttpGet]
        [Route("EditSpeaker")]
        [Route("{id}/{speakerName}/{speakerId}")]
        public async Task<IActionResult> EditSpeaker(string id, string speakerName, string speakerId)
        {

            Business newBusiness = null;

            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            newBusiness = thisBusiness;

            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            Speaker speaker = null;

            if (newBusiness.Speakers != null && newBusiness.Speakers.Any() && !string.IsNullOrEmpty(speakerId))
            {
                speaker = newBusiness.Speakers.FirstOrDefault(x => x.Id == speakerId);
            }

            if (null == speaker)
            {
                speaker = new Speaker { BusinessId = newBusiness.Id, Order = 1 };
            }

            return View(speaker);
        }


        [HttpGet]
        [Route("EditAd")]
        [Route("{id}/{adName}/{adId}")]
        public async Task<IActionResult> EditAd(string id, string adName, string adId)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            newBusiness = thisBusiness;

            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            Advert advert = null;

            if (newBusiness.Adverts != null && newBusiness.Adverts.Any() && !string.IsNullOrEmpty(adId))
            {
                advert = newBusiness.Adverts.FirstOrDefault(x => x.Id == adId);
            }

            if (null == advert)
            {
                advert = new Advert { BusinessId = newBusiness.Id, BusinessDate = DateTime.Now, BusinessEndDate = DateTime.Now.AddDays(7) };
            }

            return View(advert);
        }


        [HttpGet]
        [Route("EditSpecial")]
        [Route("{id}/{specialName}/{specialId}")]
        public async Task<IActionResult> EditSpecial(string id, string specialName, string specialId)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            // if (!string.IsNullOrEmpty(id))
            // {
            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;
            //}


            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            DailySpecial room = null;

            if (newBusiness.DailySpecials != null && newBusiness.DailySpecials.Any() && !string.IsNullOrEmpty(specialId))
            {
                room = newBusiness.DailySpecials.FirstOrDefault(x => x.Id == specialId);
            }

            if (null == room)
            {
                room = new DailySpecial { BusinessId = newBusiness.Id, BusinessDate = DateTime.Now };
            }


            room.AllStocks = model.Stocks;


            return View(room);
        }

        [HttpGet]
        [Route("EditStock")]
        [Route("{id}/{stockName}/{stockId}")]
        public async Task<IActionResult> EditStock(string id, string stockName, string stockId)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            // if (!string.IsNullOrEmpty(id))
            // {
            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;
            //}


            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            Stock room = null;

            if (newBusiness.Stocks != null && newBusiness.Stocks.Any() && !string.IsNullOrEmpty(stockId))
            {
                room = newBusiness.Stocks.FirstOrDefault(x => x.Id == stockId);
            }

            if (null == room)
            {
                room = new Stock { BusinessId = newBusiness.Id };
            }

            room.Categories = newBusiness.Categories;

            if (room.Categories == null)
            {
                room.Categories = new List<Category>().ToArray();
            }

            return View(room);
        }

        [HttpGet]
        [Route("EditCategory")]
        [Route("{id}/{roomName}/{roomId}")]
        public async Task<IActionResult> EditCategory(string id, string catName, string catId)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;


            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;



            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            Category category = null;

            if (newBusiness.Categories != null && newBusiness.Categories.Any() && !string.IsNullOrEmpty(catId))
            {
                category = newBusiness.Categories.FirstOrDefault(x => x.Id == catId);
            }

            if (null == category)
            {
                category = new Category { BusinessId = newBusiness.Id };
            }

            return View(category);
        }

        [HttpGet]
        [Route("EditRoom")]
        [Route("{id}/{roomName}/{roomId}")]
        public async Task<IActionResult> EditRoom(string id, string roomName, string roomId)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            // if (!string.IsNullOrEmpty(id))
            // {
            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;
            //}


            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            Room room = null;

            if (newBusiness.Rooms != null && newBusiness.Rooms.Any() && !string.IsNullOrEmpty(roomId))
            {
                room = newBusiness.Rooms.FirstOrDefault(x => x.Id == roomId);
            }

            if (null == room)
            {
                room = new Room();
            }

            return View(room);
        }

        [HttpGet]
        [Route("Places")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Places(string id, string eventName)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            var thisBusiness = await _dbProvider.GetFastBusinesss(id);

            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);

            newBusiness = thisBusiness;

            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            return View(model);
        }

        [HttpGet]
        [Route("Speakers")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Speakers(string id, string eventName)
        {

            Business newBusiness = null;
            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            newBusiness = thisBusiness;

            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            return View(model);
        }


        [HttpGet]
        [Route("Ads")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Ads(string id, string eventName)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            //if (!string.IsNullOrEmpty(id))
            //{
            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;
            //}


            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            return View(model);
        }

        //Specials
        [HttpGet]
        [Route("Specials")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Specials(string id, string eventName)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            //if (!string.IsNullOrEmpty(id))
            //{
            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;
            //}


            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            return View(model);
        }



        [HttpGet]
        [Route("Categories")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Categories(string id, string eventName)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            //if (!string.IsNullOrEmpty(id))
            //{
            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;
            //}


            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            return View(model);
        }

        [HttpGet]
        [Route("Stocks")]
        [Route("{id}/{eventName})")]
        public async Task<IActionResult> Stocks(string id, string eventName)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            //if (!string.IsNullOrEmpty(id))
            //{
            var thisBusiness = await _dbProvider.GetFastBusinesss(id);
            //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
            newBusiness = thisBusiness;
            //}


            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            return View(model);
        }

        [HttpGet]
        [Route("Rooms")]
        [Route("{id}/{eventName}")]
        public async Task<IActionResult> Rooms(string id, string eventName)
        {
         //   var user = await _userManager.GetUserAsync(User);

            Business newBusiness = null;

            if (!string.IsNullOrEmpty(id))
            {
                var thisBusiness = await _dbProvider.GetFastBusinesss(id);
                //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);
                newBusiness = thisBusiness;
            }


            var model = new Business { BusinessDate = DateTime.Now, Id = id };

            if (newBusiness != null)
            {
                model = newBusiness;
            }

            return View(model);
        }


        private async Task<string> CreateLocationQRCode(Business newBusiness)
        {
            var returnPath = "";

            var fullUrl = $"https://cabbashutility.azurewebsites.net/api/Location?id={newBusiness.Id}&eventName={newBusiness.AppBusinessName}&latitude={newBusiness.Latitude.Value}&longitude={newBusiness.Longitude.Value}&businessDate={newBusiness.BusinessDate}";

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

            var address = "Not Specified";

            if (!string.IsNullOrEmpty(newBusiness.Street))
            {
                address = newBusiness.Street;
            }

            var startDate = newBusiness.BusinessDate.Value;
            var endDate = startDate.AddHours(10);


            // public HttpResponseMessage Get(string id, string eventName, decimal? latitude, decimal? longitude, DateTime? businessStartDate, DateTime? businessEndDate, string address)


            var fullUrl = $"https://cabbashutility.azurewebsites.net/api/diary?id={newBusiness.Id}&eventName={newBusiness.AppBusinessName}&latitude={newBusiness.Latitude.Value}&longitude={newBusiness.Longitude.Value}&businessStartDate={startDate}&businessEndDate={endDate}&address={address}";


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

        private async Task<string> SaveFiles(IFormFile formFile, string businessId, bool isImage = true, bool useThumbnail = false, bool iscategoryImage = false)
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

        public static string sendUkMMS(string tel, string msg, string from)
        {
            tel = "44" + tel.Remove(0, 1);
            /* {"apikey" , "eRR40/21WGI-KSi4ybccDxYSYmMsk3Ddi3iMh7MPQt"},
                {"numbers" , tel},
                {"message" , msg},*/
            String message = HttpUtility.UrlEncode(msg);
            using (var wb = new WebClient())
            {
                byte[] response = wb.UploadValues("https://api.txtlocal.com/send/", new NameValueCollection()
                {
                {"apikey" , "eRR40/21WGI-KSi4ybccDxYSYmMsk3Ddi3iMh7MPQt"},
                {"numbers" , tel},
                {"message" , message},
                {"sender" , from}
                });
                string result = System.Text.Encoding.UTF8.GetString(response);
                return "100";
            }
        }

        public static string sendCabbashSMS(string msg, string telephoneNumber, string nameOfPlace, string placeId)
        {
            string message = msg;
            string sender = "CABBASH";// + nameOfPlace;

            if (!string.IsNullOrEmpty(nameOfPlace))
            {
                sender = nameOfPlace;
            }

            if (!string.IsNullOrEmpty(telephoneNumber))
            {
                telephoneNumber = telephoneNumber.Trim();
            }

            string recipient = telephoneNumber;
            //string url = "https://www.MultiTexter.com/tools/geturl/Sms.php?username=academyvistang@gmail.com&password=Lauren280701&sender=" + sender + "&message=" + message + "&flash=0&sendtime=2009-10- 18%2006:30&listname=friends&recipients=" + recipient;
            string url = "https://www.MultiTexter.com/tools/geturl/Sms.php?username=leboston@yahoo.com&password=Lauren280701&sender=" + sender + "&message=" + message + "&flash=0&sendtime=2009-10- 18%2006:30&listname=friends&recipients=" + recipient;
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(string.Format(url));
            webReq.Method = "GET";

            try
            {
                var n = "";
                //071 | 073 | 074 | 075 | 076 | 077 | 078 | 079
                if (telephoneNumber.StartsWith("071") || telephoneNumber.StartsWith("073") || telephoneNumber.StartsWith("074")
                    || telephoneNumber.StartsWith("075") || telephoneNumber.StartsWith("076") || telephoneNumber.StartsWith("077")
                    || telephoneNumber.StartsWith("078") || telephoneNumber.StartsWith("079"))
                {
                    n = sendUkMMS(telephoneNumber, message, sender);
                }
                else
                {
                    HttpWebResponse webResponse = (HttpWebResponse)webReq.GetResponse();
                    Stream answer = webResponse.GetResponseStream();
                    StreamReader _recivedAnswer = new StreamReader(answer);
                    n = _recivedAnswer.ReadToEnd().ToString();
                }


                if (n == "100")
                {
                    //InsertTextBilling(msg, telephoneNumber, placeId);
                }

                return n;
            }
            catch (Exception ex)
            {
                return "";
            }
        }
        private string SendTextToFriend(string name, string friendsName, string friendsTelephone, CosmosDBEntities.Models.Business business, string selfiePath)
        {
            try
            {
                business.BusinessMC = name;//host name
                business.BusinessMCPic = friendsName;//friends name
                business.BusinessPlaner = friendsTelephone;//friends email
                business.SelfiePath = selfiePath;

                var url = $"https://www.cabbash.com/locate/?id={business.Id}&promo=1";

                var invite = "Hello " + friendsName + ", " + name + " has sent your an interesting link, Click here " + url + " to view.";

                return sendCabbashSMS(invite, friendsTelephone, "CABBASH", business.Id);
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private async Task<string> SendEmailToFriend(string name, string friendsName, string friendsEmail, CosmosDBEntities.Models.Business business, string selfiePath)
        {
            try
            {
                business.BusinessMC = name;//host name
                business.BusinessMCPic = friendsName;//friends name
                business.BusinessPlaner = friendsEmail;//friends email
                business.SelfiePath = selfiePath;

                var strBus = JsonConvert.SerializeObject(business);

                var request = new HttpRequestMessage(HttpMethod.Get, "https://cabbashutility.azurewebsites.net/api/email/");

                request.Content = new StringContent(strBus);
                using (var client = new HttpClient())
                using (HttpResponseMessage response = await client.SendAsync(request))
                {
                    var b = response.StatusCode;
                    var responsestr = await response.Content.ReadAsStringAsync();
                    return responsestr;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }


        [HttpPost]

        public async Task<IActionResult> CreateNewPromo(Business business, List<IFormFile> selfiefiles, string id)
        {
         //   var user = await _userManager.GetUserAsync(User);

            var thisBusiness = await _dbProvider.GetFastBusinesss(id);

            //var thisBusiness = businessThis.FirstOrDefault(x => x.Id == user.BusinessIdentifier);

            var selfiePath = string.Empty;

            var emailSent = false;

            if (selfiefiles.Count > 0)
            {
                selfiePath = await SaveFiles(selfiefiles[0], business.Id);
            }

            if (!string.IsNullOrEmpty(business.PromoEmails) && !string.IsNullOrEmpty(selfiePath) && !string.IsNullOrEmpty(business.PromoSender))
            {
                var emails = business.PromoEmails.Split(",");

                foreach (var emailer in emails)
                {
                    await SendEmailToFriend(business.PromoSender, "Customer", emailer, thisBusiness, selfiePath);
                    emailSent = true;
                }
            }


            if (!string.IsNullOrEmpty(business.PromoTelephones) && !string.IsNullOrEmpty(business.PromoSender))
            {
                var telephones = business.PromoTelephones.Split(",");

                foreach (var tel in telephones)
                {
                    SendTextToFriend(business.PromoSender, "Customer", tel, thisBusiness, selfiePath);
                    emailSent = true;
                }
            }



            var types = new List<BussType>();

            types.Add(new BussType { Id = 1, Name = "Hotel" });
            types.Add(new BussType { Id = 2, Name = "Bar && Restaurant" });
            types.Add(new BussType { Id = 3, Name = "Delivery Only" });


            try
            {


                if (null == thisBusiness)
                    thisBusiness = new Business { BusinessTypes = types.ToArray(), BusinessDate = DateTime.Now };
                else
                {
                    thisBusiness.BusinessTypes = types.ToArray();
                }

                thisBusiness.PromoSent = emailSent;

                return View("Promos", thisBusiness);
            }
            catch (Exception)
            {
                var myBusiness = new Business { BusinessDate = DateTime.Now, };
                return View("Promos", myBusiness);
            }
        }
        [HttpGet]
        public async Task<IActionResult> Promos(string id)
        {
      
            var types = new List<BussType>();

            types.Add(new BussType { Id = 1, Name = "Hotel" });
            types.Add(new BussType { Id = 2, Name = "Bar && Restaurant" });
            types.Add(new BussType { Id = 3, Name = "Delivery Only" });

            try
            {
                var thisBusiness = await _dbProvider.GetFastBusinesss(id);

                //var thisBusiness = business.FirstOrDefault(x => x.Id == user.BusinessIdentifier);

                if (null == thisBusiness)
                    thisBusiness = new Business { BusinessTypes = types.ToArray(), BusinessDate = DateTime.Now };
                else
                {
                    thisBusiness.BusinessTypes = types.ToArray();
                }

                return View(thisBusiness);
            }
            catch (Exception)
            {
                var thisBusiness = new Business { BusinessDate = DateTime.Now, };
                return View(thisBusiness);
            }
        }


        [HttpGet]
        public async Task<IActionResult> OrdersById(string businessId)
        {
            var thisBusiness = await _dbProvider.GetFastBusinesss(businessId);


            var orders = await _dbProvider.GetOrderItemsByBusinessId(businessId);

            var stocks = thisBusiness.Stocks.ToList();

            foreach (var o in orders.Where(x => x.PaidInFull))
            {
                foreach (var od in o.OrderDetails)
                {
                    od.Stock = stocks.FirstOrDefault(x => x.Id == od.StockId);
                }
            }

            return PartialView("_OrderListPremium", orders);

        }


        [HttpGet]
        public IActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}