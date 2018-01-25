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
    private PrintWriter heroPositionWriter;
    private PrintWriter healthWriter;
    private PrintWriter heroSelectionWriter;
    private PrintWriter cameraWriter;
    
    private Hero hero;
    private Camera camera;
    private Selection selection;
    
    private boolean isHero(Entity e)
    {
        return e.getDtClass().getDtName().startsWith("CDOTA_Unit_Hero");
    }
    
    private boolean isGameRules(Entity e)
    {
    	return e.getDtClass().getDtName().equals("CDOTAGamerulesProxy");
    }
    
    private boolean isPlayer(Entity e)
    {
    	return e.getDtClass().getDtName().equals("CDOTAPlayer");
    }
    
    private void initializeSelection(Entity e)
    {
    	if (selection == null)
    	{
    		selection = new Selection(e);
    	}
    }
    
    private void initializeHero(Entity e)
    {
        if (hero == null)
        {
        	hero = new Hero(e);            
        }
    }
    
    private void initializeCamera(Entity e)
    {
        if (camera == null)
        {
        	camera = new Camera(e);
        }
    }
    
    @OnEntityCreated
    public void onCreated(Context ctx, Entity e)
    {
        if (!isHero(e))
        {
            return;
        }
        initializeHero(e);
        
        handleHero(ctx, e, null, 0, true);
    }

    @OnEntityUpdated
    public void onUpdated(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount)
    {
        if (isHero(e))
        {
            handleHero(ctx, e, updatedPaths, updateCount, false);
        }
        else if (isGameRules(e))
        {
        	handleHeroSelection(ctx, e, updatedPaths, updateCount);
        }
        else if (isPlayer(e))
        {
        	handleCamera(ctx, e, updatedPaths, updateCount);
        }
    }
    
    private void handleCamera(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount)
    {
    	initializeCamera(e);
    	
    	boolean updatePosition = false;
        for (int i = 0; i < updateCount; i++)
        {
            if (camera.isPosition(updatedPaths[i]))
            {
                updatePosition = true;
                break;
            }
        }
        
        if (updatePosition)
        {
        	cameraWriter.format("%d [POSITION] %s %s %s %s\n", ctx.getTick(),
                    e.getPropertyForFieldPath(camera.playerID),
                    e.getPropertyForFieldPath(camera.x),
                    e.getPropertyForFieldPath(camera.y),
                    e.getPropertyForFieldPath(camera.z));
        	cameraWriter.flush();
        }
    }
    
    private void handleHeroSelection(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount)
    {
    	initializeSelection(e);
    	
    	boolean updateSelection[] = new boolean[10];
        boolean updateBan[] = new boolean[12];
        for (int i = 0; i < updateCount; i++)
        {
        	for (int j = 0; j < 10; j++)
        	{
        		if (updatedPaths[i].equals(selection.selections[j]))
                {
        			updateSelection[j] = true;
                }
        	}
            
        	for (int j = 0; j < 10; j++)
        	{
        		if (updatedPaths[i].equals(selection.bans[j]))
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
        				e.getPropertyForFieldPath(selection.selections[i]));
            	heroSelectionWriter.flush();
        	}
        }
    	
        for (int i = 0; i < 10; i++)
        {
        	if (updateBan[i])
        	{
        		heroSelectionWriter.format("%d [BAN] %s\n",
        				ctx.getTick(),
        				e.getPropertyForFieldPath(selection.bans[i]));
            	heroSelectionWriter.flush();
        	}
        }
    }
    
    private void handleHero(Context ctx, Entity e, FieldPath[] updatedPaths, int updateCount, boolean forceUpdate)
    {        
        boolean updatePosition = false;
        boolean updateHealth = false;
        for (int i = 0; i < updateCount; i++)
        {
            if (hero.isPosition(updatedPaths[i]))
            {
                updatePosition = true;
            }
            if (hero.isHealth(updatedPaths[i]))
            {
                updateHealth = true;
            }
        }
        
        if (updatePosition || forceUpdate)
        {
        	heroPositionWriter.format("%d [POSITION] %s %s %s %s\n", ctx.getTick(),
                    e.getDtClass().getDtName(),
                    e.getPropertyForFieldPath(hero.x),
                    e.getPropertyForFieldPath(hero.y),
                    e.getPropertyForFieldPath(hero.z));
        	heroPositionWriter.flush();
        }
        if (updateHealth || forceUpdate)
        {
            healthWriter.format("%d [HEALTH] %s %s\n", ctx.getTick(),
            		e.getDtClass().getDtName(),
            		e.getPropertyForFieldPath(hero.health));
            healthWriter.flush();
        }
    }
    
    public void run(String[] args) throws Exception
    {
    	CDemoFileInfo info = Clarity.infoForFile(args[0]);
        File infoFile = new File(args[1] + "/info.txt");
        PrintWriter w = new PrintWriter(infoFile);
        w.write(info.toString());
        w.close();
        
        File positionFile = new File(args[1] + "/position.txt");
        File healthFile = new File(args[1] + "/health.txt");
        File selectionFile = new File(args[1] + "/selection.txt");
        File cameraFile = new File(args[1] + "/camera.txt");
        
        heroPositionWriter = new PrintWriter(positionFile);
        healthWriter = new PrintWriter(healthFile);
        heroSelectionWriter = new PrintWriter(selectionFile);
        cameraWriter = new PrintWriter(cameraFile);
        
        Source source = new MappedFileSource(args[0]);
        new SimpleRunner(source).runWith(this);
        
        heroPositionWriter.close();
        healthWriter.close();
        heroSelectionWriter.close();
        cameraWriter.close();
    }

    public static void main(String[] args) throws Exception
    {
    	// args[0] is location of .dem file
    	// args[1] is location to store files
    	
        // 30 ticks per second
        new App().run(args);
    }

}