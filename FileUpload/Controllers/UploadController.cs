using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace FileUpload.Controllers
{
    [Authorize]
    [RoutePrefix("api/upload")]
    public class UploadController : ApiController
    {
        protected const string DefaultUploadPath = "~/Content/Uploads";

        // POST api/upload
        [HttpPost]
        [Route("")]
        public Task<HttpResponseMessage> Upload()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartFormDataStreamProvider(HttpContext.Current.Server.MapPath(DefaultUploadPath));

            return Request.Content.ReadAsMultipartAsync(provider).ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }

                foreach (var file in provider.FileData)
                {
                    var details = new
                    {
                        LocalName = file.LocalFileName,
                        FileName = file.Headers.ContentDisposition.FileName,
                        Size = file.Headers.ContentDisposition.Size,
                        Type = file.Headers.ContentType
                    };

                    // TODO: Any logging or whatever ???
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            });
        }

        // POST api/upload/tomemory
        [HttpPost]
        [Route("tomemory")]
        public Task<HttpResponseMessage> UploadToMemory()
        {
            if (!Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new CustomMultipartFormDataStreamProvider();

            return Request.Content.ReadAsMultipartAsync(provider).ContinueWith(t =>
            {
                if (t.IsFaulted || t.IsCanceled)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, t.Exception);
                }

                foreach (var file in provider.CustomFileData)
                {
                    var details = new
                    {
                        LocalName = file.LocalFileName,
                        FileName = file.Headers.ContentDisposition.FileName,
                        Size = file.Headers.ContentDisposition.Size,
                        Type = file.Headers.ContentType,

                        Stream = file.MemoryStream
                    };

                    using (var reader = new StreamReader(details.Stream))
                    {
                        var content = reader.ReadToEnd();

                        // Use Content?
                        // Dispose of stream?
                    }

                    // TODO: Any logging or whatever ???
                }

                return Request.CreateResponse(HttpStatusCode.OK);
            });
        }

        public class CustomMultipartFileData : MultipartFileData
        {
            public MemoryStream MemoryStream { get; set; }

            public CustomMultipartFileData(HttpContentHeaders headers, string name)
                : base(headers, name)
            { }
        }

        public class CustomMultipartFormDataStreamProvider : MultipartFormDataStreamProvider
        {
            public List<CustomMultipartFileData> CustomFileData { get; set; }

            public bool InMemory { get; set; }

            public CustomMultipartFormDataStreamProvider(bool inMemory = true)
                : base("~/")
            {
                InMemory = inMemory;

                CustomFileData = new List<CustomMultipartFileData>();
            }

            public CustomMultipartFormDataStreamProvider(string path, bool inMemory = false)
                : base(path)
            {
                InMemory = inMemory;

                CustomFileData = new List<CustomMultipartFileData>();
            }

            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                if (!InMemory || headers.ContentDisposition == null || string.IsNullOrWhiteSpace(headers.ContentDisposition.FileName))
                    return base.GetStream(parent, headers);

                var data = new CustomMultipartFileData(headers, GetLocalFileName(headers))
                {
                    MemoryStream = new MemoryStream()
                };

                FileData.Add(data);
                CustomFileData.Add(data);

                return data.MemoryStream;
            }

            public override string GetLocalFileName(HttpContentHeaders headers)
            {
                var name = headers.ContentDisposition.FileName;

                if (string.IsNullOrWhiteSpace(name))
                    return base.GetLocalFileName(headers);

                // Trim quotations and ensure we just get the name of the file (exclude the path)
                return new FileInfo(name.Replace(@"""", string.Empty)).Name;
            }
        }
    }
}
