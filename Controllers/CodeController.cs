//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using IencircleAdmin.DocumentDB;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;

//namespace IencircleAdmin.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class CodeController : ControllerBase
//    {

//        private readonly IDocumentDbService _dbService;
//        public CodeController(DocumentDbService dbService)
//        {
//            _dbService = dbService;
//        }

//        public async Task<ContentResult> Get(string id)
//        {
//            var returnValue = string.Empty;

//            try
//            {

//                var existingItems = _dbService.GetItem("","");

//                var existingItem = existingItems.FirstOrDefault(x => x.Id == id);

//                var result = JsonConvert.SerializeObject(existingItem);

//                if (!string.IsNullOrEmpty(result))
//                {
//                    return Content(result, "text/json");
//                }
//                else
//                {
//                    return Content("Cached content not found", "text/json");
//                }
//            }
//            catch (Exception e)
//            {
               
//                return Content("", "text/json");
//            }
//            finally
//            {

//            }
//        }
//    }
//}