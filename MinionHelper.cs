using System;
using System.Linq;
using System.Numerics;
using System.Drawing;
using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;

namespace MinionHelper
{
    public class MinionHelper : BaseSettingsPlugin<MinionHelperSettings>
    {
        public override bool Initialise() => true;

        public override void Render()
        {
            if (!Settings.Enable) return;

            var painOfferingSpikes = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster]
                .Where(e => e.IsAlive && e.Metadata == "Metadata/Monsters/OfferingSpike/PainOfferingSpike");

            foreach (var spike in painOfferingSpikes)
            {
                RenderPainOffering(spike);
            }

            RenderGrimFeast();
        }

        private void RenderPainOffering(Entity spike)
        {
            var stats = spike.GetComponent<Stats>();
            if (stats?.StatDictionary == null) return;

            if (!stats.StatDictionary.TryGetValue(GameStat.ActiveSkillAreaOfEffectRadius, out var aoeValue)) return;

            var buffs = spike.GetComponent<Buffs>();
            if (buffs == null) return;

            var buff = buffs.BuffsList.FirstOrDefault(b => b.Name == "pain_offering_buff");
            if (buff == null) return;

            float scaledAoe = aoeValue * 10;
            float damage = stats.StatDictionary.TryGetValue(GameStat.PainOfferingDamagePctFinal, out var dmg) ? dmg : 0;
            var worldPos = spike.Pos;
            var screenPos = GameController.IngameState.Camera.WorldToScreen(worldPos);

            // Calculate minion percentages
            var minions = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster]
                .Where(e => e.IsAlive && e.Path?.Contains("PlayerSummoned") == true);
            
            int total = minions.Count();
            if (total == 0) return;

            int inRange = minions.Count(m => Vector3.Distance(m.Pos, worldPos) <= scaledAoe);
            float inRangePercent = ((float)inRange / total) * 100;

            RenderUI(buff, new Vector2(screenPos.X, screenPos.Y), worldPos, scaledAoe, damage, inRangePercent);
        }

        private void RenderUI(Buff buff, Vector2 screenPos, Vector3 worldPos, float aoe, float damage, float inRangePercent)
        {
            if (buff.Timer <= 0 || buff.MaxTime <= 0 || Graphics == null) return;

            float percent = (buff.Timer / buff.MaxTime) * 100;
            var color = GetBarColor(percent);

            Graphics.DrawCircleInWorld(worldPos, aoe, color, 2, 48);

            // Draw UI elements
            var offeringText = "Offering: ";
            var timerText = $"{buff.Timer:F1}s";
            var dmgText = "Dmg";
            var dmgValueText = $"+{damage}%";
            var rangeText = $"Buffed: {inRangePercent:F1}%";

            var offeringTextSize = Graphics.MeasureText(offeringText);
            var timerTextSize = Graphics.MeasureText(timerText);
            var dmgTextSize = Graphics.MeasureText(dmgText);
            var dmgValueTextSize = Graphics.MeasureText(dmgValueText);
            var rangeTextSize = Graphics.MeasureText(rangeText);

            float barWidth = offeringTextSize.X + timerTextSize.X;
            float barHeight = 10;
            Vector2 barPos = new Vector2(screenPos.X - barWidth / 2, screenPos.Y - barHeight / 2);

            DrawTexts(screenPos, barPos, barWidth, barHeight, percent, color, offeringText, timerText, dmgText, dmgValueText, rangeText, offeringTextSize, timerTextSize, dmgTextSize, dmgValueTextSize, rangeTextSize);
        }

        private void DrawTexts(Vector2 screenPos, Vector2 barPos, float barWidth, float barHeight, float fillRatio, Color barColor, string offeringText, string timerText, string dmgText, string dmgValueText, string rangeText, Vector2 offeringTextSize, Vector2 timerTextSize, Vector2 dmgTextSize, Vector2 dmgValueTextSize, Vector2 rangeTextSize)
        {
            Graphics.DrawTextWithBackground(offeringText, new Vector2(barPos.X, screenPos.Y - barHeight - timerTextSize.Y), Color.White, Color.Black);
            Graphics.DrawTextWithBackground(timerText, new Vector2(barPos.X + offeringTextSize.X, screenPos.Y - barHeight - timerTextSize.Y), barColor, Color.Black);
            Graphics.DrawTextWithBackground(dmgText, new Vector2(barPos.X - dmgTextSize.X - 5, barPos.Y + (barHeight - dmgTextSize.Y) / 2), Color.White, Color.Black);
            Graphics.DrawTextWithBackground(dmgValueText, new Vector2(barPos.X + barWidth + 5, barPos.Y + (barHeight - dmgValueTextSize.Y) / 2), Color.White, Color.Black);
            Graphics.DrawTextWithBackground(rangeText, new Vector2(screenPos.X - rangeTextSize.X / 2, barPos.Y + barHeight + 5), Color.White, Color.Black);

            Graphics.DrawBox(barPos, new Vector2(barPos.X + barWidth, barPos.Y + barHeight), Color.Black);
            Graphics.DrawBox(barPos, new Vector2(barPos.X + barWidth * fillRatio / 100, barPos.Y + barHeight), barColor);
        }

        private Color GetBarColor(float percent) => percent >= 66.67 ? Settings.PainOfferingHighColor.Value : percent >= 33.33 ? Settings.PainOfferingMediumColor.Value : Settings.PainOfferingLowColor.Value;

        private void RenderGrimFeast()
        {
            var effects = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Effect]
                .Where(e => e.GetComponent<Animated>()?.BaseAnimatedObjectEntity?.Path?.Contains("grimFeast") == true);

            foreach (var effect in effects)
            {
                var pos = effect.Pos;
                pos.Z -= 85;
                Graphics?.DrawCircleInWorld(pos, 25, Settings.GrimFeastColor.Value, 2, 48);
            }
        }
    }
}
