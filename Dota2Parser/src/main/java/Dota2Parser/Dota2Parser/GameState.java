package Dota2Parser.Dota2Parser;

import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;

public class GameState
{
    public FieldPath state;
    
    public GameState(Entity e)
    {
        state = e.getDtClass().getFieldPathForName("m_pGameRules.m_nGameState");
    }
    
    public boolean isState(FieldPath path)
    {
        return path.equals(state);
    }
}
