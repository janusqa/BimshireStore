
using System.Text;
using System.Text.Json;
using BimshireStore.Services.OrderAPI.Utility;
using BimshireStore.Services.OrderAPI.Models.Dto;
using BimshireStore.Services.OrderAPI.Services.IService;
using Microsoft.AspNetCore.Components.Forms;


namespace BimshireStore.Services.OrderAPI.Services
{
    public class HttpRequestMessageBuilder : IHttpRequestMessageBuilder
    {
        public HttpRequestMessage Build(ApiRequest apiRequest)
        {
            HttpRequestMessage message = new HttpRequestMessage();

            var contentType = apiRequest.ContentType switch
            {
                SD.ContentType.MultiPartFormData => "*/*",
                _ => "application/json"
            };
            message.Headers.Add("Accept", contentType);

            message.Method = apiRequest.ApiMethod switch
            {
                SD.ApiMethod.POST => HttpMethod.Post,
                SD.ApiMethod.PUT => HttpMethod.Put,
                SD.ApiMethod.DELETE => HttpMethod.Delete,
                _ => HttpMethod.Get,
            };

            message.RequestUri = new Uri(apiRequest.Url);

            if (apiRequest.Data is not null)
            {
                if (apiRequest.ContentType == SD.ContentType.MultiPartFormData)
                {
                    // form multi-part
                    var content = new MultipartFormDataContent();
                    foreach (var item in apiRequest.Data.GetType().GetProperties())
                    {
                        var value = item.GetValue(apiRequest.Data);
                        if (value is IBrowserFile)
                        {
                            var file = (IBrowserFile)value;
                            if (file is not null)
                            {
                                content.Add(new StreamContent(file.OpenReadStream()), item.Name, file.Name);
                            }
                        }
                        else
                        {
                            content.Add(new StringContent(value?.ToString() ?? ""), item.Name);
                        }
                    }
                    message.Content = content;
                }
                else
                {
                    // json 
                    message.Content = new StringContent(
                        JsonSerializer.Serialize(apiRequest.Data),
                        Encoding.UTF8,
                        "application/json"
                    );
                }
            }

            return message;
        }
    }
}