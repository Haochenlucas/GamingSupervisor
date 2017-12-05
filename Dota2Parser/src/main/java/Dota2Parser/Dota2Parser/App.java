package Dota2Parser.Dota2Parser;

import java.io.File;
import java.io.PrintWriter;

import skadistats.clarity.Clarity;
import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;
import skadistats.clarity.processor.entities.OnEntityCreated;
import skadistats.clarity.processor.entities.OnEntityUpdated;
import skadistats.clarity.processor.entities.UsesEntities;
import skadistats.clarity.processor.runner.Context;
import skadistats.clarity.processor.runner.SimpleRunner;
import skadistats.clarity.source.MappedFileSource;
import skadistats.clarity.source.Source;
import skadistats.clarity.wire.common.proto.Demo.CDemoFileInfo;

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
	
	private void ensurePositionFieldPaths(Entity e)
	{
        if (x == null)
        {
            x = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellX");
            y = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellY");
        }
    }
	
	private void ensureHealthFieldPaths(Entity e)
	{
		if (health == null)
		{
            health = e.getDtClass().getFieldPathForName("m_iHealth");
		}
	}
	
	@OnEntityCreated
    public void onCreated(Context ctx, Entity e)
	{
        if (!isHero(e))
        {
            return;
        }
        ensurePositionFieldPaths(e);
        ensureHealthFieldPaths(e);
        
        writer.format("%d [POSITION] %s %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(x), e.getPropertyForFieldPath(y));
    	writer.flush();
        System.out.format("%d [POSITION] %s %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(x), e.getPropertyForFieldPath(y));
    
        writer.format("%d [HEALTH] %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
    	writer.flush();
        System.out.format("%d [HEALTH] %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
    }

    @OnEntityUpdated
    public void onUpdated(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount)
    {
        if (!isHero(e))
        {
            return;
        }
        ensurePositionFieldPaths(e);
        ensureHealthFieldPaths(e);
        
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
        	writer.flush();
            System.out.format("%d [POSITION] %s %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(x), e.getPropertyForFieldPath(y));
        }
        if (updateHealth)
        {
        	writer.format("%d [HEALTH] %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
        	writer.flush();
            System.out.format("%d [HEALTH] %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
        }
    }
    
    public void run(String[] args) throws Exception
    {
    	File replayFile = new File(args[1] + "/replay.txt");
    	writer = new PrintWriter(replayFile);

    	CDemoFileInfo info = Clarity.infoForFile(args[0]);
    	File infoFile = new File(args[1] + "/info.txt");
    	PrintWriter w = new PrintWriter(infoFile);
    	w.write(info.getGameInfo().toString());
    	w.close();
    	
    	Source source = new MappedFileSource(args[0]);
    	new SimpleRunner(source).runWith(this);
    	writer.close();
    }

    public static void main(String[] args) throws Exception
    {
    	// 30 ticks per second
    	new App().run(args);
    }

}