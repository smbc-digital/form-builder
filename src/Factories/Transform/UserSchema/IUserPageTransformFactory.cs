﻿using form_builder.Models;

namespace form_builder.Factories.Transform.UserSchema
{
    public interface IUserPageTransformFactory
    {
        Task<Page> Transform(Page page, FormAnswers convertedAnswers);
    }
}
