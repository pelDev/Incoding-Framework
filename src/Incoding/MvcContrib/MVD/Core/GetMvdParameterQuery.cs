namespace Incoding.MvcContrib.MVD
{
    using System;
    #region << Using >>

    using System.Collections.Specialized;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Incoding.CQRS;

    #endregion

    public sealed class GetMvdParameterQuery : QueryBase<GetMvdParameterQuery.Response>
    {
        public NameValueCollection Params { get; set; }

        protected override Response ExecuteResult()
        {
            bool incIsModel;
            bool.TryParse(Params["incIsModel"], out incIsModel);

            bool isValidate;
            bool.TryParse(Params["incValidate"], out isValidate);

            bool isCompositeArray;
            bool.TryParse(Params["incIsCompositeAsArray"], out isCompositeArray);

            var contentType = Params["incContentType"];
            return new Response()
                   {
                           Type = Params["incType"] ?? Params["incTypes"],
                           IsModel = incIsModel,
                           View = HttpUtility.UrlDecode(Params["incView"]),
                           IsValidate = isValidate,
                           IsCompositeArray = isCompositeArray,
                           ContentType = string.IsNullOrWhiteSpace(contentType) ? "img" : contentType,
                           FileDownloadName = Params["incFileDownloadName"] ?? string.Empty,
                   };
        }

        protected override Task<Response> ExecuteResultAsync(CancellationToken ct = default)
        {
            return Task.FromResult(ExecuteResult());
        }

        public class Response
        {
            public string Type { get; set; }

            public bool IsModel { get; set; }

            public string View { get; set; }

            public bool IsValidate { get; set; }

            public string ContentType { get; set; }

            public string FileDownloadName { get; set; }

            public bool IsCompositeArray { get; set; }
        }
    }
}