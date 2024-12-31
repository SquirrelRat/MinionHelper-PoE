using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExileCore2;
using ExileCore2.PoEMemory.Components;
using ExileCore2.PoEMemory.MemoryObjects;
using ExileCore2.Shared.Enums;
using ExileCore2.Shared.Nodes;
using System.Numerics;
using System.Drawing;

namespace MinionHelper
{
    public class MinionHelper : BaseSettingsPlugin<MinionHelperSettings>
    {
        public override bool Initialise()
        {
            return true;
        }

        public override void Render()
        {
            if (!Settings.Enable) return;

            var entities = GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Monster];

            foreach (var entity in entities)
            {
                if (entity.IsAlive && entity.Metadata == "Metadata/Monsters/OfferingSpike/PainOfferingSpike")
                {
                    var statsComponent = entity.GetComponent<Stats>();

                    if (statsComponent != null && statsComponent.StatDictionary.TryGetValue(GameStat.ActiveSkillAreaOfEffectRadius, out var aoeValue))
                    {
                        float scaledAoeValue = aoeValue * 10;

                        var worldPos = entity.Pos;
                        var screenPos2D = GameController.IngameState.Camera.WorldToScreen(worldPos);
                        var screenPos = new Vector2(screenPos2D.X, screenPos2D.Y);

                        if (Graphics != null)
                        {
                            var buffsComponent = entity.GetComponent<Buffs>();
                            if (buffsComponent != null)
                            {
                                int offset = 0;
                                foreach (var buff in buffsComponent.BuffsList)
                                {
                                    if (buff.Name != "pain_offering_buff") continue;

                                    var timeLeft = buff.Timer;
                                    var totalTime = buff.MaxTime;
                                    var buffText = $"Offering: {timeLeft:F1}s";

                                    var baseScreenPos = new Vector2(screenPos.X, screenPos.Y);

                                    if (totalTime <= 0 || timeLeft <= 0)
                                    {
                                        continue;
                                    }

                                    var percentageLeft = (timeLeft / totalTime) * 100;
                                    Color barColor;
                                    if (percentageLeft >= 66.67)
                                    {
                                        barColor = Settings.PainOfferingHighColor.Value;
                                    }
                                    else if (percentageLeft >= 33.33)
                                    {
                                        barColor = Settings.PainOfferingMediumColor.Value;
                                    }
                                    else
                                    {
                                        barColor = Settings.PainOfferingLowColor.Value;
                                    }

                                    Graphics.DrawCircleInWorld(worldPos, scaledAoeValue, barColor, 2, 48);

                                    var textSize = Graphics.MeasureText(buffText);
                                    var barWidth = textSize.X;
                                    var barHeight = 10;
                                    var filledWidth = (timeLeft / totalTime) * barWidth;

                                    var barPos = new Vector2(baseScreenPos.X - barWidth / 2, baseScreenPos.Y - barHeight / 2);

                                    Graphics.DrawBox(barPos, new Vector2(barPos.X + barWidth, barPos.Y + barHeight), Color.Black);
                                    Graphics.DrawBox(barPos, new Vector2(barPos.X + filledWidth, barPos.Y + barHeight), barColor);

                                    var textPos = new Vector2(baseScreenPos.X - barWidth / 2, baseScreenPos.Y - barHeight - textSize.Y - offset);

                                    Graphics.DrawTextWithBackground(buffText, textPos, Color.White, Color.Black);

                                    offset += 5;
                                }
                            }
                        }
                    }
                }
            }

            DrawGrimFeastEffects();
        }

        private void DrawGrimFeastEffects()
        {
            foreach (var entity in GameController.EntityListWrapper.ValidEntitiesByType[EntityType.Effect])
            {
                var animatedComponent = entity.GetComponent<Animated>();
                if (animatedComponent == null) continue;

                var baseAnimatedObjectEntity = animatedComponent.BaseAnimatedObjectEntity;
                if (baseAnimatedObjectEntity?.Path == null) continue;

                if (!baseAnimatedObjectEntity.Path.Contains("grimFeast")) continue;

                var worldPos = entity.Pos;

                worldPos.Z += -85;

                if (Graphics != null)
                {
                    float scaledAoeValue = 25;

                    Graphics.DrawCircleInWorld(worldPos, scaledAoeValue, Settings.GrimFeastColor.Value, 2, 48);
                }
            }
        }
    }
}