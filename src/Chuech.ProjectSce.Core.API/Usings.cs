﻿global using MediatR;
global using FluentValidation;
global using NotSoAutoMapper;
global using Microsoft.EntityFrameworkCore;

global using Chuech.ProjectSce.Core.API.Infrastructure;
global using Chuech.ProjectSce.Core.API.Infrastructure.Authorization;
global using Chuech.ProjectSce.Core.API.Infrastructure.Exceptions;
global using Chuech.ProjectSce.Core.API.Infrastructure.Results;
global using Chuech.ProjectSce.Core.API.Infrastructure.Resilience;

global using NodaTime;

global using IRedisDatabase = StackExchange.Redis.IDatabase;
global using JsonIgnoreAttribute = System.Text.Json.Serialization.JsonIgnoreAttribute;