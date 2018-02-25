package Dota2Parser.Dota2Parser;

import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;

public class Spectator
{
	private final int numberOfPlayers = 10;
	
	public FieldPath netWorth[];
	
	public Spectator(Entity e)
    {
		netWorth = new FieldPath[numberOfPlayers];
        for (int i = 0; i < numberOfPlayers; i++)
        {
        	netWorth[i] = e.getDtClass().getFieldPathForName("m_iNetWorth.000" + i);
        }
    }
	
	public boolean isNetWorth(FieldPath path, int playerIndex)
    {
		return path.equals(netWorth[playerIndex]) || path.equals(netWorth[playerIndex]);
    }
}
