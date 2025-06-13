namespace Incoding.MvcContrib
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    #region << Using >>

    using System.Web.Mvc;
    using System.Web.Mvc.Html;
    using Incoding.CQRS;

    #endregion

    public class RenderViewQuery : QueryBase<MvcHtmlString>
    {
        public HtmlHelper HtmlHelper { get; set; }

        public string PathToView { get; set; }

        public object Model { get; set; }

        /// <inheritdoc/>
        protected override MvcHtmlString ExecuteResult()
        {
            return this.HtmlHelper.Partial(PathToView, Model);
        }

        /// <inheritdoc/>
        protected override Task<MvcHtmlString> ExecuteResultAsync(CancellationToken ct = default)
        {
            throw new NotSupportedException("This query does not support async execution.");
        }
    }
}