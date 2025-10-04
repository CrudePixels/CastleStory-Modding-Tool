using System.Windows.Media;

namespace CastleStoryLauncher
{
    public class FactionColor
    {
        public string Name { get; set; } = "";
        public Color Color { get; set; } = Colors.White;
        public string HexValue => $"#{Color.R:X2}{Color.G:X2}{Color.B:X2}";

        public FactionColor()
        {
        }

        public FactionColor(string name, Color color)
        {
            Name = name;
            Color = color;
        }

        public override string ToString()
        {
            return $"{Name} ({HexValue})";
        }

        public override bool Equals(object? obj)
        {
            if (obj is FactionColor other)
            {
                return Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase) && 
                       Color.Equals(other.Color);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name.ToLowerInvariant(), Color);
        }
    }
}
