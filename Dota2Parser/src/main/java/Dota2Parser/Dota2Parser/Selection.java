package Dota2Parser.Dota2Parser;

import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;

public class Selection
{
    public FieldPath selections[];
    public FieldPath bans[];
    
    public Selection(Entity e)
    {
        selections = new FieldPath[10];
        bans = new FieldPath[12];
        
        for (int i = 0; i < 10; i++)
        {
            selections[i] = e.getDtClass().getFieldPathForName("m_pGameRules.m_SelectedHeroes.000" + i);
        }
        for (int i = 0; i < 10; i++)
        {
            bans[i] = e.getDtClass().getFieldPathForName("m_pGameRules.m_BannedHeroes.000" + i);
        }
        for (int i = 10; i < 12; i++)
        {
            bans[i] = e.getDtClass().getFieldPathForName("m_pGameRules.m_BannedHeroes.00" + i);
        }
    }    
}
