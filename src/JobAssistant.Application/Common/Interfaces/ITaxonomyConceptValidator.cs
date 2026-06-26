namespace JobAssistant.Application.Common.Interfaces;

public interface ITaxonomyConceptValidator
{
    bool IsValidMunicipalityId(string conceptId);

    bool IsValidOccupationGroupId(string conceptId);
}