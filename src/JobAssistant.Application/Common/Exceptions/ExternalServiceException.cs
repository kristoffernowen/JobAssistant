namespace JobAssistant.Application.Common.Exceptions;

public sealed class ExternalServiceException(string message) : Exception(message);
