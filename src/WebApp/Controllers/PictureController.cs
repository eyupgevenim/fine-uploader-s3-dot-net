using System;
using Microsoft.AspNetCore.Mvc;
using WebApp.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using WebApp.Models;
using System.Diagnostics;

namespace WebApp.Controllers
{
    public class PictureController : Controller
    {
        private readonly ILogger<PictureController> logger;
        private readonly IAwsS3Service awsService;
        private readonly IPictureService pictureService;

        public PictureController(ILogger<PictureController> logger, 
            IAwsS3Service awsService, 
            IPictureService pictureService)
        {
            this.logger = logger;
            this.awsService = awsService;
            this.pictureService = pictureService;
        }

        public virtual ActionResult Index() => View();

        /// <summary>
        /// create signature for aws s3 file
        /// </summary>
        /// <returns>policy and signature as json</returns>
        [HttpPost]
        public virtual async Task<ActionResult> Signature()
        {
            ///if (!User.Identity.IsAuthenticated) throw new UnauthorizedAccessException();

            if (Request.QueryString.ToString() != "?v4=true")
                return Json(new { invalid = true });

            try
            {
                //if you read streamReader.ReadToEnd for HttpContext.Request.Body
                ////using Microsoft.AspNetCore.Http.Features;
                //var syncIOFeature = HttpContext.Features.Get<IHttpBodyControlFeature>();
                //if (syncIOFeature != null)
                //    syncIOFeature.AllowSynchronousIO = true;

                var policy_document =  await awsService.GetPolicyDocumentAsync(HttpContext.Request.Body);
                var policy = awsService.GetBase64Policy(policy_document);
                var signature = awsService.GetSignature(policy_document);

                return Json(new { policy = policy, signature = signature });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"PictureController.Signature|{ex.Message}");
            }

            return Json(new { invalid = true });
        }

        /// <summary>
        /// Uloaded aws s3 file when success
        /// </summary>
        /// <returns>return your coustom paramer as json</returns>
        [HttpPost]
        public virtual ActionResult UploadSuccess()
        {
            ///if (!User.Identity.IsAuthenticated) throw new UnauthorizedAccessException();

            var success = true;
            string errorMessage = null;
            var key = Request.Form["key"].ToString();
            //var myParam1 = Request.Form["myParam1"].ToString();
            //var myParam2 = Request.Form["myParam2"].ToString();
            //...

            try
            {
                pictureService.InsertPicture(key);
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                success = false;
                logger.LogError(ex, $"PictureController.UploadSuccess|{errorMessage}");
            }

            return Json(new { success, key, errorMessage });
        }

        /// <summary>
        /// delete from aws s3 file by bucket key
        /// </summary>
        /// <returns>result as boolean</returns>
        [HttpPost]
        public virtual ActionResult Delete()
        {
            ///if (!User.Identity.IsAuthenticated) throw new UnauthorizedAccessException();

            var success = false;
            var key = Request.Form["key"].ToString();
            //var myParam1 = Request.Form["myParam1"].ToString();
            //var myParam2 = Request.Form["myParam2"].ToString();
            //...

            try
            {
                pictureService.DeletePicture(key);
                success = true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"PictureController.Delete|{ex.Message}");
            }

            return Json(new { success });
        }

        /// <summary>
        /// get all pictures
        /// </summary>
        /// <returns>pictures key</returns>
        public virtual ActionResult GetPictures()
        {
            ///if (!User.Identity.IsAuthenticated) throw new UnauthorizedAccessException();

            try
            {
                return Json(new { pictures = pictureService.GetAllPicture() });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"PictureController.GetPictures|{ex.Message}");
            }

            return Json(new { pictures = new List<string>() });
        }

       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}