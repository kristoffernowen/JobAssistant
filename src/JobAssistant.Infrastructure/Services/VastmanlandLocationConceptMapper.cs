using JobAssistant.Application.Common.Interfaces;

namespace JobAssistant.Infrastructure.Services;

public sealed class VastmanlandLocationConceptMapper : ILocationConceptMapper
{
    private static readonly Dictionary<string, IReadOnlyCollection<string>> Mapping =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Västmanland"] = ["G6DV_fKE_Viz"],
            ["Västmanlands län"] = ["G6DV_fKE_Viz"],
            ["Västerås"] = ["8deT_FRF_2SP"],
            ["Arboga"] = ["Jkyb_5MQ_7pB"],
            ["Fagersta"] = ["7D9G_yrX_AGJ"],
            ["Hallstahammar"] = ["oXYf_HmD_ddE"],
            ["Kungsör"] = ["Fac5_h7a_UoM"],
            ["Köping"] = ["4Taz_AuG_tSm"],
            ["Norberg"] = ["jbVe_Cps_vtd"],
            ["Sala"] = ["dAen_yTK_tqz"],
            ["Skinnskatteberg"] = ["Nufj_vmt_VrH"],
            ["Surahammar"] = ["jfD3_Hdg_UhT"]
        };

    public bool TryMapToConceptIds(string locationInput, out IReadOnlyCollection<string> conceptIds)
    {
        var cleaned = locationInput.Trim();
        return Mapping.TryGetValue(cleaned, out conceptIds!);
    }
}
