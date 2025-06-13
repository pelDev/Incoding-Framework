namespace Incoding.Data
{
    #region << Using >>

    using NHibernate.Engine;
    using NHibernate.Id;
    using System.Threading;
    using System.Threading.Tasks;

    #endregion

    ////ncrunch: no coverage start
    public class GuidStringGenerator : IIdentifierGenerator
    {
        #region IIdentifierGenerator Members

        public object Generate(ISessionImplementor session, object obj)
        {
            return new GuidCombGenerator().Generate(session, obj).ToString();
        }

        Task<object> IIdentifierGenerator.GenerateAsync(ISessionImplementor session, object obj, CancellationToken cancellationToken)
        {
            return new GuidCombGenerator().GenerateAsync(session, obj, cancellationToken);
        }

        #endregion
    }

    ////ncrunch: no coverage end
}