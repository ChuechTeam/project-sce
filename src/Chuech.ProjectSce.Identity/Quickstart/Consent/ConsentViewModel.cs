// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;

namespace Chuech.ProjectSce.Identity.Quickstart.Consent
{
    public class ConsentViewModel : ConsentInputModel
    {
        public string ClientName { get; init; }
        public string? ClientUrl { get; set; }
        public string? ClientLogoUrl { get; set; }
        public bool AllowRememberConsent { get; set; }

        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; } = Array.Empty<ScopeViewModel>();
        public IEnumerable<ScopeViewModel> ApiScopes { get; set; } = Array.Empty<ScopeViewModel>();
    }
}
