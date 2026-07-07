namespace JobAssistant.Application.Common.Interfaces;

public interface ILocationConceptMapper
{
    bool TryMapToConceptIds(string locationInput, out IReadOnlyCollection<string> conceptIds);
}
