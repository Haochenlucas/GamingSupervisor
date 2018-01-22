package Dota2Parser.Dota2Parser;

import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;

public class Camera
{
	public FieldPath x;
	public FieldPath y;
	public FieldPath z;
	public FieldPath playerID;
	
	public Camera(Entity e)
	{
		x = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellX");
        y = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellY");
        z = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellZ");
        playerID = e.getDtClass().getFieldPathForName("m_iPlayerID");
	}
	
	public boolean isPosition(FieldPath path)
	{
		return (path.equals(x) || path.equals(y) || path.equals(z));
	}
}
