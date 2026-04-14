using System.Collections.Generic;

public class LevelManager
{
    private readonly CampaignService campaignService;

    public LevelManager(CampaignService campaignService)
    {
        this.campaignService = campaignService;
    }

    public LevelRuntime CurrentLevel => campaignService.CurrentLevel;
    public int CurrentLevelIndex => campaignService.CurrentLevelIndex;
    public IReadOnlyList<LevelRuntime> Levels => campaignService.Levels;

    public bool TrySetCurrentLevelIndex(int index)
    {
        return campaignService.TrySetCurrentLevelIndex(index);
    }

    public bool TryAdvanceToNextLevelOrRestart()
    {
        return campaignService.TryAdvanceToNextLevelOrRestart();
    }
}
