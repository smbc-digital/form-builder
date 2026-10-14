namespace form_builder.Helpers.ViewRender;

public class ViewRender(IRazorViewEngine viewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider)
    : IViewRender
{
    public async Task<string> RenderAsync<TModel>(string viewName, TModel model, Dictionary<string, dynamic> viewData = null)
    {
        var httpContext = new DefaultHttpContext { RequestServices = serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        var viewEngineResult = viewEngine.FindView(actionContext, viewName, false);

        if (!viewEngineResult.Success)
            throw new InvalidOperationException($"Couldn't find view {viewName}");

        var view = viewEngineResult.View;

        using (var output = new StringWriter())
        {
            var viewContext = new ViewContext(
                actionContext,
                view,
                new ViewDataDictionary<TModel>(
                    metadataProvider: new EmptyModelMetadataProvider(),
                    modelState: new ModelStateDictionary())
                {
                    Model = model
                },
                new TempDataDictionary(
                    actionContext.HttpContext,
                    tempDataProvider),
                output,
                new HtmlHelperOptions());

            if (viewData is not null)
                foreach (var item in viewData)
                    viewContext.ViewData.Add(item.Key, item.Value);

            await view.RenderAsync(viewContext);

            return output.ToString();
        }
    }
}