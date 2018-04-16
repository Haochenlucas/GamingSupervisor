package Dota2Parser.Dota2Parser;

import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;

public class GameTime
{
    public FieldPath realTime;
    public FieldPath replayTime;
    
    public GameTime(Entity e)
    {
    	realTime = e.getDtClass().getFieldPathForName("m_pGameRules.m_fGameTime");
    	replayTime = e.getDtClass().getFieldPathForName("m_pGameRules.m_flGameStartTime");
    }

    public boolean isRealTime(FieldPath path)
    {
        return path.equals(realTime);
    }
    
    public boolean isReplayTime(FieldPath path)
    {
        return path.equals(replayTime);
    }
}
