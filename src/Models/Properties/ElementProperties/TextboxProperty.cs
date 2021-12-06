﻿using form_builder.Enum;

namespace form_builder.Models.Properties.ElementProperties
{
    public partial class BaseProperty
    {
        public ESize Width { get; set; }
        public string Prefix { get; set; }
        public bool Decimal { get; set; } = false;
        public int DecimalSpaces { get; set; } = 2;
    }
}