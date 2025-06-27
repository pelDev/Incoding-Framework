namespace Incoding.MvcContrib.MVD
{
    #region << Using >>

    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Incoding.CQRS;
    using Incoding.Extensions;

    #endregion

    public sealed class CreateByTypeQuery : QueryBase<object>
    {
        private readonly HttpContextBase httpContextBase;

        public CreateByTypeQuery()
        {
            httpContextBase = new HttpContextWrapper(HttpContext.Current);
        }

        public CreateByTypeQuery(HttpContextBase request)
        {
            this.httpContextBase = request;
        }


        protected override object ExecuteResult()
        {
            var byPair = Type.Split(UrlDispatcher.separatorByPair.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string genericType = byPair.ElementAtOrDefault(1);

            var inst = Dispatcher.Query(new FindTypeByName()
                                        {
                                                Type = byPair[0],
                                        });
            var formCollection = Dispatcher.Query(new GetFormCollectionsQuery());
            var instanceType = IsGroup ? typeof(List<>).MakeGenericType(inst) : inst;
            if (instanceType.IsTypicalType() && IsModel)
            {
                string str = formCollection["incValue"];
                if (instanceType == typeof(string))
                    return str;
                if (instanceType == typeof(bool))
                    return bool.Parse(str);
                if (instanceType == typeof(DateTime))
                    return DateTime.Parse(str);
                if (instanceType == typeof(int))
                    return int.Parse(str);
                if (instanceType.IsEnum)
                    return Enum.Parse(instanceType, str);
            }
            else if (!string.IsNullOrWhiteSpace(genericType))
            {
                instanceType = instanceType.MakeGenericType(genericType.Split(UrlDispatcher.separatorByGeneric.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                                                       .Select(name => Dispatcher.Query(new FindTypeByName()
                                                                                                        {
                                                                                                                Type = name,
                                                                                                        }))
                                                                       .ToArray());
            }

            return new DefaultModelBinder().BindModel(ControllerContext ?? new ControllerContext(), new ModelBindingContext()
                                                                                                    {
                                                                                                            ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => Activator.CreateInstance(instanceType), instanceType),
                                                                                                            ModelState = ModelState ?? new ModelStateDictionary(),                                                                                                            
                                                                                                            ValueProvider = ControllerContext != null
                                                                                                                                    ? ValueProviderFactories.Factories.GetValueProvider(ControllerContext)
                                                                                                                                    : formCollection,
                                                                                                    });
        }

        protected override async Task<object> ExecuteResultAsync(CancellationToken ct = default)
        {
            var byPair = Type.Split(UrlDispatcher.separatorByPair.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string genericType = byPair.ElementAtOrDefault(1);

            var inst = await Dispatcher.QueryAsync(new FindTypeByName()
            {
                Type = byPair[0],
            }, null, ct).ConfigureAwait(false);

            var formCollection = await Dispatcher.QueryAsync(new GetFormCollectionsQuery(httpContextBase), ct: ct)
                .ConfigureAwait(false);

            var instanceType = IsGroup ? typeof(List<>).MakeGenericType(inst) : inst;

            if (instanceType.IsTypicalType() && IsModel)
            {
                string str = formCollection["incValue"];
                if (instanceType == typeof(string))
                    return str;
                if (instanceType == typeof(bool))
                    return bool.Parse(str);
                if (instanceType == typeof(DateTime))
                    return DateTime.Parse(str);
                if (instanceType == typeof(int))
                    return int.Parse(str);
                if (instanceType.IsEnum)
                    return Enum.Parse(instanceType, str);
            }
            else if (!string.IsNullOrWhiteSpace(genericType))
            {
                var genericArgs = await Task.WhenAll(
                    genericType.Split(UrlDispatcher.separatorByGeneric.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                               .Select(name => Dispatcher.QueryAsync(new FindTypeByName() { Type = name }, null, ct))
                );
                instanceType = instanceType.MakeGenericType(genericArgs);
            }

            object model = null;
            try
            {
                model = Activator.CreateInstance(instanceType);
            }
            catch (MissingMethodException ex)
            {
                throw new InvalidOperationException($"Cannot create instance of type {instanceType.FullName}.", ex);
            }

            return new DefaultModelBinder().BindModel(
                ControllerContext ?? throw new InvalidOperationException("ControllerContext is not initialized."),
                new ModelBindingContext()
                {
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => model, instanceType),
                    ModelState = ModelState ?? new ModelStateDictionary(),
                    ValueProvider = ControllerContext != null
                        ? ValueProviderFactories.Factories.GetValueProvider(ControllerContext)
                        : formCollection,
                });
        }


        public sealed class AsCommands : QueryBase<CommandBase[]>
        {
            private readonly HttpContextBase httpContextBase;

            public AsCommands()
            {
                httpContextBase = new HttpContextWrapper(HttpContext.Current);
            }

            public AsCommands(HttpContextBase request)
            {
                this.httpContextBase = request;
            }

            public string IncTypes { get; set; }

            public bool? IsComposite { get; set; }

            public ModelStateDictionary ModelState { get; set; }

            public ControllerContext ControllerContext { get; set; }

            protected override CommandBase[] ExecuteResult()
            {
                var splitByType = IncTypes.Split(UrlDispatcher.separatorByType.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                bool isCompositeAsArray = splitByType.Count() == 1 && IsComposite.GetValueOrDefault();
                return isCompositeAsArray
                               ? ((IEnumerable<CommandBase>)Dispatcher.Query(new CreateByTypeQuery()
                                                                             {
                                                                                     Type = splitByType[0],
                                                                                     ControllerContext = ControllerContext,
                                                                                     ModelState = ModelState,
                                                                                     IsGroup = true
                                                                             })).ToArray()
                               : splitByType.Select(r => (CommandBase)Dispatcher.Query(new CreateByTypeQuery() { Type = r, ControllerContext = this.ControllerContext, ModelState = ModelState })).ToArray();
            }

            protected override async Task<CommandBase[]> ExecuteResultAsync(CancellationToken ct = default)
            {
                var splitByType = IncTypes.Split(UrlDispatcher.separatorByType.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                bool isCompositeAsArray = splitByType.Length == 1 && IsComposite.GetValueOrDefault();

                if (isCompositeAsArray)
                {
                    var result = await Dispatcher.QueryAsync(new CreateByTypeQuery(httpContextBase)
                    {
                        Type = splitByType[0],
                        ControllerContext = ControllerContext,
                        ModelState = ModelState,
                        IsGroup = true
                    }, null, ct).ConfigureAwait(false);

                    return ((IEnumerable<CommandBase>)result).ToArray();
                }
                else
                {
                    var tasks = splitByType.Select(type =>
                        Dispatcher.QueryAsync(new CreateByTypeQuery(httpContextBase)
                        {
                            Type = type,
                            ControllerContext = ControllerContext,
                            ModelState = ModelState
                        }, null, ct)
                    );

                    var results = await Task.WhenAll(tasks).ConfigureAwait(false);

                    return results.Cast<CommandBase>().ToArray();
                }
            }

        }

        #region Nested classes

        public   sealed class FindTypeByName : QueryBase<Type>
        {
            #region Static Fields

            static readonly ConcurrentDictionary<string, string> cache = new ConcurrentDictionary<string, string>();

            #endregion

            #region Fields

            public string Type { get; set; }

            #endregion

            protected override Type ExecuteResult()
            {
                string name = HttpUtility.UrlDecode(Type).Replace(" ", "+");
                var assmelbyName = cache.GetOrAdd(name, (i) =>
                                                        {
                                                            var clearName = name.Contains("`") ? name.Split('`')[0] + "`1" : name;
                                                            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                                                            {
                                                                var findType = assembly.GetLoadableTypes().FirstOrDefault(type => type.Name == clearName || type.FullName == clearName);
                                                                if (findType != null)
                                                                    return findType.AssemblyQualifiedName;
                                                            }

                                                            throw new IncMvdException("Not found any type {0}".F(name));
                                                        });
                return System.Type.GetType(assmelbyName);
            }

            protected override Task<Type> ExecuteResultAsync(CancellationToken ct = default)
            {
                return Task.FromResult(ExecuteResult());
            }
        }

        #endregion

        public sealed class GetFormCollectionsQuery : QueryBase<FormCollection>
        {
            HttpContextBase _httpContextBase;

            public GetFormCollectionsQuery()
            {
                _httpContextBase = new HttpContextWrapper(HttpContext.Current);
            }

            public GetFormCollectionsQuery(HttpContextBase request)
            {
                _httpContextBase = request;
            }

            protected override FormCollection ExecuteResult()
            {
                var formAndQuery = new FormCollection(_httpContextBase.Request.Form)
                {
                    _httpContextBase.Request.QueryString
                };
                return formAndQuery;
            }

            protected override Task<FormCollection> ExecuteResultAsync(CancellationToken ct = default)
            {
                return Task.FromResult(ExecuteResult());
            }

        }

        #region Properties

        public string Type { get; set; }

        public bool IsGroup { get; set; }

        public bool IsModel { get; set; }

        public ModelStateDictionary ModelState { get; set; }

        public ControllerContext ControllerContext { get; set; }

        #endregion
    }
}