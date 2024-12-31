using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using Newtonsoft.Json;
using System.Drawing;

namespace MinionHelper
{
    public class MinionHelperSettings : ISettings
    {
        public ToggleNode Enable { get; set; } = new ToggleNode(true);

        [Menu("Pain Offering High Duration Color (>66%)")]
        public ColorNode PainOfferingHighColor { get; set; } = new ColorNode(Color.Green);

        [Menu("Pain Offering Medium Duration Color (33-66%)")]
        public ColorNode PainOfferingMediumColor { get; set; } = new ColorNode(Color.Yellow);

        [Menu("Pain Offering Low Duration Color (<33%)")]
        public ColorNode PainOfferingLowColor { get; set; } = new ColorNode(Color.Red);

        [Menu("Grim Feast Circle Color")]
        public ColorNode GrimFeastColor { get; set; } = new ColorNode(Color.Aqua);
    }
}