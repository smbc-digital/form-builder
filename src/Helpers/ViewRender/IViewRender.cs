﻿namespace form_builder.Helpers.ViewRender
{
    public interface IViewRender
    {
        Task<string> RenderAsync<TModel>(string viewName, TModel model, Dictionary<string, dynamic> viewData = null);
    }
}
