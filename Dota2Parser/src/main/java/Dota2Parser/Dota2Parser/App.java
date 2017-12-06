package Dota2Parser.Dota2Parser;

import java.io.File;
import java.io.PrintWriter;
import java.util.HashMap;
import java.util.Iterator;
import java.util.Map;

import skadistats.clarity.Clarity;
import skadistats.clarity.model.Entity;
import skadistats.clarity.model.FieldPath;
import skadistats.clarity.processor.entities.Entities;
import skadistats.clarity.processor.entities.OnEntityCreated;
import skadistats.clarity.processor.entities.OnEntityUpdated;
import skadistats.clarity.processor.entities.UsesEntities;
import skadistats.clarity.processor.reader.OnTickStart;
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
    private FieldPath z;
    private FieldPath health;
    
    private Map<Integer, Entity> players;
    
	private boolean isHero(Entity e)
	{
		return e.getDtClass().getDtName().startsWith("CDOTA_Unit_Hero");
	}
	
	private boolean isPlayer(Entity e)
	{
		return e.getDtClass().getDtName().startsWith("CDOTAPlayer");
	}
	
	private void ensurePositionFieldPaths(Entity e)
	{
        if (x == null)
        {
            x = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellX");
            y = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellY");
            z = e.getDtClass().getFieldPathForName("CBodyComponent.m_cellZ");
        }
    }
	
	private void ensureHealthFieldPaths(Entity e)
	{
		if (health == null)
		{
            health = e.getDtClass().getFieldPathForName("m_iHealth");
		}
	}
	
	private Integer[] coordFromCell(Entity e)
	{
        Integer[] coord = new Integer[2];

        int x = e.getProperty("CBodyComponent.m_cellX");
        int y = e.getProperty("CBodyComponent.m_cellY");

        float xOffset = e.getProperty("CBodyComponent.m_vecX");
        float yOffset = e.getProperty("CBodyComponent.m_vecY");
        //cellXY*128+vecXY-32768/2;
        int x1 = (int)((x * 128 + xOffset - 8192) / 16384.0);
        int y1 = (int)((24576 - 128 * y - yOffset) / 16384.0);

        coord[0] = x1;
        coord[1] = y1;

        return coord;
    }
	
    @OnTickStart
    public void onTickStart(Context ctx, boolean synthetic)
    {
    	//writer.format("Time %d\n", ctx.getTick());
        //Entity pr = ctx.getProcessor(Entities.class).getByDtName("CDOTA_PlayerResource");
        Iterator<Entity> n = ctx.getProcessor(Entities.class).getAllByDtName("CDOTAPlayer");
        players = new HashMap<>();
        while (n.hasNext())
        {
            Entity en = (Entity)n.next();
            players.put(en.getProperty("m_iPlayerID"), en);
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
        
        writer.format("%d [POSITION] %s %s %s %s\n", ctx.getTick(),
        		e.getDtClass().getDtName(),
        		e.getPropertyForFieldPath(x),
        		e.getPropertyForFieldPath(y),
        		e.getPropertyForFieldPath(z));
    	writer.flush();
        System.out.format("%d [POSITION] %s %s %s %s\n",ctx.getTick(),
        		e.getDtClass().getDtName(),
        		e.getPropertyForFieldPath(x),
        		e.getPropertyForFieldPath(y),
        		e.getPropertyForFieldPath(z));
    
        writer.format("%d [HEALTH] %s %s\n", ctx.getTick(),
        		e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
    	writer.flush();
        System.out.format("%d [HEALTH] %s %s\n", ctx.getTick(),
        		e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
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
        for (int i = 0; i < updateCount; i++)
        {
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
        	writer.format("%d [POSITION] %s %s %s %s\n", ctx.getTick(),
        			e.getDtClass().getDtName(),
        			e.getPropertyForFieldPath(x),
        			e.getPropertyForFieldPath(y),
        			e.getPropertyForFieldPath(z));
        	writer.flush();
            System.out.format("%d [POSITION] %s %s %s %s\n", ctx.getTick(),
            		e.getDtClass().getDtName(),
            		e.getPropertyForFieldPath(x),
            		e.getPropertyForFieldPath(y),
            		e.getPropertyForFieldPath(z));
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
    	w.write(info.toString());
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