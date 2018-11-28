using System;
using System.Management.Automation;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Briver.Framework;
using Briver.Http;

namespace Briver.Commands
{
    [Cmdlet(VerbsLifecycle.Invoke, "Api")]
    public class InvokeApi : Cmdlet, ICommand
    {
        [Parameter(Mandatory = true, Position = 1)]
        public string Uri { get; set; }

        [Parameter(Mandatory = false)]
        public ApiMethod Method { get; set; } = ApiMethod.Get;

        [Parameter(Mandatory = false)]
        public object Content { get; set; }

        private HttpMethod GetHttpMethod()
        {
            switch (this.Method)
            {
                case ApiMethod.Get:
                    return HttpMethod.Get;
                case ApiMethod.Post:
                    return HttpMethod.Post;
                case ApiMethod.Put:
                    return HttpMethod.Put;
                case ApiMethod.Delete:
                    return HttpMethod.Delete;
                default:
                    return HttpMethod.Get;
            }
        }

        protected override void ProcessRecord()
        {
            var request = new HttpRequestMessage
            {
                Method = GetHttpMethod(),
                RequestUri = new Uri(Uri, UriKind.Absolute),
            };
            if (Content != null)
            {
                request.Content = new FormContent(Content);
            }
            var result = Task.Run(async () =>
            {
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = OnValidateServerCertificate,
                };
                var client = new HttpClient(handler);
                var response = await client.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;

            }).GetAwaiter().GetResult();

            WriteObject(result);
        }

        private bool OnValidateServerCertificate(HttpRequestMessage message, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //if (errors != SslPolicyErrors.None)
            //{
            //    Console.WriteLine(errors);
            //}
            return true;
        }
    }

    public enum ApiMethod
    {
        Get,
        Post,
        Put,
        Delete
    }
}
