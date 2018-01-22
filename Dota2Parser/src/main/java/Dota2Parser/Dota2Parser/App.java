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
    private PrintWriter positionWriter;
    private PrintWriter healthWriter;
    private PrintWriter heroSelectionWriter;
    
    private FieldPath x;
    private FieldPath y;
    private FieldPath z;
    private FieldPath health;
    private FieldPath heroSelections[];
    private FieldPath heroBans[];
    
    private boolean isHero(Entity e)
    {
        return e.getDtClass().getDtName().startsWith("CDOTA_Unit_Hero");
    }
    
    private boolean isGameRules(Entity e)
    {
    	return e.getDtClass().getDtName().equals("CDOTAGamerulesProxy");
    }
    
    private void ensureHeroSelectionFieldPaths(Entity e)
    {
    	if (heroSelections == null)
    	{
    		heroSelections = new FieldPath[10];
    		heroBans = new FieldPath[12];
    		
    		for (int i = 0; i < 10; i++)
    		{
    			heroSelections[i] = e.getDtClass().getFieldPathForName("m_pGameRules.m_SelectedHeroes.000" + i);
    		}
    		for (int i = 0; i < 10; i++)
    		{
    			heroBans[i] = e.getDtClass().getFieldPathForName("m_pGameRules.m_BannedHeroes.000" + i);
    		}
    	}
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
    
    @OnEntityCreated
    public void onCreated(Context ctx, Entity e)
    {
        if (!isHero(e))
        {
            return;
        }
        ensurePositionFieldPaths(e);
        ensureHealthFieldPaths(e);
        
        positionWriter.format("%d [POSITION] %s %s %s %s\n", ctx.getTick(),
                e.getDtClass().getDtName(),
                e.getPropertyForFieldPath(x),
                e.getPropertyForFieldPath(y),
                e.getPropertyForFieldPath(z));
        positionWriter.flush();
    
        healthWriter.format("%d [HEALTH] %s %s\n", ctx.getTick(),
                e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
        healthWriter.flush();
    }

    @OnEntityUpdated
    public void onUpdated(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount)
    {
        if (isHero(e))
        {
            handleHero(ctx, e, updatedPaths, updateCount);
        }
        else if (isGameRules(e))
        {
        	handleHeroSelection(ctx, e, updatedPaths, updateCount);
        }        
    }
    
    private void handleHeroSelection(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount)
    {
    	ensureHeroSelectionFieldPaths(e);
    	
    	boolean updateSelection[] = new boolean[10];
        boolean updateBan[] = new boolean[12];
        for (int i = 0; i < updateCount; i++)
        {
        	for (int j = 0; j < 10; j++)
        	{
        		if (updatedPaths[i].equals(heroSelections[j]))
                {
        			updateSelection[j] = true;
                }
        	}
            
        	for (int j = 0; j < 10; j++)
        	{
        		if (updatedPaths[i].equals(heroBans[j]))
                {
        			updateBan[j] = true;
                }
        	}
        }        
        
        for (int i = 0; i < 10; i++)
        {
        	if (updateSelection[i])
        	{
        		heroSelectionWriter.format("%d [SELECT] %s\n",
        				ctx.getTick(),
        				e.getPropertyForFieldPath(heroSelections[i]));
            	heroSelectionWriter.flush();
        	}
        }
    	
        for (int i = 0; i < 10; i++)
        {
        	if (updateBan[i])
        	{
        		heroSelectionWriter.format("%d [BAN] %s\n",
        				ctx.getTick(),
        				e.getPropertyForFieldPath(heroBans[i]));
            	heroSelectionWriter.flush();
        	}
        }
    }
    
    private void handleHero(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount)
    {
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
            positionWriter.format("%d [POSITION] %s %s %s %s\n", ctx.getTick(),
                    e.getDtClass().getDtName(),
                    e.getPropertyForFieldPath(x),
                    e.getPropertyForFieldPath(y),
                    e.getPropertyForFieldPath(z));
            positionWriter.flush();
            //System.out.format("%d [POSITION] %s %s %s %s\n", ctx.getTick(),
            //        e.getDtClass().getDtName(),
            //        e.getPropertyForFieldPath(x),
            //        e.getPropertyForFieldPath(y),
            //        e.getPropertyForFieldPath(z));
        }
        if (updateHealth)
        {
            healthWriter.format("%d [HEALTH] %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
            healthWriter.flush();
            //System.out.format("%d [HEALTH] %s %s\n", ctx.getTick(), e.getDtClass().getDtName(), e.getPropertyForFieldPath(health));
        }
    }
    
    public void run(String[] args) throws Exception
    {
        File positionFile = new File(args[1] + "/position.txt");
        File healthFile = new File(args[1] + "/health.txt");
        File selectionFile = new File(args[1] + "/selection.txt");
        positionWriter = new PrintWriter(positionFile);
        healthWriter = new PrintWriter(healthFile);
        heroSelectionWriter = new PrintWriter(selectionFile);

        CDemoFileInfo info = Clarity.infoForFile(args[0]);
        File infoFile = new File(args[1] + "/info.txt");
        PrintWriter w = new PrintWriter(infoFile);
        w.write(info.toString());
        w.close();
        
        Source source = new MappedFileSource(args[0]);
        new SimpleRunner(source).runWith(this);
        
        positionWriter.close();
        healthWriter.close();
        heroSelectionWriter.close();
    }

    public static void main(String[] args) throws Exception
    {
    	// args[0] is location of .dem file
    	// args[1] is location to store files
    	
        // 30 ticks per second
        new App().run(args);
    }

}