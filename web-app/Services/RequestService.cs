using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Web.Http;
using web_app.Requests;
using web_server.Models.DBModels;
using web_server.Services.Interfaces;

namespace web_app.Services
{
    public class RequestService : IRequestService
    {
        IJsonService _jsonService;
        public RequestService(IJsonService jsonService)
        {
            _jsonService = jsonService;
        }
        public ResponseModel SendGet(CustomRequestGet request)
        {
            return SendGet(request, null);
        }
        public IActionResult JsonToResult(string json)
        {
            HttpResponseMessage msg = new HttpResponseMessage();
            if (json.Contains("недостаточно прав"))
            {
                msg = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "У Вас недостаточно прав для просмотра данной страницы" };
                throw new HttpResponseException(msg);
            }
            else if (json.Contains("401 (Unauthorized)"))
            {
                msg = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "401" };
                throw new HttpResponseException(msg);
            }


            return new OkObjectResult(json);
        }

        public ResponseModel SendGet(CustomRequestGet request, HttpContext context)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    if (context != null)
                    {
                        if (context.Request.Cookies.ContainsKey(".AspNetCore.Application.Id"))
                        {
                            client.DefaultRequestHeaders.Add(".AspNetCore.Application.Id", context.Request.Cookies[".AspNetCore.Application.Id"]);
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + context.Request.Cookies[".AspNetCore.Application.Id"]);
                        }

                        if (context.Request.Headers.ContainsKey(".AspNetCore.Application.Id"))
                        {
                            client.DefaultRequestHeaders.Add(".AspNetCore.Application.Id", context.Request.Headers[".AspNetCore.Application.Id"][0]);
                            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + context.Request.Headers[".AspNetCore.Application.Id"][0]);
                        }
                    }

                    string url = "";
                    if (request.Args == null)
                    {
                        url = Program.web_server_ip + "/" + request.Address;
                    }
                    else
                    {
                        url = Program.web_server_ip + "/" + request.Address + $"?args={request.Args}";
                    }
                    var responseString = client.GetStringAsync(url).Result;
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(responseString);
                }

            }
            catch (Exception ex)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(_jsonService.PrepareErrorJson(ex.Message));
            }

        }

        public ResponseModel SendPost(CustomRequestPost req) => SendPost(req, null);

        public ResponseModel SendPost(CustomRequestPost req, HttpContext context)
        {
            try
            {
                string url = "";

                url = Program.web_server_ip + "/" + req.Address;

                var request = WebRequest.Create(url);
                request.Method = "POST";

                string json = "";
                byte[] byteArray;
                if(req.Tutor != null)
                {
                    json = JsonSerializer.Serialize(req.Tutor);
                    byteArray = Encoding.UTF8.GetBytes(json);
                }
                else
                {
                    json = JsonSerializer.Serialize(req.User);
                    byteArray = Encoding.UTF8.GetBytes(json);

                }
              
                request.ContentType = "application/x-www-form-urlencoded";


                request.Credentials = CredentialCache.DefaultNetworkCredentials;
                if (context != null)
                {
                    if (context.Request.Cookies.ContainsKey(".AspNetCore.Application.Id"))
                    {
                        request.Headers.Add(".AspNetCore.Application.Id", context.Request.Cookies[".AspNetCore.Application.Id"]);
                        request.Headers.Add("Authorization", "Bearer " + context.Request.Cookies[".AspNetCore.Application.Id"]);

                    }
                }

                if (req.User != null)
                {
                    using var reqStream = request.GetRequestStream();
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }

                if (req.Tutor != null)
                {
                    using var reqStream = request.GetRequestStream();
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }
                if (req.Args != null)
                {

                    byteArray = Encoding.UTF8.GetBytes(req.Args);
                    request.ContentLength = byteArray.Length;


                    using var reqStream = request.GetRequestStream();
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }
                if (req.TransferData != null)
                {
                    var type = req.TransferData.GetType();
                    if (type.Name == "Registration")
                    {
                        var model = (Registration)req.TransferData;
                        json = Newtonsoft.Json.JsonConvert.SerializeObject(model);
                    }
                    else
                    {
                        json = Newtonsoft.Json.JsonConvert.SerializeObject(req.TransferData);

                    }

                    byteArray = Encoding.UTF8.GetBytes(json);
                    request.ContentLength = byteArray.Length;


                    using var reqStream = request.GetRequestStream();
                    reqStream.Write(byteArray, 0, byteArray.Length);
                }


                using var response = request.GetResponse();

                using var respStream = response.GetResponseStream();

                using var reader = new StreamReader(respStream);
                string data = reader.ReadToEnd();

                return Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(data);
            }
            catch (Exception ex)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(_jsonService.PrepareErrorJson(ex.Message));
            }

        }
    }
}
