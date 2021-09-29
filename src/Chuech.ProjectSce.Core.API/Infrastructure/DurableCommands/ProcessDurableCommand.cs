namespace Chuech.ProjectSce.Core.API.Infrastructure.DurableCommands;

public record ProcessDurableCommand<T>(T Command) where T : class;
