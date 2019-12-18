using IronOcr;
using RRS_API.Models;
using RRS_API.Models.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using AForge.Imaging.Filters;


namespace RRS_API.Controllers
{
    [RoutePrefix("api/Settings")]
    public class SettingsController : ApiController
    {
        private AdvancedOcr ocr; //ocr object
        private SettingsMngr SettingsMngr = new SettingsMngr();
        private TokenMngr TokenMngr = new TokenMngr();

        #region GET Requests

        public HttpResponseMessage GetException()
        {
            try
            {
                IronOcr.License.LicenseKey = "IRONOCR-331669D230-119164-85AA56-6D1E880DDB-7AC6351-UEx46DE8D12FEFC7D8-BENGURIONUNIVERSITY.IRO190324.4912.54129.PRO.1DEV.1YR.SUPPORTED.UNTIL.24.MAR.2020";
                IronOcrInstallation.InstallationPath = @"C:\Users\Maor\Desktop\semeter f\Client-Server-17-08\20.09\RRS-SERVER\publish\Test";
                this.ocr = new AdvancedOcr()
                {
                    Language = IronOcr.Languages.Hebrew.OcrLanguagePack,
                    ColorSpace = AdvancedOcr.OcrColorSpace.GrayScale,
                    EnhanceResolution = true,
                    EnhanceContrast = true,
                    CleanBackgroundNoise = true,
                    ColorDepth = 15,
                    RotateAndStraighten = true,
                    DetectWhiteTextOnDarkBackgrounds = true,
                    ReadBarCodes = false,
                    Strategy = AdvancedOcr.OcrStrategy.Advanced,
                    InputImageType = AdvancedOcr.InputTypes.Snippet
                };
                string path = System.Web.HttpContext.Current.Server.MapPath("~/Images/receipt201909141301444.jpg");
                Image newImage = Image.FromFile(path);
                OcrResult ocrResults = ocr.Read(convertToGrayscaleV1(newImage));
                return Request.CreateResponse(HttpStatusCode.OK, ocrResults);

            }
            catch (Exception e)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, e.ToString());
            }
        }

        private Bitmap convertToGrayscaleV1(Image image)
        {

            /*
            int[,] kernel = {
            { -2, -1,  0 },
            { -1,  1,  1 },
            {  0,  1,  2 } };
            // create filter
            Convolution Convolution = new Convolution(kernel);
            // apply the filter
            Convolution.ApplyInPlace(a);
            */
            Bitmap a = new Bitmap(image);
            var bmp8bpp = Grayscale.CommonAlgorithms.BT709.Apply(a);
            OtsuThreshold OtsuThreshold = new OtsuThreshold();
            OtsuThreshold.ApplyInPlace(bmp8bpp);
            return bmp8bpp;
        }

        [Route("GetFamilies")]
        [HttpPost]
        public HttpResponseMessage GetFamilies()
        {
            try
            {
                List<string> Families = new List<string>();
                var httpRequest = HttpContext.Current.Request;
                string token = httpRequest.Headers["Authorization"];
                //if token valid
                if (TokenMngr.isTokenValid(token))
                {
                    //if admin
                    if (TokenMngr.isAdmin(token))
                    {
                        Families = SettingsMngr.getFamilies();
                    }
                    //user
                    else
                    {
                        var jwtToken = new JwtSecurityToken(token);
                        object username = "";
                        jwtToken.Payload.TryGetValue("unique_name", out username);
                        Families.Add(username.ToString());
                    }
                    return Request.CreateResponse(HttpStatusCode.OK, Families);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden);
                }
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.Forbidden);
            }
        }

        [Route("GetMarkets")]
        [HttpGet]
        public HttpResponseMessage GetMarkets()
        {
            try
            {
                List<String> Markets = SettingsMngr.getMarkets();
                return Request.CreateResponse(HttpStatusCode.OK, Markets);
            }
            catch (Exception)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError);
            }
        }

        #endregion

        #region POST Requests
        #endregion
    }
}