using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CosmosDBEntities.Models;
using CosmosDBService;
using IdentitySample.Models;
using IencircleAdmin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;

namespace IencircleAdmin.Controllers
{
    [Authorize()]

    public class GuestController : Controller
    {
        private UserManager<ApplicationUser> _userManager;
        protected ICompositeViewEngine viewEngine;
        protected IDbProvider _cosmosDbProvider;
        public GuestController(ICompositeViewEngine viewEngine, UserManager<ApplicationUser> userManager, IMemoryCache cache, IDbProvider dbProvider)
        {

            try
            {
                this.viewEngine = viewEngine;
                _userManager = userManager;
                _cosmosDbProvider = dbProvider;
                _cosmosDbProvider.SetCache(cache);
            }
            catch (Exception ex)
            {
                string pp = ex.Message;
            }
        }

        public IActionResult Index()
        {
            return View();
        }


        public async Task<IActionResult> Download(string id, string guestId, bool? inviteAcceped)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (!string.IsNullOrEmpty(guestId))
                {

                    var events = await _cosmosDbProvider.GetFastBusinesssAllEvents();

                    var newEvent = events.FirstOrDefault(x => x.Id == id);

                    Guest item = newEvent != null && newEvent.Guests != null ? newEvent.Guests.FirstOrDefault(x => x.Id == guestId) : null;

                    if (null != item)
                    {
                        var telephone = item.Telephone;
                        var fullname = item.Telephone;
                        var email = item.Telephone;
                        var companyName = item.Telephone;
                        //var telephone = item.Telephone;

                        var onlyQrCode = true;

                        var fullUrl = $"https://businesscardgeneratorlive.azurewebsites.net/api/Values?guestId={guestId}&telephone={telephone}&fullname={fullname}&email={email}&id={newEvent.Id}&companyName={companyName}&eventName={newEvent.AppBusinessName}&onlyQrCode={onlyQrCode}";

                        var invitePath64 = "";

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
                                            invitePath64 = JsonConvert.DeserializeObject<string>(jsoninvitePath);
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
                    }
                    else
                    {
                        EventViewModel model = new EventViewModel { Fullname = "Not Known", ColourCodeDescription = "UNKNOWN", Telephone = "00000000000", ColourCode = "Red", NoOfInvitess = "1" };
                        return View(model);
                    }
                }
            }

            EventViewModel model1 = new EventViewModel { Fullname = "Not Known", ColourCodeDescription = "UNKNOWN", Telephone = "00000000000", ColourCode = "Blue", NoOfInvitess = "1" };

            return View(model1);

        }

        public async Task<IActionResult> Invite(string id, string guestId, bool? inviteAcceped)
        {
            if (!string.IsNullOrEmpty(id))
            {
                if (!string.IsNullOrEmpty(guestId))
                {
                    this._cosmosDbProvider = new DbProvider();

                     var events = await _cosmosDbProvider.GetFastBusinesssAllEvents();

                    var newEvent = events.FirstOrDefault(x => x.Id == id);

                    Guest item = newEvent != null && newEvent.Guests != null ? newEvent.Guests.FirstOrDefault(x => x.Id == guestId) : null;

                    if (null != item)
                    {
                        var cc = "Red";

                        if (item.Status.ToUpper().StartsWith("V"))
                        {
                            cc = "Green";
                        }

                        EventViewModel model = new EventViewModel { AppBusinessName = newEvent.AppBusinessName, InviteAcceped = inviteAcceped, BusinessLogo = newEvent.BusinessLogoPath, Fullname = item.Fullname, ColourCodeDescription = item.Status, CompanyName = item.CompanyName, Telephone = item.Telephone, ColourCode = cc, NoOfInvitess = item.NoOfInvites.ToString() };

                        return View(model);
                    }

                    EventViewModel model2 = new EventViewModel { Fullname = "Not Known", ColourCodeDescription = "UNKNOWN", Telephone = "00000000000", ColourCode = "Blue", NoOfInvitess = "1" };
                    return View(model2);
                }
            }
            else
            {
                EventViewModel model = new EventViewModel { Fullname = "Not Known", ColourCodeDescription = "UNKNOWN", Telephone = "00000000000", ColourCode = "Red", NoOfInvitess = "1" };
                return View(model);
            }

            EventViewModel model1 = new EventViewModel { Fullname = "Not Known", ColourCodeDescription = "UNKNOWN", Telephone = "00000000000", ColourCode = "Blue", NoOfInvitess = "1" };
            return View(model1);

        }
    }
}