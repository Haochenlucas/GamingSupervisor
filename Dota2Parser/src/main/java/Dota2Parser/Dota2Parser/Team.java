package Dota2Parser.Dota2Parser;

import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;

public class Team
{
	private final int playersOnTeam = 5;
	
	public FieldPath reliableGold[];
	public FieldPath unreliableGold[];
	
	public Team(Entity e)
    {
		reliableGold = new FieldPath[playersOnTeam];
		unreliableGold = new FieldPath[playersOnTeam];
        for (int i = 0; i < playersOnTeam; i++)
        {
        	reliableGold[i] = e.getDtClass().getFieldPathForName("m_vecDataTeam.000" + i + ".m_iReliableGold");
        	unreliableGold[i] = e.getDtClass().getFieldPathForName("m_vecDataTeam.000" + i + ".m_iUnreliableGold");
        }
    }
	
	public boolean isGold(FieldPath path, int playerIndex)
    {
		return path.equals(reliableGold[playerIndex]) || path.equals(unreliableGold[playerIndex]);
    }
}
