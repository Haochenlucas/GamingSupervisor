package Dota2Parser.Dota2Parser;

import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;

public class GameTime
{
    public FieldPath time;
    
    public GameTime(Entity e)
    {
    	time = e.getDtClass().getFieldPathForName("m_pGameRules.m_fGameTime");
    }
    
    public boolean isTime(FieldPath path)
    {
        return path.equals(time);
    }
}
