using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    /*
        Knowledge(difficulty_table: column index -1)
        The amount of knowledge about heroes, abilities, items, … that is required to play the hero effectively.

        1:Requires basic knowledge about the game and the own hero.
        2:Requires the player to know which heroes to jump.
        3:Requires knowledge about other players heroes, abilities and items.
        4:Requires advanced knowledge; adapting item builds to the game.
        5:Requires advanced knowledge about all heroes, abilities and items.

        Awareness(difficulty_table: column index -2)

        1:The hero requires basic awareness of the close surroundings of the hero.
        2:The hero requires basic awareness of the entire map.
        3:Needs to be aware of his surroundings to escape from ganks or to land long range skills.
        4:Needs to be aware of the state of the game, as well as the positions of enemies.
        5:Needs to be aware of what's happening around the map to land global skills or to gank enemies.
        
        Positioning(difficulty_table: column index -3)
        
        1:The hero has no special requirements in terms of positioning.
        2:The hero has abilities that rely on good positioning, but is useful even without them.
        3:The hero has abilities that rely on good positioning and are important to hit.
        4:The hero constantly has to be in the correct positions to save people/counter-gank
        5:The hero heavily relies on good positioning to stay alive (has no abilities to escape and is squishy)
        
        Farming(difficulty_table: column index -4)

        1:The hero needs no items to be relevant in the game
        2:The hero needs some farm in order to become relevant
        3:The hero needs a lot of farm; has an easy time getting it
        4:The hero needs a key item quickly
        5:The hero needs a lot of farm; has a hard time getting it

        Mechanics(difficulty_table: column index -5)

        1:Simple spells; no exact timing or quick reactions necessary
        2:Non-targeted spells; quick reactions necessary
        3:The hero relies on good item and ability usage to be effective.
        4:The hero needs to control multiple units.
        5:The hero needs to control multiple units with different abilities or relies on quick reflexes to stay alive.
        
        Survival(difficulty_table: column index -6)

        1:The hero can die without being heavily punished for it.
        2:The hero needs to stay alive long enough to get off a key spell.
        3:The hero needs to continually use abilities in fights to be useful.
        4:The hero gets punished heavily for dying
        5:The hero absolutely can't afford to die.

        Final Rating(difficulty_table: column index -7)
        For the final rating these six numbers get added together. The final rating can range from 6 (the easiest) to 30 (the hardest).
     */

    public class hero_difficulty
    {
        public static int[,] difficulty_table = new int[116, 8];
        public hero_difficulty()
        {
            string s = Path.Combine(Environment.CurrentDirectory, @"..\..\..\replayParse\Properties\hero_difficulty_version_1.txt");
            string[] lines = System.IO.File.ReadAllLines(s);
            string[] second_lines = lines;
            int key = 0;
            int length_name = 0;
            foreach (string line in lines)
            {
                key++;
                length_name = 0;
                string second_string = string.Empty;
                string[] words = line.Split('\t');
                string[] name = words[0].Split(' ');
                length_name = name.Length;
                
                for (int i = 0; i < length_name; i++)
                {
                    Int32.TryParse(name[i], out difficulty_table[key, i]);
                }
            }
        }

        /*
        * provide the whole rating table.
        */
        public int[,] getDiffTable()
        {
            return difficulty_table;
        }

        /*
         * provide a table which only contain hero_id and hero final rating.
         */
        public int[,] getDiffFinal()
        {
            int[,] diffFinal = new int[116, 2];
            for(int i = 0; i< 116; i++)
            {
                diffFinal[i, 0] = difficulty_table[i, 0];
                diffFinal[i, 1] = difficulty_table[i, 7];
             }
            return diffFinal;
        }

        public string mainDiff(int heroID)
        {
            string s = "";
            int max = 0;
            List<int> hardBar = new List<int>();
            for(int i = 1; i < 7; i++)
            {
                if(difficulty_table[heroID, i] >= max)
                {
                    max = difficulty_table[heroID, i];
                }
            }

            for (int i = 1; i < 7; i++)
            {
                if (difficulty_table[heroID, i] >= max)
                {
                    hardBar.Add(i);
                }
            }
            int mark = 0;
            foreach(int hardI in hardBar)
            {
                if(hardI == 1)
                {
                    if (mark == 0)
                    {
                        mark++;
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = "Knowledge: Requires basic knowledge about the game and the own hero.";
                                break;
                            case 2:
                                s = "Knowledge: Requires the player to know which heroes to jump.";
                                break;
                            case 3:
                                s = "Knowledge: Requires knowledge about other players heroes, abilities and items.";
                                break;
                            case 4:
                                s = "Knowledge: Requires advanced knowledge; adapting item builds to the game.";
                                break;
                            case 5:
                                s = "Knowledge: Requires advanced knowledge about all heroes, abilities and items.";
                                break;
                        }
                    }
                    else
                    {
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = s + "\r\n" + "Knowledge: Requires basic knowledge about the game and the own hero.";
                                break;
                            case 2:
                                s = s + "\r\n" + "Knowledge: Requires the player to know which heroes to jump.";
                                break;
                            case 3:
                                s = s + "\r\n" + "Knowledge: Requires knowledge about other players heroes, abilities and items.";
                                break;
                            case 4:
                                s = s + "\r\n" + "Knowledge: Requires advanced knowledge; adapting item builds to the game.";
                                break;
                            case 5:
                                s = s+ "\r\n" + "Knowledge: Requires advanced knowledge about all heroes, abilities and items.";
                                break;
                        }
                    }    
                }
                else if (hardI == 2)
                {
                    if (mark == 0)
                    {
                        mark++;
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = "Awareness: The hero requires basic awareness of the close surroundings of the hero.";
                                break;
                            case 2:
                                s = "Awareness: The hero requires basic awareness of the entire map.";
                                break;
                            case 3:
                                s = "Awareness: Needs to be aware of his surroundings to escape from ganks or to land long range skills.";
                                break;
                            case 4:
                                s = "Awareness: Needs to be aware of the state of the game, as well as the positions of enemies.";
                                break;
                            case 5:
                                s = "Awareness: Needs to be aware of what's happening around the map to land global skills or to gank enemies.";
                                break;
                        }
                    }
                    else
                    {
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = s + "\r\n" + "Awareness: The hero requires basic awareness of the close surroundings of the hero.";
                                break;
                            case 2:
                                s = s + "\r\n" + "Awareness: The hero requires basic awareness of the entire map.";
                                break;
                            case 3:
                                s = s + "\r\n" + "Awareness: Needs to be aware of his surroundings to escape from ganks or to land long range skills.";
                                break;
                            case 4:
                                s = s + "\r\n" + "Awareness: Needs to be aware of the state of the game, as well as the positions of enemies.";
                                break;
                            case 5:
                                s = s + "\r\n" + "Awareness: Needs to be aware of what's happening around the map to land global skills or to gank enemies.";
                                break;
                        }
                    }
                }
                else if (hardI == 3)
                {
                    if (mark == 0)
                    {
                        mark++;
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = "Positioning: The hero has no special requirements in terms of positioning.";
                                break;
                            case 2:
                                s = "Positioning: The hero has abilities that rely on good positioning, but is useful even without them.";
                                break;
                            case 3:
                                s = "Positioning: The hero has abilities that rely on good positioning and are important to hit.";
                                break;
                            case 4:
                                s = "Positioning: The hero constantly has to be in the correct positions to save people/counter-gank.";
                                break;
                            case 5:
                                s = "Positioning: The hero heavily relies on good positioning to stay alive (has no abilities to escape and is squishy.";
                                break;
                        }
                    }
                    else
                    {
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = s + "\r\n" + "Positioning: The hero has no special requirements in terms of positioning.";
                                break;
                            case 2:
                                s = s + "\r\n" + "Positioning: The hero has abilities that rely on good positioning, but is useful even without them.";
                                break;
                            case 3:
                                s = s + "\r\n" + "Positioning: The hero has abilities that rely on good positioning and are important to hit.";
                                break;
                            case 4:
                                s = s + "\r\n" + "Positioning: The hero constantly has to be in the correct positions to save people/counter-gank.";
                                break;
                            case 5:
                                s = s + "\r\n" + "Positioning: The hero heavily relies on good positioning to stay alive (has no abilities to escape and is squishy.";
                                break;
                        }
                    }
                }
                else if (hardI == 4)
                {
                    if (mark == 0)
                    {
                        mark++;
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = "Farming: The hero needs no items to be relevant in the game.";
                                break;
                            case 2:
                                s = "Farming: The hero needs some farm in order to become relevant.";
                                break;
                            case 3:
                                s = "Farming: The hero needs a lot of farm; has an easy time getting it.";
                                break;
                            case 4:
                                s = "Farming: The hero needs a key item quickly.";
                                break;
                            case 5:
                                s = "Farming: The hero needs a lot of farm; has a hard time getting it.";
                                break;
                        }
                    }
                    else
                    {
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = s + "\r\n" + "Farming: The hero needs no items to be relevant in the game.";
                                break;
                            case 2:
                                s = s + "\r\n" + "Farming: The hero needs some farm in order to become relevant.";
                                break;
                            case 3:
                                s = s + "\r\n" + "Farming: The hero needs a lot of farm; has an easy time getting it.";
                                break;
                            case 4:
                                s = s + "\r\n" + "Farming: The hero needs a key item quickly.";
                                break;
                            case 5:
                                s = s + "\r\n" + "Farming: The hero needs a lot of farm; has a hard time getting it.";
                                break;
                        }
                    }
                }
                else if (hardI == 5)
                {
                    if (mark == 0)
                    {
                        mark++;
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = "Mechanics: Simple spells; no exact timing or quick reactions necessary.";
                                break;
                            case 2:
                                s = "Mechanics: Non-targeted spells; quick reactions necessary.";
                                break;
                            case 3:
                                s = "Mechanics: The hero relies on good item and ability usage to be effective.";
                                break;
                            case 4:
                                s = "Mechanics: The hero needs to control multiple units.";
                                break;
                            case 5:
                                s = "Mechanics: The hero needs to control multiple units with different abilities or relies on quick reflexes to stay alive.";
                                break;
                        }
                    }
                    else
                    {
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = s + "\r\n" + "Mechanics: Simple spells; no exact timing or quick reactions necessary.";
                                break;
                            case 2:
                                s = s + "\r\n" + "Mechanics: Non-targeted spells; quick reactions necessary.";
                                break;
                            case 3:
                                s = s + "\r\n" + "Mechanics: The hero relies on good item and ability usage to be effective.";
                                break;
                            case 4:
                                s = s + "\r\n" + "Mechanics: The hero needs to control multiple units.";
                                break;
                            case 5:
                                s = s + "\r\n" + "Mechanics: The hero needs to control multiple units with different abilities or relies on quick reflexes to stay alive.";
                                break;
                        }
                    }
                }
                else if (hardI == 6)
                {
                    if (mark == 0)
                    {
                        mark++;
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = "Survival: The hero can die without being heavily punished for it.";
                                break;
                            case 2:
                                s = "Survival: The hero needs to stay alive long enough to get off a key spell.";
                                break;
                            case 3:
                                s = "Survival: The hero needs to continually use abilities in fights to be useful.";
                                break;
                            case 4:
                                s = "Survival: The hero gets punished heavily for dying.";
                                break;
                            case 5:
                                s = "Survival: The hero absolutely can't afford to die.";
                                break;
                        }
                    }
                    else
                    {
                        switch (difficulty_table[heroID, hardI])
                        {
                            case 1:
                                s = s + "\r\n" + "Survival: The hero can die without being heavily punished for it.";
                                break;
                            case 2:
                                s = s + "\r\n" + "Survival: The hero needs to stay alive long enough to get off a key spell.";
                                break;
                            case 3:
                                s = s + "\r\n" + "Survival: The hero needs to continually use abilities in fights to be useful.";
                                break;
                            case 4:
                                s = s + "\r\n" + "Survival: The hero gets punished heavily for dying.";
                                break;
                            case 5:
                                s = s + "\r\n" + "Survival: The hero absolutely can't afford to die.";
                                break;
                        }
                    }
                }
            }
            return s;
        }

        public int getFinalRating(int heroID)
        {
            return difficulty_table[heroID, 7];
        }

        /*
         * get the difficulty level of certain hero.
         * string[0]: store the level: Beginner,Intermediate,Advanced.
         * string[1]: store the relevent explanation.
         */
        public string[] getFinalLevel(int heroID)
        {
            string[] haha = new string[2];
            if (difficulty_table[heroID, 7]<= 16)
            {
                haha[0] = "Beginner";
                haha[1] = "This hero is considered suitable for beginning players.";
             }
            else if (difficulty_table[heroID, 7] <= 22)
            {
                haha[0] = "Intermediate";
                haha[1] = "This hero is considered suitable for intermediate players.";
            }
            else
            {
                haha[0] = "Advanced";
                haha[1] = "This hero is considered suitable for advanced players.";
            }
            return haha;
        }

        /*
         * get the difficulty level of certain hero represent by integer:
         *  Beginner-1,Intermediate-2,Advanced-3.
         */

        public int getFinalIntLevel(int heroID)
        {
            string[] haha = new string[2];
            if (difficulty_table[heroID, 7] <= 16)
            {
                return 1;
            }
            else if (difficulty_table[heroID, 7] <= 22)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }
    }
}
