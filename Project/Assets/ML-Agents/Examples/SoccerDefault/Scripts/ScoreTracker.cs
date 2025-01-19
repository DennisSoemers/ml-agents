public class ScoreTracker
{
    private int blueScore, purpleScore, totalMatches,limit;
    private SoccerSettings.ModelType modelTypeBlue, modelTypePurple;
    private static ScoreTracker instance;
    private ScoreTracker(int limit, SoccerSettings.ModelType modelTypeBlue, SoccerSettings.ModelType modelTypePurple)
    {
        this.limit = limit;
        this.modelTypeBlue = modelTypeBlue;
        this.modelTypePurple = modelTypePurple;
        blueScore = 0;
        purpleScore = 0;
        totalMatches = 0;
    }
    public static ScoreTracker GetScoreTracker(int limit,SoccerSettings.ModelType modelTypeBlue, SoccerSettings.ModelType modelTypePurple)
    {
        if (instance == null)
        {
            instance = new ScoreTracker(limit,modelTypeBlue,modelTypePurple);
        }
        return instance;
    }

    public void addScoreBlue()
    {
        blueScore++;
    }
    public void addScorePurple()
    {
        purpleScore++;
    }
    public void addMatch()
    {
        totalMatches++;
    }
    public string checkLimit()
    {
        if (totalMatches == limit)
        {
            return $@"
{totalMatches} played
blue model type: {getModelType(modelTypeBlue)} blue won: {blueScore}
purple model type: {getModelType(modelTypePurple)} purple won: {purpleScore}
";
        }
        return "";
    }

    private string getModelType(SoccerSettings.ModelType modelType)
    {
        if (modelType == SoccerSettings.ModelType.ForwardAndBackwardRaycast)
        {
            return "ForwardAndBackwardRaycast";
        }
        else if (modelType == SoccerSettings.ModelType.SoundAndViewRotation)
        {
            return "SoundAndViewRotation";
        }
        else if (modelType == SoccerSettings.ModelType.OnlyForwardRaycast)
        {
            return "OnlyForwardRaycast";
        }
        return "Unknown";

    }
}