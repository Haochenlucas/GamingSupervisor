package Dota2Parser.Dota2Parser;

import java.io.File;
import java.io.PrintWriter;

import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;
import skadistats.clarity.processor.entities.OnEntityCreated;
import skadistats.clarity.processor.entities.OnEntityUpdated;
import skadistats.clarity.processor.entities.UsesEntities;
import skadistats.clarity.processor.runner.Context;
import skadistats.clarity.processor.runner.SimpleRunner;
import skadistats.clarity.source.MappedFileSource;
import skadistats.clarity.source.Source;

@UsesEntities
public class App
{	
	private PrintWriter writer;
	
	private FieldPath x;
    private FieldPath y;
    private FieldPath health;
    
	private boolean isHero(Entity e)
	{
		return e.getDtClass().getDtName().startsWith("CDOTA_Unit_Hero");
	}
	
	private void ensureFieldPaths(Entity e)
	{
        if (x == null)
        {
            x = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellX");
            y = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellY");
            health = e.getDtClass().getFieldPathForName("m_iHealth");
        }
    }
	
	/*@OnEntityCreated
    public void onCreated(Context ctx, Entity e) {
        if (!isHero(e))
        {
            return;
        }
        ensureFieldPaths(e);
        //System.out.format("%s (%s/%s)\n", e.getDtClass().getDtName(), e.getPropertyForFieldPath(x), e.getPropertyForFieldPath(y));
    }*/

    @OnEntityUpdated
    public void onUpdated(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount)
    {
        if (!isHero(e))
        {
            return;
        }
        ensureFieldPaths(e);
        
        boolean updatePosition = false;
        boolean updateHealth = false;
        for (int i = 0; i < updateCount; i++) {
            if (updatedPaths[i].equals(x) || updatedPaths[i].equals(y))
            {
                updatePosition = true;
            }
            if (updatedPaths[i].equals(health))
            {
            	updateHealth = true;
            }
        }
        
        if (updatePosition)
        {
        	writer.format("%d [POSITION] %s %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(x), e.getPropertyForFieldPath(y));
            System.out.format("%d [POSITION] %s %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(x), e.getPropertyForFieldPath(y));
        }
        else if (updateHealth)
        {
        	writer.format("%d [HEALTH] %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
            System.out.format("%d [HEALTH] %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
        }
    }
    
    public void run(String[] args) throws Exception
    {
    	File file = new File(args[1] + "/replay.txt");
    	writer = new PrintWriter(file);
    	
    	Source source = new MappedFileSource(args[0]);
    	new SimpleRunner(source).runWith(this);    	
    }

    public static void main(String[] args) throws Exception
    {
    	// 30 ticks per second
    	new App().run(args);
    }

}