using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayParse
{
    public static class ConvertedHeroName
    {
        public static string Get(string unconverted)
        {
            String parsedHeroName = unconverted.Split(new string[] { "hero_" }, StringSplitOptions.None).Last();
            parsedHeroName = parsedHeroName.Split(new char[] { '\"' }).First();
            String[] temp = parsedHeroName.Split(new char[] { '_' });
            String[] upperCase = new String[temp.Length];
            for (int i = 0; i < temp.Length; i++)
            {
                upperCase[i] = temp[i].First().ToString().ToUpper() + temp[i].Substring(1);
            }
            parsedHeroName = string.Join("", upperCase);
            string name = parsedHeroName.ToLower();
            name = name.Replace(" ", string.Empty);

            if (name.Contains("never"))
            {
                name = "shadowfiend";
            }
            else if (name.Contains("obsidian"))
            {
                name = "outworlddevourer";
            }
            else if (name.Contains("wisp"))
            {
                name = "io";
            }
            else if (name.Contains("magnataur"))
            {
                name = "magnus";
            }
            else if (name.Contains("treant"))
            {
                name = "treantprotector";
            }
            else if (name.Contains("skele"))
            {
                name = "wraithking";
            }
            else if (name.Contains("rattletrap"))
            {
                name = "clockwerk";
            }
            else if (name.Contains("doombringer"))
            {
                name = "doom";
            }
            else if (name.Contains("antimage"))
            {
                name = "anti-mage";
            }
            else if (name.Contains("necrolyte"))
            {
                name = "necrophos";
            }
            else if (name.Contains("zuus"))
            {
                name = "zeus";
            }
            else if (name.Contains("abyssal"))
            {
                name = "underlord";
            }

            return name;
        }
    }
}
