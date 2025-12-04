using System.Collections.Generic;
using TaleWorlds.Localization;

namespace AgricultureEstate.l18n
{
    internal class Localization
    {
        public static TextObject SetTextVariables(string text, params KeyValuePair<string, string>[] args)
        {
            TextObject textObject = new TextObject(text, null);
            foreach (KeyValuePair<string, string> keyValuePair in args)
            {
                string value = keyValuePair.Value;

                if (keyValuePair.Key == "GOLD_ICON")
                {
                    value = "<img src=\"General\\Icons\\Coin@2x\" extend=\"8\">";
                }

                textObject.SetTextVariable(keyValuePair.Key, value);
            }
            return textObject;
        }
    }
}